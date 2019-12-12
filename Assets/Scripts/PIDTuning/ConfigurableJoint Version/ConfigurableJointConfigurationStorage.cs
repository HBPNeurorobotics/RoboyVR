using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
/// <summary>
/// Adaption of PidConfigurationStorage for ConfigurableJoint tuning.
/// </summary>
public class ConfigurableJointConfigurationStorage : MonoBehaviour
{

    public ConfigurableJointConfiguration configuration { get; private set; }

    [SerializeField]
    private RigAngleTracker _userAvatar;

    [SerializeField]
    private UserAvatarService _userAvatarService;

    private void Awake()
    {
        Assert.IsNotNull(_userAvatar);
        Assert.IsNotNull(_userAvatarService);

        configuration = new ConfigurableJointConfiguration(DateTime.UtcNow);

        configuration.InitializeMapping(_userAvatar._avatarManager.GetGameObjectPerBoneTargetDictionary().Keys,
                                        new JointSettings(_userAvatarService.initialAngularXDriveSpring,
                                                          _userAvatarService.initialAngularXDriveDamper,
                                                          _userAvatarService.initialAngularYZDriveSpring,
                                                          _userAvatarService.initialAngularYZDriveDamper)
                                        );
    }

    /// <summary>
    /// Transmit the given ConfigurableJoint configuration to the simulation. If no config is provided as
    /// an argument, the current value of the Configuration property of this component
    /// will be transmitted.
    /// </summary>
    public void TransmitFullConfiguration()
    {
        AssertServiceReady();

        foreach (HumanBodyBones bone in configuration.mapping.Keys)
        {
            TransmitSingleJointConfiguration(bone, configuration.mapping[bone]);
            /*
            string topic = "/" + _userAvatarService.avatar_name + "/avatar_ybot/" + joint.Key + "/set_pid_params";

            ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(joint.Value.Kp, joint.Value.Ki, joint.Value.Kd));
            */
        }
    }

    public void TransmitSingleJointConfiguration(HumanBodyBones bone, JointSettings settings)
    {
        AssertServiceReady();
        configuration.mapping[bone] = settings;
        /*
        string topic = "/" + _userAvatarService.avatar_name + "/avatar_ybot/" + bone + "/set_pid_params";
        ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(jointConfig.Kp, jointConfig.Ki, jointConfig.Kd));
        */
    }

    public void ResetConfiguration(JointSettings newSettings)
    {
        // We need to enumerate the key collection here since lazy evaluation would fail during the call to
        // InitializeMapping, since it modifies the underlying collection
        var jointNames = configuration.mapping.Keys.ToArray();

        configuration.InitializeMapping(jointNames, newSettings);
        TransmitFullConfiguration();
    }

    private void AssertServiceReady()
    {
        Assert.IsNotNull(configuration);

        Assert.IsTrue(_userAvatarService.IsRemoteAvatarPresent, "Cannot transmit joint config when remote avatar is not present. Did you forget to spawn it?");
    }
}
