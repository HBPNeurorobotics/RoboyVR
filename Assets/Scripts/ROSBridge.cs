using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;

/// <summary>
/// Handles the ROSBridge connection. Adds all ROS components of each ROSObject in the scene. You need one object of this in each scene where you have ROS actors.
/// </summary>
public class ROSBridge : Singleton<ROSBridge> {

    /// <summary>
    /// Stores the rotation of the avatar as distributed by the corresponding topic.
    /// </summary>
    public Quaternion serverAvatarRot;

    /// <summary>
    /// The IP address of the roscore running on the other side of the ROSBridge.
    /// </summary>
    public string ROSCoreIP = "";

    /// <summary>
    /// Port of the ROSBridge.
    /// </summary>
    public int Port = 9090;

    /// <summary>
    /// Public property for other classes to the ros websocket.
    /// </summary>
    public ROSBridgeWebSocketConnection ROS { get { return m_ROS; } }

    /// <summary>
    /// Public property of all active ROSObjects in the scene.
    /// </summary>
    public List<GameObject> ROSObjects { get { return m_ROSObjects; } }

    /// <summary>
    /// ROS websocket connection.
    /// </summary>
    private ROSBridgeWebSocketConnection m_ROS = null;

    /// <summary>
    /// Is ROS initialized?
    /// </summary>
    private bool m_ROSInitialized = false;

    /// <summary>
    /// List of all active ROSObjects.
    /// </summary>
    private List<GameObject> m_ROSObjects = new List<GameObject>();

    /// <summary>
    /// Initializes the ROS websocket connection and searches for all ROSObjects in the scene.
    /// </summary>
    private void Start()
    {
        m_ROS = new ROSBridgeWebSocketConnection("ws://" + ROSCoreIP, Port);

        if (m_ROS != null)
        {
            // find all ROSObjects
            var rosObjects = FindObjectsOfType<ROSObject>();
            
            // get all attached ros components of all ROSObjects and add them
            foreach (ROSObject rosObj in rosObjects)
            {
                var subscribers = rosObj.GetComponents<ROSBridgeSubscriber>();
                var publishers = rosObj.GetComponents<ROSBridgePublisher>();
                var services = rosObj.GetComponents<ROSBridgeService>();
                foreach (var sub in subscribers)
                    m_ROS.AddSubscriber(sub.GetType());

                foreach (var pub in publishers)
                    m_ROS.AddPublisher(pub.GetType());

                foreach (var serv in services)
                    m_ROS.AddServiceResponse(serv.GetType());

                m_ROSObjects.Add(rosObj.gameObject);
            }

            // start connection
            m_ROS.Connect();
            m_ROSInitialized = true;
            Debug.Log("ROSBridge connection successfully established!");
        }
    }

    /// <summary>
    /// Run ROSBridge.
    /// </summary>
    private void Update()
    {
        if (m_ROSInitialized)
        {
            m_ROS.Render();
        }
    }

    /// <summary>
    /// Disconnect from the simulation when Unity is not running.
    /// </summary>
    private void OnApplicationQuit()
    {
        if (m_ROSInitialized)
        {
            m_ROS.Disconnect();
        }
    }

    /// <summary>
    /// Main function to receive avatar rotation messages from ROSBridge.
    /// </summary>
    /// <param name="msg">JSON msg containing the rotation of the avatar in quaternion.</param>
    public void ReceiveMessage(QuaternionMsg msg)
    {
        serverAvatarRot = new Quaternion(-(float)msg.GetX(), -(float)msg.GetZ(), -(float)msg.GetY(), (float)msg.GetW());
    }
}
