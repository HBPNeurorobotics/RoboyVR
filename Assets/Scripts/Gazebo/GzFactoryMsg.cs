using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using SimpleJSON;

public class GzFactoryMsg : ROSBridgeMsg {


    #region PRIVATE_MEMBER_VARIABLES

    private string _name;
    private string _type;
    private int _createEntity = 1;
    private PointMsg _position;
    private QuaternionMsg _orientation;
    

    #endregion //PRIVATE_MEMBER_VARIABLES


    #region PUBLIC_METHODS

    public GzFactoryMsg(JSONNode msg)
    {
        _name = msg["name"];
        _type = msg["type"];
        _position = new PointMsg(msg["position"]);
        _orientation = new QuaternionMsg(msg["orientation"]);
    }

    public GzFactoryMsg(string name, string sdfType, PointMsg position, QuaternionMsg orientation)
    {
        _name = name;
        _type = sdfType;
        _position = position;
        _orientation = orientation;
    }

    public static string GetMessageType()
    {
        return "gazebo.msgs.Factory";
    }

    public string GetName()
    {
        return _name;
    }

    public string GetSDFType()
    {
        return _type;
    }

    public PointMsg GetPosition()
    {
        return _position;
    }

    public QuaternionMsg GetOrientation()
    {
        return _orientation;
    }

    public override string ToString()
    {
        return "Factory [name=" + _name + ", type=" + _type + ", createEntity=" + _createEntity + ", position=" + _position.ToString() + ", orientation=" + _orientation.ToString() + "]";
    }

    public override string ToYAMLString()
    {
        return "{\"name\": \"" + _name + "\" ,\"type\":\"" + _type + "\",\"createEntity\":" + _createEntity + ",\"position\":" + _position.ToYAMLString() + ",\"orientation\":" + _orientation.ToYAMLString() + "}";
    }

    #endregion //PUBLIC_METHODS


}
