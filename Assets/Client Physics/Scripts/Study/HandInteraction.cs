using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInteraction : MonoBehaviour {
    /*
	// Use this for initialization
	public PhysicsTest test;
    public GameObject handTriggerLeft, handTriggerRight;

    public Color defaultCol;

    public float timeA1, timeB1, timeC1, timeD1;
    public float timeA2, timeB2, timeC2, timeD2;


	bool handContact, grabDone, grabbed, testFinished;

    void Start()
    {
        PrepareHandTrigger(handTriggerLeft);
        PrepareHandTrigger(handTriggerRight);
    }

    void PrepareHandTrigger(GameObject trigger)
    {
        Vector3 com = Vector3.zero;
        Vector3 inertiaTensor = Vector3.one;
        Rigidbody parent = trigger.transform.parent.gameObject.GetComponent<Rigidbody>();
        if (parent != null)
        {
            com = parent.centerOfMass;
            inertiaTensor = parent.inertiaTensor;
        }

        trigger.SetActive(true);

        if (parent != null)
        {
            parent.centerOfMass = com;
            parent.inertiaTensor = inertiaTensor;
        }
    }


    // Update is called once per frame
    void FixedUpdate () {
        /*
        if (!grabbed && handContact)
        {
            grabbed = true;
            //transform.parent = hand;
            handTriggerLeft.GetComponent<Collider>().enabled = false;
            handTriggerRight.GetComponent<Collider>().enabled = false;

            test.StopCountTimeUntilGrabbed(true);
        }
        if (grabbed && !grabDone)
        {
            respawnTrigger.SetActive(false);

            //Follow Hand

            //GetComponent<Rigidbody>().isKinematic = true;
            //GetComponent<Collider>().isTrigger = true;

            transform.parent = hand;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            grabDone = true;
        }
        */
        /*
    }


    void OnTriggerEnter(Collider other)
    {
        if (!testFinished)
        {
            switch (other.name)
            {
                case "SectionA1":
                case "SectionA2":
                case "SectionB1":
                case "SectionB2":
                case "SectionC1":
                case "SectionC2":
                case "SectionD1":
                case "SectionD2": other.gameObject.GetComponent<MeshRenderer>().material.color = new Vector4(1, 0, 0, defaultCol.a); break;
                case "FinishLine": Debug.Log("Finished");  
                                   testFinished = true; 
                                   //test.StopCountTimeUntilFinished(true);
                                   break;
                default: break;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!testFinished)
        {
            switch (other.name)
            {
                case "SectionA1": timeA1 += Time.deltaTime; break;
                case "SectionA2": timeA2 += Time.deltaTime; break;
                case "SectionB1": timeB1 += Time.deltaTime; break;
                case "SectionB2": timeB2 += Time.deltaTime; break;
                case "SectionC1": timeC1 += Time.deltaTime; break;
                case "SectionC2": timeC2 += Time.deltaTime; break;
                case "SectionD1": timeD1 += Time.deltaTime; break;
                case "SectionD2": timeD2 += Time.deltaTime; break;
                default: break;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        switch (other.name)
        {
            case "SectionA1": 
            case "SectionA2": 
            case "SectionB1": 
            case "SectionB2": 
            case "SectionC1": 
            case "SectionC2": 
            case "SectionD1": 
            case "SectionD2": other.gameObject.GetComponent<MeshRenderer>().material.color = defaultCol; break;
            default: break;
        }
        
    }
    */
}
