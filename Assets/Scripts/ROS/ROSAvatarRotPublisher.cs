using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;

public class ROSAvatarRotPublisher : ROSBridgePublisher
{
    private static string _avatarId = "";

    public static void setAvatarId(string id)
    {
        _avatarId = id;
    }

    public new static string GetMessageTopic()
    {
        return "/user_avatar_" + _avatarId + "/cmd_rot";
    }

    public new static string GetMessageType()
    {
        return "geometry_msgs/Quaternion";
    }

    public static string ToYAMLString(QuaternionMsg msg)
    {
        return msg.ToYAMLString();
    }
}
