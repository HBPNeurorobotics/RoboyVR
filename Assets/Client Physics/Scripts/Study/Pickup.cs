using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

	// Use this for initialization
	public PhysicsTest test;
	public GameObject[] triggerObjects = new GameObject[6];
	Transform hand;
	Vector3 startPos = new Vector3();
	Quaternion startRot = new Quaternion();
	bool handContact, proximalContact, grabbed;

	void Start () {

		startPos = transform.position;
		startRot = transform.rotation;

		foreach(GameObject obj in triggerObjects)
		{
			Vector3 com = Vector3.zero;
			Vector3 inertiaTensor = Vector3.one;
			Rigidbody parent = obj.transform.parent.gameObject.GetComponent<Rigidbody>();
			if(parent != null)
			{
				com = parent.centerOfMass;
				inertiaTensor = parent.inertiaTensor;
			}

			obj.SetActive(true);

			if (parent != null)
			{
				parent.centerOfMass = com;
				parent.inertiaTensor = inertiaTensor;
			}
		}
		

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(!grabbed && handContact)
		{
			grabbed = true;
			transform.parent = hand;
			foreach (GameObject obj in triggerObjects)
			{
				obj.SetActive(false);
			}
			test.SetCountTimeUntilGrabbed(true);
		}
	}

    void OnCollisionEnter(Collision other)
    {
		/*
        if (!grabbed)
        {
            foreach (ContactPoint point in other.contacts)
            {
                Collider child = point.otherCollider;
				Debug.Log(child.name);
                if (child != null)
                {
                    if (child.tag.Equals("Test"))
                    {
                        Debug.Log("Contact");
                        handContact = true;
                        hand = other.transform;
                    }
                    else
                    {
                        if (child.name.Equals("RespawnTrigger"))
                        {
                            Debug.Log("Respawn");
                            test.IncrementTries();
                            transform.position = startPos;
                            transform.rotation = startRot;
                        }

                    }
                }
            }
        }
		*/
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
}
