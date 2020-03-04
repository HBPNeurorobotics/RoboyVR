using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFinish : MonoBehaviour {
	public PhysicsTest test;
	public bool trigger;
    public bool isTarget;
    Color defaultCol;

    public PhysicsTest.PHASE phase;
	// Use this for initialization
	void Start () {
        defaultCol = GetComponent<MeshRenderer>().material.color;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerEnter(Collider other)
	{
		//It has been hit by a limb of the player
		if (trigger && (other.gameObject.layer == 10 || other.gameObject.layer == 11 || other.gameObject.layer == 13 || other.gameObject.layer == 14 || other.gameObject.layer == 19 || other.gameObject.layer == 20))
		{
			trigger = false;
            if (isTarget)
            {
                SetSuccess(new Color(0, 1, 0, defaultCol.a));
            }
            else
            {
                test.OnFinishLineReached(phase);
            }
		}
	}

    private void OnTriggerExit(Collider other)
    {
        if (trigger && (other.gameObject.layer == 10 || other.gameObject.layer == 11 || other.gameObject.layer == 13 || other.gameObject.layer == 14 || other.gameObject.layer == 19 || other.gameObject.layer == 20))
        {
            trigger = true;
            if (isTarget)
            {
                SetSuccess(defaultCol);
            }
        }
    }

    void SetSuccess(Color new_Color)
    {
        foreach(Transform wall in gameObject.transform.parent)
        {
            Debug.Log(gameObject.name + "  " + wall.name);
            wall.gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0);
        }
    }

}
