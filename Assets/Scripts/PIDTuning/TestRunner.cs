using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using WebSocketSharp;

namespace PIDTuning
{
    [RequireComponent(typeof(TestEnvSetup), typeof(AnimatorControl), typeof(PoseErrorTracker))]
    public class TestRunner : MonoBehaviour
    {
        private TestEnvSetup _testEnvSetup;
        private AnimatorControl _animatorControl;
        private PoseErrorTracker _poseErrorTracker;
        public PidConfigurationStorage PidConfigurationStorage { private set; get; }

        public enum TestRunnerState
        {
            NotReady, // = A dependency is not ready
            Ready,
            RunningTest,
            FinishedTest
        }
         
        public TestRunnerState State { private set; get; }

        public string CurrentTestLabel = "Unnamed Test";

        /// <summary>
        /// The User Avatar animator component must contain all animation states that are listed here.
        /// The test runner will go through these states from top to bottom. Each state will be recorded
        /// into its own PidStepData instance to avoid lingering effects between animations.
        /// Changes to this field during runtime have no effect since the animation states are cached internally.
        /// </summary>
        [Multiline]
        [SerializeField]
        private string _testAnimationStateNames;

        [SerializeField] private float _minSampleIntervalSeconds = 1f/ 60f;

        /// <summary>
        /// This is the cached backing field to _testAnimationStateNames
        /// </summary>
        private List<string> _testAnimationStateList;

        /// <summary>
        /// Timestamp of the most recently performed recording
        /// </summary>
        private DateTime? _latestTestTimestamp;

        /// <summary>
        /// Holds a mapping of animation name -> joint name -> step-data of the most recently performed recording
        /// </summary>
        private Dictionary<string, Dictionary<string, PidStepData>> _latestAnimationToJointToStepData;

        /// <summary>
        /// Holds the PID config that was used in the most recently performed recording
        /// </summary>
        private PidConfiguration _latestPidConfiguration;

        /// <summary>
        /// Holds a mapping of animation name -> joint name -> evaluation of the most recently performed recording
        /// </summary>
        public Dictionary<string, Dictionary<string, PerformanceEvaluation>> LatestAnimationToJointToEvaluation;

        /// <summary>
        /// Holds a mapping of the cumulative performance evaluation of all joints for a given animation in the last test run
        /// </summary>
        public Dictionary<string, PerformanceEvaluation> LatestAnimationToEvaluation;

        /// <summary>
        /// Holds a cumulative performance evaluation for all animation combined in the latest test run
        /// </summary>
        public PerformanceEvaluation LatestEvaluation;

        private bool _isRunningManualRecord = false;

        private void OnEnable()
        {
            State = TestRunnerState.NotReady;

            Assert.IsNotNull(UserAvatarService.Instance);

            Assert.IsNotNull(_testEnvSetup = GetComponent<TestEnvSetup>());
            Assert.IsNotNull(_animatorControl = GetComponent<AnimatorControl>());
            Assert.IsNotNull(_poseErrorTracker = GetComponent<PoseErrorTracker>());
            Assert.IsNotNull(PidConfigurationStorage = GetComponent<PidConfigurationStorage>());

            _testAnimationStateList = ParseTestAnimationStatesInput();

            UserAvatarService.Instance.OnAvatarSpawned += ResetTestRunner;
            if (!UserAvatarService.Instance.use_gazebo) ResetTestRunner();
        }

        private void OnDisable()
        {
            if (null != UserAvatarService.Instance)
            {
                UserAvatarService.Instance.OnAvatarSpawned -= ResetTestRunner;
            }
        }

        private List<string> ParseTestAnimationStatesInput()
        {
            var list = new List<string>();

            // Transform multiline string to separate lines in the safest way possible
            var sr = new StringReader(_testAnimationStateNames);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (!line.IsNullOrEmpty())
                {
                    list.Add(line);
                }
            }

            // Validate that all states exist in the animator
            _animatorControl.ValidateStateList(list);

            return list;
        }

        public IEnumerator RunTest()
        {
            AssertReadyForTest();

            Debug.Log("Running Test...");

            State = TestRunnerState.RunningTest;

            // Prepare Simulation and Data Structures
            // -----------------------------------------------------------------------------------

            var testRunTimeStamp = DateTime.UtcNow;

            // We copy the current PID configuration here so that the user cannot accidentally modify it during the test.
            // (They still can do that if they transmit a new configuration, but in that case that's their own fault.)
            var testRunPidConfig = new PidConfiguration(PidConfigurationStorage.Configuration);

            if (!UserAvatarService.Instance.use_gazebo)
            {
                PidConfiguration config = new PidConfiguration(DateTime.UtcNow);
                foreach (var joint in _poseErrorTracker.GetJointNames())
                {
                    ConfigurableJoint configurableJoint = LocalPhysicsToolkit.GetRemoteJointOfCorrectAxisFromString(joint, UserAvatarService.Instance._avatarManager.GetGameObjectPerBoneRemoteAvatarDictionary());
                    config.Mapping.Add(joint, PidParameters.FromParallelForm(configurableJoint.angularXDrive.positionSpring, 0, configurableJoint.angularXDrive.positionDamper));
                }

                testRunPidConfig = config;
            }
            else
            { 
                PidConfigurationStorage.TransmitFullConfiguration();
            }

            

            // Run Simulation Loop and record data
            // -----------------------------------------------------------------------------------

            var _tempAnimationToJointToStepData = new Dictionary<string, Dictionary<string, PidStepData>>();

            foreach (var animation in _testAnimationStateList)
            {
                // Reset & Prepare Simulation and Animator
                // -----------------------------------------------------------------------------------

                _animatorControl.ResetUserAvatar();

                yield return StartCoroutine(_testEnvSetup.RunSimulationReset());

                // Prepare step data collection
                // -----------------------------------------------------------------------------------

                var stepData = new Dictionary<string, PidStepData>();
                foreach (var joint in _poseErrorTracker.GetJointNames())
                {
                    var sd = new PidStepData(testRunTimeStamp);
                    sd.AdditionalKeys["animation"] = animation;
                    sd.AdditionalKeys["joint"] = joint;
                    sd.AdditionalKeys["simulationTimeStretchFactor"] = _animatorControl.TimeStretchFactor.ToString(CultureInfo.InvariantCulture);

                    stepData[joint] = sd;
                }

                // Play animation and record samples during playback
                // -----------------------------------------------------------------------------------

                yield return _animatorControl.StartAnimationPlayback(animation);

                // Skip first second of animation to allow the model to assume the correct initial pose.
                // Ideally, we should wait until the robot has settled into the pose that is present in the
                // first frame of the animation, but that is kinda difficult...
                yield return new WaitForSeconds(1f);

                yield return RecordMotion(() => _animatorControl.IsAnimationRunning, stepData);

                _tempAnimationToJointToStepData[animation] = stepData;
            }

            // Reset user avatar pose at the very end
            _animatorControl.ResetUserAvatar();

            // Set member variables to allow access to the recorded data
            // -----------------------------------------------------------------------------------

            _latestTestTimestamp = testRunTimeStamp;
            _latestAnimationToJointToStepData = _tempAnimationToJointToStepData;
            _latestPidConfiguration = testRunPidConfig;
            CalculatePerformanceEvaluation(testRunTimeStamp);

            State = TestRunnerState.FinishedTest;

            Debug.Log("Test Finished");
        }

        private void AssertReadyForTest()
        {
            string errorMessage =
                "TestRunner was not properly reset before starting the next test. Please call \"ResetTestRunnre\" after each test";

            Assert.AreEqual(State, TestRunnerState.Ready, errorMessage);
            Assert.AreEqual(_latestTestTimestamp, null, errorMessage);
            Assert.AreEqual(_latestPidConfiguration, null, errorMessage);
            Assert.AreEqual(_latestAnimationToJointToStepData, null, errorMessage);
            Assert.AreEqual(LatestAnimationToJointToEvaluation, null, errorMessage);
            Assert.AreEqual(LatestAnimationToEvaluation, null, errorMessage);
            Assert.AreEqual(LatestEvaluation, null, errorMessage);
        }

        /// <summary>
        /// Calculates performance metrics for each PidStepData that we collected in the last test/recording
        /// </summary>
        private void CalculatePerformanceEvaluation(DateTime testRunTimeStamp)
        {
            // If you cannot read this code, I don't blame you. It's much nicer in .NET 4+.
            // In case you really, really hate it, you can rewrite it using nested foreach loops

            LatestAnimationToJointToEvaluation = _latestAnimationToJointToStepData
                .Select(animToJoints => new KeyValuePair<string, Dictionary<string, PerformanceEvaluation>>(animToJoints.Key, animToJoints.Value
                    .Select(jointToStepData => new KeyValuePair<string, PerformanceEvaluation>(jointToStepData.Key, PerformanceEvaluation.FromStepData(testRunTimeStamp, jointToStepData.Value)))
                    .ToDictionary(jointToEvaluation => jointToEvaluation.Key, jointToEvaluation => jointToEvaluation.Value)))
                .ToDictionary(animToJoints => animToJoints.Key, animToJoints => animToJoints.Value);

            LatestAnimationToEvaluation = LatestAnimationToJointToEvaluation
                .Select(animToJoints => new KeyValuePair<string, PerformanceEvaluation>(animToJoints.Key, PerformanceEvaluation.FromCumulative(animToJoints.Value.Select(pair => pair.Value))))
                .ToDictionary(animToJoints => animToJoints.Key, animToJoints => animToJoints.Value);

            LatestEvaluation = PerformanceEvaluation.FromCumulative(LatestAnimationToEvaluation.Select(pair => pair.Value));
        }

        private IEnumerator RecordMotion(Func<bool> shouldContinueRecording, Dictionary<string, PidStepData> jointToStepDataTarget)
        {
            while (shouldContinueRecording())
            {
                // We take the timestamp now to make sure that all step data
                // entries receive a consistent timestamp
                var frameTimestamp = DateTime.UtcNow;

                foreach (var joint in _poseErrorTracker.GetJointNames())
                {
                    var entry = _poseErrorTracker.GetCurrentStepDataForJoint(joint);

                    // Maybe: Add additional keys to the entry here if needed. A good example would be the
                    // total control loop RTT
                    // entry.AddCorrelatedData(...)

                    jointToStepDataTarget[joint].Data.Add(
                        frameTimestamp,
                        entry);
                }

                // Wait for sample interval 
                yield return new WaitForSeconds(_minSampleIntervalSeconds);
            }
        }

        public void StartManualRecord()
        {
            AssertReadyForTest();

            State = TestRunnerState.RunningTest;
            _isRunningManualRecord = true;

            _latestTestTimestamp = DateTime.UtcNow;

            _latestPidConfiguration = new PidConfiguration(DateTime.UtcNow);

            if (!UserAvatarService.Instance.use_gazebo)
            {
                PidConfiguration config = new PidConfiguration(DateTime.UtcNow);
                if (_poseErrorTracker == null) _poseErrorTracker = GetComponent<PoseErrorTracker>();
                foreach (var joint in _poseErrorTracker.GetJointNames())
                {
                    ConfigurableJoint configurableJoint = LocalPhysicsToolkit.GetRemoteJointOfCorrectAxisFromString(joint, UserAvatarService.Instance._avatarManager.GetGameObjectPerBoneRemoteAvatarDictionary());
                    config.Mapping.Add(joint, PidParameters.FromParallelForm(configurableJoint.angularXDrive.positionSpring, 0, configurableJoint.angularXDrive.positionDamper));
                }

                _latestPidConfiguration = config;
            }
            else
            {
                _latestPidConfiguration = new PidConfiguration(PidConfigurationStorage.Configuration);
            }
            // Prepare step data target dictionary

            _latestAnimationToJointToStepData = new Dictionary<string, Dictionary<string, PidStepData>>();
            _latestAnimationToJointToStepData["recording"] = new Dictionary<string, PidStepData>();

            foreach (var joint in _poseErrorTracker.GetJointNames())
            {
                var sd = new PidStepData(_latestTestTimestamp.Value);
                sd.AdditionalKeys["animation"] = "recording";
                sd.AdditionalKeys["joint"] = joint;

                _latestAnimationToJointToStepData["recording"][joint] = sd;
            }
            StartCoroutine(RecordMotion(() => _isRunningManualRecord, _latestAnimationToJointToStepData["recording"]));
        }

        /// <summary>
        /// Convenience Feature: Returns the evaluations of all joints for the recording.
        /// </summary>
        public Dictionary<string, PidStepData> StopManualRecord()
        {
            Assert.IsTrue(_isRunningManualRecord);
            Assert.AreEqual(State, TestRunnerState.RunningTest);

            CalculatePerformanceEvaluation(_latestTestTimestamp.Value);

            State = TestRunnerState.FinishedTest;
            _isRunningManualRecord = false;

            return _latestAnimationToJointToStepData["recording"];
        }

        // We need the parameter here to be able to subscribe to OnAvatarSpawned without
        // having to write a wrapper function. No, we also wont use lambdas, as they 
        // are a pain to unsubscribe
        public void ResetTestRunner(UserAvatarService _ = null)
        {
            _latestTestTimestamp = null;
            _latestAnimationToJointToStepData = null;
            _latestPidConfiguration = null;
            LatestAnimationToJointToEvaluation = null;
            LatestAnimationToEvaluation = null;
            LatestEvaluation = null;

            State = TestRunnerState.Ready;
        }

        public void SaveTestData(bool isPhysicsTest = false, string folder = "")
        {
            Assert.AreEqual(State, TestRunnerState.FinishedTest);
            string dataPath = Application.dataPath.Replace('/', Path.DirectorySeparatorChar);
            var outputFolder = Path.Combine(dataPath, "PidStepData");
            var testRunFolder = isPhysicsTest ? folder : Path.Combine(outputFolder, CurrentTestLabel + "-" + _latestTestTimestamp.Value.ToFileTimeUtc());

            Directory.CreateDirectory(outputFolder);
            Directory.CreateDirectory(testRunFolder);

            File.WriteAllText(Path.Combine(testRunFolder, "pid-config.json"), _latestPidConfiguration.ToJson().ToString());
            File.WriteAllText(Path.Combine(testRunFolder, "eval.json"), LatestEvaluation.ToJson().ToString());

            foreach (var animation in _latestAnimationToJointToStepData)
            {
                var animationDirectory = Path.Combine(testRunFolder, animation.Key);
                Directory.CreateDirectory(animationDirectory);

                foreach (var joint in animation.Value)
                {
                    File.WriteAllText(Path.Combine(animationDirectory, joint.Key + ".json"), joint.Value.ToJson().ToString());
                    File.WriteAllText(Path.Combine(animationDirectory, joint.Key + "-eval.json"), LatestAnimationToJointToEvaluation[animation.Key][joint.Key].ToJson().ToString());
                }

                File.WriteAllText(Path.Combine(animationDirectory, "eval.json"), LatestAnimationToEvaluation[animation.Key].ToJson().ToString());
                Debug.Log(animationDirectory);
            }
        }
    }
}