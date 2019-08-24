using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PIDTuning
{
    public class TestRunner : MonoBehaviour
    {
        public enum TestRunnerState
        {
            NotReady, // A dependecy is not ready
            Ready,
            RunningTest,
            FinishedTest
        }

        public TestRunnerState State { private set; get; }

        private void OnEnable()
        {
            State = TestRunnerState.NotReady;
        }

        public IEnumerator RunTest()
        {
            State = TestRunnerState.RunningTest;

            yield return new WaitForSeconds(3f); // TODO: Actually do work...

            State = TestRunnerState.FinishedTest;
        }

        public void Reset()
        {
            // TODO: Actually reset

            State = TestRunnerState.Ready;
        }
    }
}