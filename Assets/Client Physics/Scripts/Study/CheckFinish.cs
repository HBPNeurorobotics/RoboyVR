using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFinish : MonoBehaviour {
	public PhysicsTest test;
	public bool trigger;

	public enum PHASE
	{
		HAND,
		FOOT
	}
	public PHASE phase;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnTriggerEnter(Collider other)
	{
		//It has been hit by a limb of the player
		if (trigger && (other.gameObject.layer == 10 || other.gameObject.layer == 11 || other.gameObject.layer == 13 || other.gameObject.layer == 14))
		{
			trigger = false;
			test.OnFinishLineReached(phase);
		}
	}
}
