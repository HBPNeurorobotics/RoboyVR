using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Measures time of contact between a body part and an object.
/// </summary>
public class CheckBound : MonoBehaviour {

	public float timeSpent = 0;
	public bool measure;
	public List<Collider> contacts = new List<Collider>();
    public Dictionary<string, float> timesPerBodyParts = new Dictionary<string, float>();
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
		// measure time as long as there is contact between the player and the object
		if (contacts.Count != 0)
		{
			timeSpent += Time.fixedDeltaTime;
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
            TimeBodyPart timeBodyPart = gameObject.AddComponent<TimeBodyPart>();
            timeBodyPart.name = other.collider.name;

            if (!timesPerBodyParts.ContainsKey(other.collider.name))
            {
                timesPerBodyParts.Add(other.collider.name, 0f);
            }
		}
	}

	void OnCollisionExit(Collision other)
	{
        if (measure && other.gameObject.layer >= 10 && other.gameObject.layer <= 26 && other.gameObject.layer != 13 && other.gameObject.layer != 14)
        {
            contacts.Remove(other.collider);

            foreach(TimeBodyPart timeBodyPart in gameObject.GetComponents<TimeBodyPart>())
            {
                if (timeBodyPart.name.Equals(other.collider.name))
                {
                    if (timesPerBodyParts.ContainsKey(other.collider.name))
                    {
                        timesPerBodyParts[other.collider.name] += timeBodyPart.time;
                        Destroy(timeBodyPart);
                    }
                }
            }
		}
	}

    void SetColors(Color color)
    {
        renderer.material.color = color;
    }

}
