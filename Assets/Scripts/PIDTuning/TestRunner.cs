using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        [SerializeField]
        private UserAvatarService _avatarService;

        /// <summary>
        /// This is the cached backing field to _testAnimationStateNames
        /// </summary>
        private List<string> _testAnimationStateList;

        /// <summary>
        /// Holds a mapping of animation name -> joint name -> step-data of the most recently performed test
        /// </summary>
        private Dictionary<string, Dictionary<string, PidStepData>> _latestAnimationToJointToStepData;

        /// <summary>
        /// Holds the PID config that was used in the most recently performed test
        /// </summary>
        private PidConfiguration _latestPidConfiguration;

        private void OnEnable()
        {
            State = TestRunnerState.NotReady;

            Assert.IsNotNull(_avatarService);

            _testEnvSetup = GetComponent<TestEnvSetup>();
            _animatorControl = GetComponent<AnimatorControl>();
            _poseErrorTracker = GetComponent<PoseErrorTracker>();

            _testAnimationStateList = ParseTestAnimationStatesInput();

            _avatarService.OnAvatarSpawned += ResetTestRunner;
        }

        private void OnDisable()
        {
            if (null != _avatarService)
            {
                _avatarService.OnAvatarSpawned -= ResetTestRunner;
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
            Assert.AreEqual(State, TestRunnerState.Ready);
            Assert.AreEqual(_latestPidConfiguration, null);
            Assert.AreEqual(_latestAnimationToJointToStepData, null);

            State = TestRunnerState.RunningTest;

            // Prepare Simulation and Data Structures
            // -----------------------------------------------------------------------------------

            var testRunTimeStamp = DateTime.UtcNow;

            // TODO: The PID config should come from the user, but for now we are just going to instantiate it here
            var pidConfig = new PidConfiguration("pid-conf", testRunTimeStamp);

            yield return StartCoroutine(_testEnvSetup.TransmitPidConfiguration(pidConfig));

            // Run Simulation Loop and record data
            // -----------------------------------------------------------------------------------

            var _tempAnimationToJointToStepData =  new Dictionary<string, Dictionary<string, PidStepData>>();

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
                    var sd = new PidStepData(animation + "-step-data", testRunTimeStamp);
                    sd.AdditionalKeys["animation"] = animation;
                    sd.AdditionalKeys["joint"] = joint;
                }   

                // Play animation and record samples during playback
                // -----------------------------------------------------------------------------------

                _animatorControl.PlayAnimation(animation);

                while (_animatorControl.IsAnimationRunning)
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

                        stepData[joint].Data.Add(
                            frameTimestamp, 
                            entry);
                    }

                    // Wait for next frame 
                    yield return null;
                }

                _tempAnimationToJointToStepData[animation] = stepData;
            }

            // Set member variables to allow access to the recorded data
            // -----------------------------------------------------------------------------------

            _latestAnimationToJointToStepData = _tempAnimationToJointToStepData;
            _latestPidConfiguration = pidConfig;

            State = TestRunnerState.FinishedTest;
        }

        // We need the parameter here to be able to subscribe to OnAvatarSpawned without
        // having to write a wrapper function. No, we also wont use lambdas, as they 
        // are a pain to unsubscribe
        public void ResetTestRunner(UserAvatarService _ = null)
        {
            // TODO: Actually reset. Also test if the results have been saved. Actually, prefer to do that in the Editor UI
            _latestAnimationToJointToStepData = null;
            _latestPidConfiguration = null;

            State = TestRunnerState.Ready;
        }
    }
}