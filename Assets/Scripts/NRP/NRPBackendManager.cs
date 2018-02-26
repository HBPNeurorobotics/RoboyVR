using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.gazebo_msgs;
using System.Net;

public class NRPBackendManager : MonoBehaviour {

    public string NRPBackendIP = "192.168.0.153";
    public int GzBridgePort = 8080;
    public int ROSBridgePort = 9090;
    public int BackendProxyPort = 8000;
    public string AuthUsername = "nrpuser";
    public string AuthPassword = "password";
    public GameObject GazeboScene = null;

    /// <summary>
    /// The coordinates of the point where the avatar should spawn.
    /// </summary>
    public Vector3 avatarPosition = Vector3.zero;

    private GzBridgeManager GzBridgeManager;
    private ROSBridge ROSBridgeManager;
    //private string authToken = null;
    private string avatarId = null;
    private float resetTime = 0;

    // Use this for initialization
    void Start ()
    {
        // Generate an avatarId
        avatarId = Network.player.ipAddress.Replace('.', '_');

        if (!string.IsNullOrEmpty(this.NRPBackendIP))
        {
            // set the avatar id for all avatar publisher and subscriber
            ROSAvatarVelPublisher.setAvatarId(avatarId);
            ROSAvatarRotPublisher.setAvatarId(avatarId);
            ROSAvatarRotSubscriber.setAvatarId(avatarId);

            this.ConnectToGazeboBridge();
            this.ConnectToROSBridge();
        }
    }
	
	
    /// <summary>
    /// Enables the user to reset the avatar's position to the starting position by pressing R and T.
    /// </summary>
	void Update () {
        if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.T) && Time.time > resetTime + 5 && avatarId != null)
        {
            Debug.Log("Reset avatar position");
           
            // Tells the server to reset the avatar.
            ROSBridge.Instance.ROS.Publish(ROSAvatarResetPublisher.GetMessageTopic(),new ModelStateMsg("user_avatar_"+avatarId, 
                new PoseMsg(new PointMsg(avatarPosition.x, avatarPosition.z, avatarPosition.y), new QuaternionMsg(0, 0, 0, 1)), 
                new Vector3Msg(1, 1, 1), 
                new TwistMsg(new Vector3Msg(0, 0, 0), new Vector3Msg(0, 0, 0))));

            resetTime = Time.time;

            // Synchronize the VR headset with the new avatar position.
            VRMountToAvatarHeadset vr = FindObjectOfType<VRMountToAvatarHeadset>();
            if(vr != null)
            {
                vr.synchronizeVRToAvatarPosition();
            }
        }
    }

    private void ConnectToGazeboBridge()
    {
        // initialize GzBridge component
        if (this.gameObject.GetComponent<GzBridgeManager>() == null)
        {
            this.gameObject.AddComponent<GzBridgeManager>();
        }
        this.GzBridgeManager = this.gameObject.GetComponent<GzBridgeManager>();

        // get authentication token
        string authURL = this.NRPBackendIP + ":" + this.BackendProxyPort.ToString() + "/authentication/authenticate";
        string authJSON = "{\"user\":\"" + this.AuthUsername + "\",\"" + this.AuthPassword + "\":\"password\"}";
        Debug.Log("authentication json: " + authJSON);

        WWW www;
        Dictionary<string, string> postHeader = new Dictionary<string, string>();
        postHeader.Add("Content-Type", "application/json");

        //convert json string to byte
        var postData = System.Text.Encoding.UTF8.GetBytes(authJSON);

        www = new WWW(authURL, postData, postHeader);
        StartCoroutine(this.WaitForAuthRequest(www));

        GzBridgeManager.avatarPosition = avatarPosition;
        GzBridgeManager.avatarId = avatarId;
        GzBridgeManager.URL = NRPBackendIP + ":" + GzBridgePort.ToString() + "/gzbridge";
        GzBridgeManager.GazeboScene = this.GazeboScene;
        GzBridgeManager.ConnectToGzBridge();

    }

    private void ConnectToROSBridge()
    {
        // initialize ROSBridge component
        if (this.gameObject.GetComponent<ROSBridge>() == null)
        {
            this.gameObject.AddComponent<ROSBridge>();
        }
        this.ROSBridgeManager = this.gameObject.GetComponent<ROSBridge>();

        ROSBridgeManager.ROSCoreIP = NRPBackendIP;
        ROSBridgeManager.Port = ROSBridgePort;
    }

    // This code was previously used to acquire a token from the backend and work with this token as avatarId and authToken
    private IEnumerator WaitForAuthRequest(WWW data)
    {
        yield return data; // Wait until the download is done
        if (data.error != null)
        {
            Debug.LogWarning("There was an error sending request: " + data.error);
        }
        //else
        //{
        //    this.authToken = data.text;
        //    Debug.Log("auth token: " + this.authToken);

        //    // the authentication token needs to be adapted to be used as avatar id, as the topics doesn't allow ' - ' in their names
        //    avatarId = this.authToken.Replace('-', '_');

        //    // set the avatar id for all avatar publisher and subscriber
        //    ROSAvatarVelPublisher.setAvatarId(avatarId);
        //    ROSAvatarRotPublisher.setAvatarId(avatarId);
        //    ROSAvatarRotSubscriber.setAvatarId(avatarId);

        //    GzBridgeManager.avatarPosition = avatarPosition;
        //    GzBridgeManager.avatarId = avatarId;
        //    GzBridgeManager.URL = NRPBackendIP + ":" + GzBridgePort.ToString() + "/gzbridge?token=" + this.authToken;
        //    GzBridgeManager.GazeboScene = this.GazeboScene;
        //    GzBridgeManager.ConnectToGzBridge();

        //    this.ConnectToROSBridge();
        //}
    }
}
