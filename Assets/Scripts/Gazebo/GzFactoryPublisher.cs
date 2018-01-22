using System;
using UnityEngine;
using SimpleJSON;
using ROSBridgeLib;

public class GzFactoryPublisher : ROSBridgePublisher {

    public new static string GetMessageTopic()
    {
        return "~/factory";
    }

    public new static string GetMessageType()
    {
        return "gazebo.msgs.Factory";
    }

    public static string ToYAMLString(GzFactoryMsg msg)
    {
        return msg.ToYAMLString();
    }
}
