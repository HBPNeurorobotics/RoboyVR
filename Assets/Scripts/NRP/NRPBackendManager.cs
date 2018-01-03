﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NRPBackendManager : MonoBehaviour {

    public string NRPBackendIP = "192.168.0.153";
    public int GzBridgePort = 8080;
    public int ROSBridgePort = 9090;
    public int BackendProxyPort = 8000;
    public string AuthUsername = "nrpuser";
    public string AuthPassword = "password";
    public GameObject GazeboScene = null;

    private GzBridgeManager GzBridgeManager;
    private ROSBridge ROSBridgeManager;
    private string authToken = null;

	// Use this for initialization
	void Start ()
    {
        if (!string.IsNullOrEmpty(this.NRPBackendIP))
        {
            this.ConnectToGazeboBridge();
            this.ConnectToROSBridge();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
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

        // convert json string to byte
        var postData = System.Text.Encoding.UTF8.GetBytes(authJSON);

        www = new WWW(authURL, postData, postHeader);
        StartCoroutine(this.WaitForAuthRequest(www));
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

    private IEnumerator WaitForAuthRequest(WWW data)
    {
        yield return data; // Wait until the download is done
        if (data.error != null)
        {
            Debug.LogWarning("There was an error sending request: " + data.error);
        }
        else
        {
            this.authToken = data.text;
            //Debug.Log("auth token: " + this.authToken);

            GzBridgeManager.URL = NRPBackendIP + ":" + GzBridgePort.ToString() + "/gzbridge?token=" + this.authToken;
            GzBridgeManager.GazeboScene = this.GazeboScene;
            GzBridgeManager.ConnectToGzBridge();
        }
    }
}
