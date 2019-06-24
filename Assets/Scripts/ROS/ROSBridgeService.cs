using UnityEngine;
using System.Collections.Generic;
using ROSBridgeLib;

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
public class ROSBridgeService : Singleton<ROSBridgeService>
{
    #region PUBLIC_MEMBER_VARIABLES

    public ROSBridgeWebSocketConnection websocket
    {
        get { return this.m_Ros; }
    }

    #endregion //PUBLIC_MEMBER_VARIABLES

    #region PRIVATE_MEMBER_VARIABLES

    private ROSBridgeWebSocketConnection m_Ros = null;
    private bool m_ROSInitialized = false;

    #endregion //PRIVATE_MEMBER_VARIABLES

    #region MONOBEHAVIOR_METHODS

    // Use this for initialization
    void Start()
    {
        this.ConnectToROSBridge();
    }

    void Update()
    {

    }
    
    void OnApplicationQuit()
    {
        if (m_ROSInitialized)
            m_Ros.Disconnect();
    }
    #endregion //MONOBEHAVIOR_METHODS

    #region PUBLIC_METHODS
    
    public void CallService(string service, string args)
    {
        this.m_Ros.CallService(service, args);
    }

    #endregion //PUBLIC_METHODS

    #region PRIVATE_METHODS

    void ConnectToROSBridge()
    {
        m_Ros = new ROSBridgeWebSocketConnection("ws://" + BackendConfigService.Instance.IP, BackendConfigService.Instance.ROSBridgePort, BackendConfigService.Instance.ROSBridgeSuffix);
        if (m_Ros != null)
        {
            m_Ros.Connect();
            m_ROSInitialized = true;

            Debug.Log("ROS successfully initialized!");
        }
        else
        {
            Debug.LogWarning("ROS could not be initialized!");
        }
    }

    #endregion //PRIVATE_METHODS
}
