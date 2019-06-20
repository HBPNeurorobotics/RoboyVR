 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GzBridgeLib;
using ROSBridgeLib;
using SimpleJSON;

public class GzBridgeService : Singleton<GzBridgeService> {

    #region PUBLIC_MEMBER_VARIABLES
    
    public GameObject GazeboScene = null;

    public GzBridgeWebSocketConnection gzbridge
    {
        get { return this.m_GzBridge; }
    }

    public delegate void CallbackOnCloseConnection();

    public delegate void CallbackMaterialMsg(GzMaterialMsg msg);
    public delegate void CallbackModelInfoMsg(GzModelInfoMsg msg);
    public delegate void CallbackPoseInfoMsg(GzPoseInfoMsg msg);
    public delegate void CallbackSceneMsg(GzSceneMsg msg);
    public delegate void CallbackRequestMsg(GzRequestMsg msg);

    #endregion //PUBLIC_MEMBER_VARIABLES

    #region PRIVATE_MEMBER_VARIABLES

    private GzBridgeWebSocketConnection m_GzBridge = null;
    private bool m_Initialized = false;

    private List<CallbackOnCloseConnection> callbacks_on_close_connection = new List<CallbackOnCloseConnection>();

    private List<CallbackMaterialMsg> callbacks_material_msg = new List<CallbackMaterialMsg>();
    private List<CallbackModelInfoMsg> callbacks_model_info_msg = new List<CallbackModelInfoMsg>();
    private List<CallbackPoseInfoMsg> callbacks_pose_info_msg = new List<CallbackPoseInfoMsg>();
    private List<CallbackSceneMsg> callbacks_scene_msg = new List<CallbackSceneMsg>();
    private List<CallbackRequestMsg> callbacks_request_msg = new List<CallbackRequestMsg>();

    #endregion //PRIVATE_MEMBER_VARIABLES

    #region MONOBEHAVIOR_METHODS

    // Use this for initialization
    void Start () {
        string url = string.Format("{0}:{1}/gzbridge", BackendConfigService.Instance.IP, BackendConfigService.Instance.GzBridgePort);
        this.ConnectToGzBridge(url);
    }
	
	// Update is called once per frame
	void Update () {
        if (m_Initialized)
        {
            m_GzBridge.Render();
        }
    }

    void OnApplicationQuit()
    {
        foreach (CallbackOnCloseConnection callback in callbacks_on_close_connection)
        {
            callback();
        }

        if (m_Initialized)
            m_GzBridge.Disconnect();
    }

    #endregion //MONOBEHAVIOR_METHODS

    #region PUBLIC_METHODS

    public void AddCallbackOnCloseConnection(CallbackOnCloseConnection callback)
    {
        this.callbacks_on_close_connection.Add(callback);
    }

    public void AddCallbackMaterialMsg(CallbackMaterialMsg callback)
    {
        this.callbacks_material_msg.Add(callback);
    }

    public void AddCallbackModelInfoMsg(CallbackModelInfoMsg callback)
    {
        this.callbacks_model_info_msg.Add(callback);
    }

    public void AddCallbackPoseInfoMsg(CallbackPoseInfoMsg callback)
    {
        this.callbacks_pose_info_msg.Add(callback);
    }

    public void AddCallbackSceneMsg(CallbackSceneMsg callback)
    {
        this.callbacks_scene_msg.Add(callback);
    }

    public void AddCallbackRequestMsg(CallbackRequestMsg callback)
    {
        this.callbacks_request_msg.Add(callback);
    }

    /// <summary>
    /// Main function to receive scene messages from GzBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing scene message.</param>
    public void ReceiveMessage(GzSceneMsg msg)
    {
        //GazeboScene.GetComponent<GazeboSceneManager>().OnSceneMsg(msg.MsgJSON);
        foreach(CallbackSceneMsg callback in callbacks_scene_msg)
        {
            callback(msg);
        }
    }

    /// <summary>
    /// Main function to receive pose info messages from GzBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing pose info message.</param>
    public void ReceiveMessage(GzPoseInfoMsg msg)
    {
        //GazeboScene.GetComponent<GazeboSceneManager>().OnPoseInfoMsg(msg.MsgJSON);
        foreach (CallbackPoseInfoMsg callback in callbacks_pose_info_msg)
        {
            callback(msg);
        }
    }

    /// <summary>
    /// Main function to receive model info messages from GzBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing model info message.</param>
    public void ReceiveMessage(GzModelInfoMsg msg)
    {
        //GazeboScene.GetComponent<GazeboSceneManager>().OnModelInfoMsg(msg);
        foreach (CallbackModelInfoMsg callback in callbacks_model_info_msg)
        {
            callback(msg);
        }
    }

    /// <summary>
    /// Main function to receive material messages from GzBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing material message.</param>
    public void ReceiveMessage(GzMaterialMsg msg)
    {
        //GazeboScene.GetComponent<GazeboSceneManager>().OnMaterialMsg(msg);
        foreach (CallbackMaterialMsg callback in callbacks_material_msg)
        {
            callback(msg);
        }
    }

    public void ReceiveMessage(GzRequestMsg msg)
    {
        //Debug.Log("ReceiveMessage(GzRequestMsg msg)");
        foreach (CallbackRequestMsg callback in callbacks_request_msg)
        {
            callback(msg);
        }
    }

    public void ConnectToGzBridge(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        if (GazeboScene == null || GazeboScene.GetComponent<GazeboSceneManager>() == null)
            return;

        m_GzBridge = new GzBridgeWebSocketConnection("ws://" + url);

        // DOES NOT WORK! m_Ros is never null if you call the Constructor, WAIT TILL SIMON IMPLEMENTS UDP BROADCAST WITH ROS CONFIGURATION, GET THE IP ADDRESS FROM THE BROADCAST
        if (m_GzBridge != null)
        {
            //m_GzBridge.AddSubscriber(typeof(GzResponseSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzSceneMsgSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzPoseInfoSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzModelInfoSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzMaterialSubscriber));
            m_GzBridge.AddSubscriber(typeof(GzRequestSubscriber));
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

    /*IEnumerator Connect()
    {
        yield return new WaitUntil(() => !string.IsNullOrEmpty(BackendConfigService.Instance.IP));

        string url = string.Format("{0}:{1}/gzbridge", BackendConfigService.Instance.IP, BackendConfigService.Instance.GzBridgePort);
        this.ConnectToGzBridge(url);
    }*/

    #endregion //PRIVATE_METHODS
}
