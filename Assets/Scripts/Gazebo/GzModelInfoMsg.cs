using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using ROSBridgeLib;

public class GzModelInfoMsg : ROSBridgeMsg
{

    #region PUBLIC_MEMBER_VARIABLES

    public JSONNode MsgJSON
    {
        get
        {
            return this.msg_json_;
        }

        set
        {
            this.msg_json_ = value;
        }
    }

    #endregion //PUBLIC_MEMBER_VARIABLES

    #region PRIVATE_MEMBER_VARIABLES

    private JSONNode msg_json_;

    #endregion //PRIVATE_MEMBER_VARIABLES

    #region MONOBEHAVIOR_METHODS
    #endregion //MONOBEHAVIOR_METHODS

    #region PUBLIC_METHODS

    public GzModelInfoMsg(JSONNode msg)
    {
        this.msg_json_ = msg;
    }

    public static string GetMessageType()
    {
        return "gazebo.msgs.Model";
    }

    public override string ToString()
    {
        return "gazebo.msgs.Model [name =" + this.msg_json_["name"].ToString() + "]";
    }

    public override string ToYAMLString()
    {
        return "{}";
    }

    #endregion //PUBLIC_METHODS

    #region PRIVATE_METHODS
    #endregion //PRIVATE_METHODS
}
