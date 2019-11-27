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

        public RigAngleTracker LocalRig { private set; get; }
        public RigAngleTracker RemoteRig { private set; get; }

        /// <summary>
        /// Returns the a pair [input, output] (in that order!) of a control loop
        /// </summary>
        public PidStepDataEntry GetCurrentStepDataForJoint(string jointName)
        {
            var input = LocalRig.GetJointToRadianMapping()[jointName];
            var output = RemoteRig.GetJointToRadianMapping()[jointName];

            return PidStepDataEntry.FromRadians(input, output);
        }

        private void OnEnable()
        {
            Assert.IsNotNull(_userAvatarService);
            Assert.IsNotNull(_userAvatarService.avatar_rig);

            LocalRig = _userAvatarService.avatar_rig.GetComponent<RigAngleTracker>();
            Assert.IsNotNull(LocalRig);

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
            RemoteRig = avatarService.avatar.AddComponent<RigAngleTracker>();
        }

        public IEnumerable<string> GetJointNames()
        {
            return LocalRig.GetJointToRadianMapping().Keys;
        }
    }
}