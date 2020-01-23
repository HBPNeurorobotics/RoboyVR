using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    public class PoseErrorTracker : MonoBehaviour
    {
        [SerializeField] public RigAngleTracker LocalRig;
        
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
            Assert.IsNotNull(UserAvatarService.Instance);
            Assert.IsNotNull(LocalRig);

            UserAvatarService.Instance.OnAvatarSpawned += AddTrackerToAvatar;
        }

        private void OnDisable()
        {
            if (null != UserAvatarService.Instance)
            {
                UserAvatarService.Instance.OnAvatarSpawned -= AddTrackerToAvatar;
            }  
        }

        private void AddTrackerToAvatar(UserAvatarService avatarService)
        {
            // This thing allows us to track all joint angles on the remote
            // avatar. We do the same thing with the local avatar, so we
            // can just calculate the difference to get the pose error.
            if (UserAvatarService.Instance.use_gazebo)
            {
                RemoteRig = avatarService.avatar.AddComponent<RigAngleTracker>();
            }
            else
            {
                RemoteRig = GameObject.Find("remote_avatar").AddComponent<RigAngleTracker>();
            }
        }

        public IEnumerable<string> GetJointNames()
        {
            return LocalRig.GetJointToRadianMapping().Keys;
        }
    }
}