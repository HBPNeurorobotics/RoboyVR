using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PIDTuning;
using ROSBridgeLib.geometry_msgs;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;

namespace PIDTuning
{
    /// <summary>
    /// Wraps the PidConfiguration class into a MonoBehaviour so we
    /// can draw a fancy editor for it.
    /// </summary>
    public class PidConfigurationStorage : MonoBehaviour
    {
        string sessionStart = DateTime.Now.ToString();
        public PidConfiguration Configuration { get; private set; }

        [SerializeField]
        private RigAngleTracker _userAvatar;

        private void Awake()
        {
            Assert.IsNotNull(_userAvatar);
            Assert.IsNotNull(UserAvatarService.Instance);

            Configuration = new PidConfiguration(DateTime.UtcNow);

            Configuration.InitializeMapping(_userAvatar.GetJointToRadianMapping().Keys, PidParameters.FromParallelForm(
                UserAvatarService.Instance.InitialP,
                UserAvatarService.Instance.InitialI,
                UserAvatarService.Instance.InitialD));
        }

        /// <summary>
        /// Transmit the given PID configuration to the simulation. If no config is provided as
        /// an argument, the current value of the Configuration property of this component
        /// will be transmitted.
        /// </summary>
        public void TransmitFullConfiguration(bool save = false, float relay = 2000, bool mirror = false)
        {
            AssertServiceReady();
            bool gazebo = UserAvatarService.Instance.use_gazebo;
            Dictionary<string, JointSettings> tunedSettings = new Dictionary<string, JointSettings>();
            foreach (var joint in Configuration.Mapping)
            {
                if (gazebo)
                {
                    string topic = "/" + UserAvatarService.Instance.avatar_name + "/avatar_ybot/" + joint.Key + "/set_pid_params";

                    ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(joint.Value.Kp, joint.Value.Ki, joint.Value.Kd));
                }
                else
                {
                    tunedSettings.Add(joint.Key, TransmitSingleJointConfiguration(joint.Key, relay, mirror));
                }
            }

            if (!gazebo) SafeNonGazeboTuning(tunedSettings);
        }

        void SafeNonGazeboTuning(Dictionary<string, JointSettings> tunedSettings)
        {
            //from EditAvatarTemplate
            string values = ConfigJointUtility.ConvertDictionaryToJson(tunedSettings);
            string path = "Assets/Client Physics/Scripts/Editor/Tuned Settings/";

            path += "tuning_" + sessionStart.Replace('/', '_').Replace(' ', '_').Replace(':', '_') + ".txt";

            File.WriteAllText(path, values);
        }

        public JointSettings TransmitSingleJointConfiguration(string joint, float relay, bool mirror = false)
        {

            AssertServiceReady();
            var jointConfig = Configuration.Mapping[joint];

            if (UserAvatarService.Instance.use_gazebo)
            {
                string topic = "/" + UserAvatarService.Instance.avatar_name + "/avatar_ybot/" + joint + "/set_pid_params";
                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(jointConfig.Kp, jointConfig.Ki, jointConfig.Kd));
                return null;
            }
            else
            {
                ConfigurableJoint configurableJoint = ConfigJointUtility.GetRemoteJointOfCorrectAxisFromString(joint, UserAvatarService.Instance._avatarManager.GetGameObjectPerBoneRemoteAvatarDictionary());

                //Get the joint configuration from the left side for the right side.
                if (mirror && joint.StartsWith("Right"))
                {
                    string mirroredKey = joint.Replace("Right", "Left");
                    jointConfig = Configuration.Mapping[mirroredKey];
                }

                JointDrive angularDrive = new JointDrive();
                angularDrive.positionSpring = jointConfig.Kp;
                angularDrive.positionDamper = jointConfig.Kd;
                angularDrive.maximumForce = relay;
                configurableJoint.angularXDrive = angularDrive;

                return new JointSettings(joint, configurableJoint);
            }
        }

        public void ReplaceWithConfigFromJson(string json)
        {
            var newConfig = PidConfiguration.FromJson(json);

            if (!newConfig.Mapping.Keys.OrderBy(s => s).SequenceEqual(Configuration.Mapping.Keys.OrderBy(s => s)))
            {
                throw new ArgumentException("Joint mappings are not compatible.");
            }

            Configuration = newConfig;
            TransmitFullConfiguration();
        }

        public void ResetConfiguration(float kp, float ki, float kd)
        {
            // We need to enumerate the key collection here since lazy evaluation would fail during the call to
            // InitializeMapping, since it modifies the underlying collection
            var joinNames = Configuration.Mapping.Keys.ToArray();

            Configuration.InitializeMapping(joinNames, PidParameters.FromParallelForm(kp, ki, kd));
            TransmitFullConfiguration();
        }

        private void AssertServiceReady()
        {
            Assert.IsNotNull(Configuration);

            Assert.IsTrue(UserAvatarService.Instance.IsRemoteAvatarPresent, "Cannot transmit PID config when remote avatar is not present. Did you forget to spawn it?");
        }
    }

}