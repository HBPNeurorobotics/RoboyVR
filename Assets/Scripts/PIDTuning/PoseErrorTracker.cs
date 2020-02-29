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
        [SerializeField] public GameObject RemoteNonGazeboAvatar;
        
        public RigAngleTracker RemoteRig { private set; get; }

        private void OnEnable()
        {
            Assert.IsNotNull(UserAvatarService.Instance);
            Assert.IsNotNull(LocalRig);

            if (UserAvatarService.Instance.use_gazebo)
            {
                UserAvatarService.Instance.OnAvatarSpawned += AddTrackerToAvatar;
            }
            else
            {
                RemoteRig = RemoteNonGazeboAvatar.AddComponent<RigAngleTracker>();
            }
        }

        private void OnDisable()
        {
            if (null != UserAvatarService.Instance)
            {
                if (UserAvatarService.Instance.use_gazebo){
                    UserAvatarService.Instance.OnAvatarSpawned -= AddTrackerToAvatar;
                }
                else
                {
                    Destroy(RemoteRig);
                }
            }  
        }
        
        /// <summary>
        /// Returns the a pair [input, output] (in that order!) of a control loop
        /// </summary>
        public PidStepDataEntry GetCurrentStepDataForJoint(string jointName)
        {
            var input = LocalRig.GetJointToRadianMapping()[jointName];
            var output = RemoteRig.GetJointToRadianMapping()[jointName];

            return PidStepDataEntry.FromRadians(input, output);
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