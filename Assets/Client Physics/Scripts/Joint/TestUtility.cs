using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUtility : MonoBehaviour {
    Rigidbody rigidbody;
    public GameObject obj;
    public float proportionalGain = 10000;
    public float integralGain = 100;
    public float derivativeGain = 1000;

    Vector3 previousError = Vector3.zero;
    Vector3 integral= Vector3.zero;

	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody>();
	}
	

	// Update is called once per frame
	void FixedUpdate () {
        Vector3 error = (Quaternion.Inverse(obj.transform.localRotation) * transform.localRotation).eulerAngles;
        rigidbody.AddTorque(GetCorrection(error), ForceMode.Force);
	}

    Vector3 GetCorrection(Vector3 error)
    {
        Vector3 derivative = (error - previousError) / Time.fixedDeltaTime;
        previousError = error;
        integral += error * Time.fixedDeltaTime;
        return proportionalGain * error + integralGain * integral + derivativeGain * derivative; 
    }
    
}
