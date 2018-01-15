using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;

public class ROSAvatarRotPublisher : ROSBridgePublisher
{
	public new static string GetMessageTopic()
    {
        return "/user_avatar_default_owner/cmd_rot";
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
