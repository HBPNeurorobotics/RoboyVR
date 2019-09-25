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

        private RigAngleTracker _localAngleTracker;
        private RigAngleTracker _remoteAngleTracker;

        /// <summary>
        /// Returns the a pair [input, output] (in that order!) of a control loop
        /// </summary>
        public PidStepDataEntry GetCurrentStepDataForJoint(string jointName)
        {
            var input = _localAngleTracker.GetJointToAngleMapping()[jointName];
            var output = _remoteAngleTracker.GetJointToAngleMapping()[jointName];

            Debug.Log(input + " " + output);

            return new PidStepDataEntry(input, output);
        }

        private void OnEnable()
        {
            Assert.IsNotNull(_userAvatarService);
            Assert.IsNotNull(_userAvatarService.avatar_rig);

            _localAngleTracker = _userAvatarService.avatar_rig.GetComponent<RigAngleTracker>();
            Assert.IsNotNull(_localAngleTracker);

            _userAvatarService.OnAvatarSpawned += AddTrackerToAvatar;
        }

        private void OnDisable()
        {
            if (null != _userAvatarService)
            {
                _userAvatarService.OnAvatarSpawned -= AddTrackerToAvatar;
            }  
        }

        private void AddTrackerToAvatar(UserAvatarService avatarService)
        {
            // This thing allows us to track all joint angles on the remote
            // avatar. We do the same thing with the local avatar, so we
            // can just calculate the difference to get the pose error.
            _remoteAngleTracker = avatarService.avatar.AddComponent<RigAngleTracker>();
        }
    }
}