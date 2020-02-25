using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBound : MonoBehaviour {

	public float timeSpent = 0;
	public bool measure;
	List<Collider> contacts = new List<Collider>();
	MeshRenderer renderer;
	Color defaultCol;
	// Use this for initialization
	void Start () {
		renderer = GetComponent<MeshRenderer>();
		defaultCol = renderer.material.color;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		if (contacts.Count != 0)
		{
			timeSpent += Time.deltaTime;
			renderer.material.color = new Vector4(1, 0, 0, defaultCol.a);
		}
		else
		{
			renderer.material.color = defaultCol;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		//It has been hit by the body of the player
		if(measure && other.gameObject.layer >= 10 && other.gameObject.layer <= 20)
		{
			contacts.Add(other);
		}
	}
	void OnTriggerExit(Collider other)
	{
		if (measure && other.gameObject.layer >= 10 && other.gameObject.layer <= 20)
		{
			contacts.Remove(other);

		}
	}
}
