using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

	// Use this for initialization
	public PhysicsTest test;
	public GameObject handTrigger, respawnTrigger;

    public Color defaultCol;

    public float timeA1, timeB1, timeC1, timeD1;
    public float timeA2, timeB2, timeC2, timeD2;

	Transform hand;
	Vector3 startPos = new Vector3();
	Quaternion startRot = new Quaternion();
	bool handContact, proximalContact, grabbed, testFinished;

    void Start()
    {

        startPos = transform.position;
        startRot = transform.rotation;

        Vector3 com = Vector3.zero;
        Vector3 inertiaTensor = Vector3.one;
        Rigidbody parent = handTrigger.transform.parent.gameObject.GetComponent<Rigidbody>();
        if (parent != null)
        {
            com = parent.centerOfMass;
            inertiaTensor = parent.inertiaTensor;
        }

        handTrigger.SetActive(true);

        if (parent != null)
        {
            parent.centerOfMass = com;
            parent.inertiaTensor = inertiaTensor;
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (!grabbed && handContact)
        {
            grabbed = true;
            //transform.parent = hand;

            handTrigger.GetComponent<Collider>().enabled = false;

            test.SetCountTimeUntilGrabbed(true);
        }
        if (grabbed)
        {
            respawnTrigger.SetActive(false);

            //Follow Hand
            GetComponent<Rigidbody>().isKinematic = true;

            transform.parent = hand;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

	public void Reset()
	{
		test.IncrementTries();
		transform.position = startPos;
		transform.rotation = startRot;
	}

	public void HandContact(Transform toFollow)
	{ 
		handContact = true;
		hand = toFollow;
        Debug.Log(hand.name);
        if (hand.name.Contains("Left"))
        {
            gameObject.layer = 10;
        }
        else if (hand.name.Contains("Right"))
        {
            gameObject.layer = 11;
        }
        test.SetCountTimeUntilFinished(false);
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
                case "FinishLine": testFinished = true; test.SetCountTimeUntilFinished(true); break;
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
}
