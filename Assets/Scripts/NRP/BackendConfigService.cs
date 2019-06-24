using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackendConfigService : Singleton<BackendConfigService>
{
    public string IP = "xxx.xxx.xxx.xxx";
    public int GzBridgePort = 8080;
    public int ROSBridgePort = 8080;
    public string ROSBridgeSuffix = "/rosbridge";
    public int ProxyPort = 9000;

    private ROSBridge ROSBridgeManager;

	// Use this for initialization
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update () {

    }
}
