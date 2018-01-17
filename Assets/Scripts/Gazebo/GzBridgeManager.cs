using UnityEngine;
using System.Collections.Generic;
using GzBridgeLib;
using SimpleJSON;

/// <summary>
/// Roboymanager has different tasks:
///
/// <b>- Run ROS:</b>
///     -# Connect to the simulation.
///     -# Add subscriber to the pose.
///     -# Add publisher for external force.
///     -# Add service response for world reset.
///
/// <b>- Receive and send ROS messages:</b>
///     -# receive pose msg to adjust roboy pose.
///     -# subscribe to external force event and send msg to simulation.
///     -# send service call for world reset.
///     -# FUTURE: receive motor msg and forward it to the according motors.
/// </summary>
public class GzBridgeManager : Singleton<GzBridgeManager>
{
    #region PUBLIC_MEMBER_VARIABLES

    /// <summary>
    /// The IP address of the VM or the machine where the simulation is running
    /// </summary>
    public string URL = "192.168.0.17:8080/gzbridge";

    public GameObject GazeboScene = null;

    #endregion //PUBLIC_MEMBER_VARIABLES

    #region PRIVATE_MEMBER_VARIABLES

    /// <summary>
    /// ROSBridge websocket
    /// </summary>
    public GzBridgeWebSocketConnection m_GzBridge = null;

    /// <summary>
    /// Variable to check if the ROS connection is working!
    /// </summary>
    private bool m_Initialized = false;

    #endregion //PRIVATE_MEMBER_VARIABLES

    #region MONOBEHAVIOR_METHODS

    /// <summary>
    /// Initialize ROSBridge and roboy parts
    /// </summary>
    void Awake()
    {
        //this.ConnectToGzBridge();
    }

    /// <summary>
    /// Run ROSBridge
    /// </summary>
    void Update()
    {
        if (m_Initialized)
        {
            m_GzBridge.Render();
        }
    }

    /// <summary>
    /// Disconnect from the simulation when Unity is not running.
    /// </summary>
    void OnApplicationQuit()
    {
        if (m_Initialized)
            m_GzBridge.Disconnect();
    }
    #endregion //MONOBEHAVIOR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    /// Main function to receive scene messages from GzBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing scene message.</param>
    public void ReceiveMessage(GzSceneMsg msg)
    {
        GazeboScene.GetComponent<GazeboSceneManager>().OnSceneMsg(msg.MsgJSON);
    }

    /// <summary>
    /// Main function to receive pose info messages from GzBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing pose info message.</param>
    public void ReceiveMessage(GzPoseInfoMsg msg)
    {
        GazeboScene.GetComponent<GazeboSceneManager>().OnPoseInfoMsg(msg.MsgJSON);
    }

    /// <summary>
    /// Main function to receive model info messages from GzBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing model info message.</param>
    public void ReceiveMessage(GzModelInfoMsg msg)
    {
        GazeboScene.GetComponent<GazeboSceneManager>().OnModelInfoMsg(msg.MsgJSON);
    }

    /// <summary>
    /// Main function to receive material messages from GzBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing material message.</param>
    public void ReceiveMessage(GzMaterialMsg msg)
    {
        GazeboScene.GetComponent<GazeboSceneManager>().OnMaterialMsg(msg.MsgJSON);
    }

    public void ConnectToGzBridge()
    {
        if (string.IsNullOrEmpty(URL))
            return;

        if (GazeboScene == null || GazeboScene.GetComponent<GazeboSceneManager>() == null)
            return;

        m_GzBridge = new GzBridgeWebSocketConnection("ws://" + URL);

        // DOES NOT WORK! m_Ros is never null if you call the Constructor, WAIT TILL SIMON IMPLEMENTS UDP BROADCAST WITH ROS CONFIGURATION, GET THE IP ADDRESS FROM THE BROADCAST
        if (m_GzBridge != null)
        {
            //m_GzBridge.AddSubscriber(typeof(GzResponseSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzSceneMsgSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzPoseInfoSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzModelInfoSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzMaterialSubscriber));
            m_GzBridge.AddPublisher(typeof(GzFactoryPublisher));
            m_GzBridge.Connect();
            m_Initialized = true;

            Debug.Log("GzBridge successfully initialized!");
        }
        else
        {
            Debug.LogWarning("GzBridge could not be initialized!");
        }
    }

    #endregion //PUBLIC_METHODS

    #region PRIVATE_METHODS

    #endregion //PRIVATE_METHODS
}
