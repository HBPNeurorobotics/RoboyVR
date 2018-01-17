using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using SimpleJSON;

public class GzFactoryMsg : ROSBridgeMsg {

    /**
     * eine entity message:
    var entityMsg =
   {
     name : model.name,  //name in der szene, kann 'Otto' sein
     type : type,  // name des .sdf model files, dass muss 'user_avatar_basic' sein
     createEntity : 1,
     position :
     {
       x : translation.x,
       y : translation.y,
       z : translation.z
     },
     scale :
     {
       x : scale.x,
       y : scale.y,
       z : scale.z
     },
     orientation :
     {
       w: quaternion.w,
       x: quaternion.x,
       y: quaternion.y,
       z: quaternion.z
     }
   };
     **/


    #region PRIVATE_MEMBER_VARIABLES

    private string _name;
    private string _type;
    private int _createEntity;
    private PointMsg _position;
    private PointMsg _scale;
    private QuaternionMsg _orientation;
    

    #endregion //PRIVATE_MEMBER_VARIABLES


    #region PUBLIC_METHODS

    public GzFactoryMsg(JSONNode msg)
    {
        _name = msg["name"];
        _type = msg["type"];
        
        _position = new PointMsg(msg["position"]);
        _scale = new PointMsg(msg["scale"]);
        _orientation = new QuaternionMsg(msg["orientation"]);
    }

    public GzFactoryMsg(string name, string sdfType, PointMsg position, PointMsg scale, QuaternionMsg orientation)
    {
        _name = name;
        _type = sdfType;
        _position = position;
        _scale = scale;
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

    public PointMsg GetScale()
    {
        return _scale;
    }

    public QuaternionMsg GetOrientation()
    {
        return _orientation;
    }

    public override string ToString()
    {
        return "Factory [name=" + _name + ", type=" + _type + ", position=" + _position.ToString() + ", scale=" +_scale.ToString() + ", orientation=" + _orientation.ToString() + "]";
    }

    public override string ToYAMLString()
    {
        return "{\"name\" : " + _name + ", \type\" : " + _type + ", \"position\" : " + _position.ToYAMLString() +  ", \"scale\" : " + _scale.ToYAMLString() + ", \"orientation\" : " + _orientation.ToYAMLString() + "}";
    }

    #endregion //PUBLIC_METHODS


}
