using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using SimpleJSON;

public class GzFactoryMsg2 : ROSBridgeMsg
{
    /**
     * package gazebo.msgs;

    /// \ingroup gazebo_msgs
    /// \interface Factory
    /// \brief Message to create new entities in gazebo, at a given pose.
    /// A model can be created in one of the following ways:
    ///
    /// 1. From an SDF string (sdf field)
    /// 2. From an SDF file (sdf_filename)
    /// 3. Cloning an existing model (clone_model_name)
    ///
    /// If more than one way is specified, the first field will be parsed and the
    /// following ignored.
    ///
    /// The message can also be used to edit an existing entity. The new entity
    /// description is pushed into the entity named `edit_name`.
    /// See issue #1954 for the current limitations using this method to edit
    /// entities.

    import "pose.proto";

    message Factory
    {
    /// \brief SDF description in string format.
    optional string sdf                       = 1;

    /// \brief Full path to SDF file.
    optional string sdf_filename              = 2;

    /// \brief Pose where the entity will be spawned.
    optional Pose pose                        = 3;

    /// \brief Name of the entity which will be updated.
    optional string edit_name                 = 4;

    /// \brief Name of model to clone.
    optional string clone_model_name          = 5;
    }
     **/

    #region PRIVATE_MEMBER_VARIABLES

    private string _sdfFileName;
    private PoseMsg _pose;


    #endregion //PRIVATE_MEMBER_VARIABLES


    #region PUBLIC_METHODS

    public GzFactoryMsg2(JSONNode msg)
    {
        _sdfFileName = msg["sdf_filename"];
        _pose = new PoseMsg(msg["pose"]);
    }

    public GzFactoryMsg2(string name, PoseMsg pose)
    {
        _sdfFileName = name;
        _pose = pose;
    }

    public static string GetMessageType()
    {
        return "gazebo.msgs.Factory";
    }


    public override string ToString()
    {
        return "Factory [name=" + _sdfFileName + ", pose=" + _pose.ToString() +  "]";
    }

    public override string ToYAMLString()
    {
        return "{\"name\" : " + _sdfFileName + ", \"orientation\" : " + _pose.ToYAMLString() + "}";
    }

    #endregion //PUBLIC_METHODS


}
