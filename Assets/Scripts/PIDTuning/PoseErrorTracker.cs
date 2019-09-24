using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    public class PoseErrorTracker : MonoBehaviour
    {
        [SerializeField]
        private UserAvatarService _userAvatarService;

        private Dictionary<string, >

        /// <summary>
        /// Returns the a pair [input, output] (in that order!) of a control loop
        /// </summary>
        public KeyValuePair<float, float> GetCurrentStepDataForJoint(string jointName)
        {
            throw new NotImplementedException();
        }

        private void OnEnable()
        {
            Assert.IsNotNull(_userAvatarService);
            Assert.IsNotNull(_userAvatarService.avatar_rig);

            _userAvatarService.OnJointDictionaryReady += AcquireJointMapping;
        }

        private void OnDisable()
        {
            if (null != _userAvatarService)
            {
                _userAvatarService.OnJointDictionaryReady -= AcquireJointMapping;
            }  
        }
    }
}