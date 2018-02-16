using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.gazebo_msgs;

public class ROSAvatarResetPublisher : ROSBridgePublisher {

    public new static string GetMessageTopic()
    {
        return "/gazebo/set_model_state";
    }

    public new static string GetMessageType()
    {
        return "gazebo_msgs/ModelState";
    }

    public static string ToYAMLString(ModelStateMsg msg)
    {
        return msg.ToYAMLString();
    }
}
