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

	void OnCollisionEnter(Collision other)
	{
		//It has been hit by the body of the player
		if(measure && other.gameObject.layer >= 10 && other.gameObject.layer <= 26 && other.gameObject.layer != 13 && other.gameObject.layer != 14)
		{
			contacts.Add(other.collider);
		}
	}
	void OnCollisionExit(Collision other)
	{
        if (measure && other.gameObject.layer >= 10 && other.gameObject.layer <= 26 && other.gameObject.layer != 13 && other.gameObject.layer != 14)
        {
            contacts.Remove(other.collider);
		}
	}

    void SetColors(Color color)
    {
        renderer.material.color = color;
        foreach (Transform child in transform)
        {
            if (!child.name.Equals("Target"))
            {
                child.gameObject.GetComponent<MeshRenderer>().material.color = color;
            }
        }
    }

}
