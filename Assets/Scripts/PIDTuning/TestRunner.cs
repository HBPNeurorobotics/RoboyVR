using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using WebSocketSharp;

namespace PIDTuning
{
    [RequireComponent(typeof(TestEnvSetup), typeof(AnimatorControl))]
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

        private List<string> _testAnimationStateList;

        private void OnEnable()
        {
            State = TestRunnerState.NotReady;

            Assert.IsNotNull(_testEnvSetup = GetComponent<TestEnvSetup>());
            Assert.IsNotNull(_animatorControl = GetComponent<AnimatorControl>());
            Assert.IsNotNull(_poseErrorTracker = GetComponent<PoseErrorTracker>());

            _testAnimationStateList = ParseTestAnimationStatesInput();
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

            State = TestRunnerState.RunningTest;

            // Prepare Simulation and Data Structures
            // -----------------------------------------------------------------------------------

            var testRunTimeStamp = DateTime.UtcNow;

            // TODO: The PID config should come from the user, but for now we are just going to instantiate it here
            var pidConfig = new PidConfiguration("pid-conf", testRunTimeStamp);

            yield return StartCoroutine(_testEnvSetup.TransmitPidConfiguration(pidConfig));

            // Run Simulation Loop and record data
            // -----------------------------------------------------------------------------------

            foreach (var animation in _testAnimationStateList)
            {
                // Reset & Prepare Simulation and Animator
                // -----------------------------------------------------------------------------------

                _animatorControl.ResetUserAvatar();

                yield return StartCoroutine(_testEnvSetup.RunSimulationReset());

                // Prepare step data collection
                // -----------------------------------------------------------------------------------

                var stepData = new Dictionary<string, PidStepData>();
                foreach (var joint in _animatorControl.GetJointNames())
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

                    foreach (var joint in _animatorControl.GetJointNames())
                    {
                        var io = _poseErrorTracker.GetCurrentStepDataForJoint(joint);
                        var entry = new PidStepDataEntry(io.Key, io.Value);

                        // TODO: Add additional keys to the entry here if needed. A good example would be the
                        // total control loop RTT
                        // entry.AddCorrelatedData(...)

                        stepData[joint].Data.Add(
                            frameTimestamp, 
                            entry);
                    }

                    // Wait for next frame 
                    yield return null;
                }
            }

            State = TestRunnerState.FinishedTest;
        }

        public void ResetTestRunner()
        {
            // TODO: Actually reset. Also test if the results have been saved. Actually, prefer to do that in the Editor UI

            State = TestRunnerState.Ready;
        }
    }
}