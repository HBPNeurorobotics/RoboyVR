using UnityEngine;
using ROSBridgeLib;
using SimpleJSON;

public class GzRequestSubscriber : ROSBridgeSubscriber
{
    #region PUBLIC_MEMBER_VARIABLES
    #endregion //PUBLIC_MEMBER_VARIABLES

    #region PRIVATE_MEMBER_VARIABLES
    #endregion //PRIVATE_MEMBER_VARIABLES

    #region MONOBEHAVIOR_METHODS
    #endregion //MONOBEHAVIOR_METHODS

    #region PUBLIC_METHODS

    public new static string GetMessageTopic()
    {
        return "~/request";
    }

    public new static string GetMessageType()
    {
        return "gazebo.msgs.Request";
    }

    public new static ROSBridgeMsg ParseMessage(JSONNode msg)
    {
        //Debug.Log("GzRequestSubscriber.ParseMessage");
        return new GzRequestMsg(msg);
    }

    public new static void CallBack(ROSBridgeMsg msg)
    {
        GzBridgeService.Instance.ReceiveMessage((GzRequestMsg)msg);
    }

    #endregion //PUBLIC_METHODS

    #region PRIVATE_METHODS

    #endregion //PRIVATE_METHODS
}
