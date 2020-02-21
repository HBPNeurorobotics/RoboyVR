using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

	// Use this for initialization
	public PhysicsTest test;
	public GameObject handTrigger, respawnTrigger;
	Transform hand;
	Vector3 startPos = new Vector3();
	Quaternion startRot = new Quaternion();
	bool handContact, proximalContact, grabbed;
    public Color defaultCol;

    public float timeA, timeB, timeC, timeD;

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
            /*
            transform.position = new Vector3(hand.position.x - 0.1f, hand.position.y - 0.05f, hand.position.z);
            transform.rotation = new Quaternion(hand.rotation.x, hand.rotation.y, hand.rotation.z, hand.rotation.w);
            */
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

    }

    /*
	void OnCollisionExit(Collision other)
	{
		if (!grabbed)
		{
			if (other.gameObject.name.Equals("1")) handContact = false;
		}
	}
	*/

    void OnTriggerStay(Collider other)
    {
        if (other.name.Equals("SectionA"))
        {
            timeA += Time.deltaTime;
        }
        else if (other.name.Equals("SectionB"))
        {
            timeB += Time.deltaTime;
        }
        else if (other.name.Equals("SectionC"))
        {
            timeC += Time.deltaTime;
        }
        else if (other.name.Equals("SectionD"))
        {
            timeD += Time.deltaTime;
        }

        other.gameObject.GetComponent<MeshRenderer>().material.color = new Vector4(1,0,0, defaultCol.a);
    }

    void OnTriggerExit(Collider other)
    {
        other.gameObject.GetComponent<MeshRenderer>().material.color = defaultCol;
    }
}
