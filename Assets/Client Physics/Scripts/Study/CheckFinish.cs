using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFinish : MonoBehaviour {
	public PhysicsTest test;
	public bool trigger;
    public bool isTarget;
    Vector4 defaultCol;
    public Color highlightCol;

    public PhysicsTest.PHASE phase;
	// Use this for initialization
	void Start () {
        defaultCol = GetComponent<MeshRenderer>().material.color;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnCollisionEnter(Collision other)
	{
		//It has been hit by a limb of the player
		if (trigger && (other.gameObject.layer == 19 || other.gameObject.layer == 20 || other.gameObject.layer == 25 || other.gameObject.layer == 26))
		{
			
            if (isTarget)
            {
                SetColor(highlightCol);
            }
            else
            {
                trigger = false;
                test.OnFinishLineReached(phase);
            }
		}
	}

    private void OnCollisionExit(Collision other)
    {
        if (trigger && (other.gameObject.layer == 19 || other.gameObject.layer == 20 || other.gameObject.layer == 25 || other.gameObject.layer == 26))
        {
            trigger = true;
            if (isTarget)
            {
                SetColor(defaultCol);
            }
        }
    }

    void SetColor(Vector4 new_Color)
    {

        GetComponent<MeshRenderer>().material.color = new_Color;
    }

}
