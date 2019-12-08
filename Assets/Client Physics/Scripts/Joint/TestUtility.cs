using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUtility : MonoBehaviour {
    public GameObject obj;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Quaternion localRot = transform.localRotation;
            localRot.x += 1;
            Quaternion localZero = Quaternion.identity;
            ConfigJointUtility.SwitchConfigJointMotionHandlerForDuration(GetComponent<ConfigurableJoint>(), obj.transform.localRotation, 5f, false, false, false, this);
        }
	}
}
