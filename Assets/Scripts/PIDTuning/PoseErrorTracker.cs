using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PIDTuning
{
    public class PoseErrorTracker : MonoBehaviour
    {
        /// <summary>
        /// Returns the a pair [input, output] (in that order!) of a control loop
        /// </summary>
        public KeyValuePair<float, float> GetCurrentStepDataForJoint(string jointName)
        {
            throw new NotImplementedException();
        }
    }
}