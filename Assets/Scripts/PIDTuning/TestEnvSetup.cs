using System;
using System.Collections;
using System.Collections.Generic;
using ROSBridgeLib.geometry_msgs;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    public class TestEnvSetup : MonoBehaviour
    {
        [SerializeField]
        private UserAvatarService _userAvatarService;

        private void Awake()
        {
            Assert.IsNotNull(_userAvatarService);
        }

        public IEnumerator RunSimulationReset()
        {
            // TODO: Reset Gazebo sim and wait for confirmation
            // For now, let's actually just wait a few seconds until the avatar has settled
            yield return new WaitForSeconds(10f);
        }

        public void TransmitPidConfiguration(PidConfiguration config)
        {
            foreach (var joint in config.Mapping)
            {
                string topic = "/" + _userAvatarService.avatar_name + "/avatar_ybot/" + joint.Key + "/set_pid_params";

                // default was (100f, 50f, 10f)
                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(joint.Value.Kp, joint.Value.Ki, joint.Value.Kd));
            }
        }
    }
}