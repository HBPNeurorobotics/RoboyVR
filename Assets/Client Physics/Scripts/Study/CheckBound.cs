using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBound : MonoBehaviour {

	public float timeSpent = 0;
	public bool measure;
	public List<Collider> contacts = new List<Collider>();
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
            Color warningColor = new Vector4(1, 0, 0, defaultCol.a);
            SetColors(warningColor);
		}
		else
		{
			SetColors(defaultCol);
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

    void SetColors(Color color)
    {
        renderer.material.color = color;
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<MeshRenderer>().material.color = color;
        }
    }

}
