using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Force : MonoBehaviour
{

	public GameObject sword;

	public float DebugRotFactor;

	private Rigidbody rBodySword;

	// Use this for initialization
	void Start()
	{
		rBodySword = sword.GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
	{
		float rotFactor = 1;

		//direction of force: difference of position, reduced by velocity (slows down bevor it reaches the target).
		Vector3 transForce = (transform.position - sword.transform.position) - rBodySword.velocity;

		rBodySword.AddForce(transForce * Time.deltaTime * 100);



		Quaternion rs = sword.transform.rotation;
		Quaternion rt = transform.rotation;

		Quaternion diff = rt * Quaternion.Inverse(rs);



		rBodySword.AddTorque(new Vector3(diff.x, diff.y, diff.z) * Time.deltaTime * rotFactor * 1000);
		//sword.transform.rotation *= test;
		DebugRotFactor = rotFactor;
	}

	void FixedUpdate()
	{

	}
}
