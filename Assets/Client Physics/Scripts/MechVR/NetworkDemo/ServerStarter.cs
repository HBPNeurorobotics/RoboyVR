using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerStarter : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{
		var nwMgr = GetComponent<NetworkManager>();
		nwMgr.StartServer();
	}

	// Update is called once per frame
	void Update()
	{

	}
}
