using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;

public class ROSAvatarVelPublisher : ROSBridgePublisher {

    public static string _avatarId = "";

    public static void setAvatarId(string id)
    {
        _avatarId = id;
    }

    public new static string GetMessageTopic()
    {
        return "/user_avatar_" + _avatarId + "/user_avatar_basic/body/cmd_vel";
    }

    public new static string GetMessageType()
    {
        return "geometry_msgs/Vector3";
    }

    public static string ToYAMLString(Vector3Msg msg)
    {
        return msg.ToYAMLString();
    }
}
