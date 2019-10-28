using System;
using System.Collections;
using System.Collections.Generic;
using ROSBridgeLib.geometry_msgs;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    /// <summary>
    /// Fo now, this component seems pretty useless since our test environment is empty,
    /// except for the robot. In the future, you might want to control the test environ-
    /// ment from this class.
    /// </summary>
    public class TestEnvSetup : MonoBehaviour
    {
        public float PoseResetTimeEstimate = 8f;

        public IEnumerator RunSimulationReset()
        {
            // For now, let's actually just wait a few seconds until the avatar has settled
            yield return new WaitForSeconds(PoseResetTimeEstimate);
        }
    }
}