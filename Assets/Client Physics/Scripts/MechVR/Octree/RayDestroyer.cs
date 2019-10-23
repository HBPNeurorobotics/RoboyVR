using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayDestroyer : MonoBehaviour
{
	bool destroy;
	public GameObject sphereObject;
	public GameObject octreeObject;
	public GameObject bulletPrefab;
	public GameObject camObj;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			//RayKill();

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			var ball = Instantiate(bulletPrefab, camObj.transform.position, new Quaternion());
			var rigidBody = ball.GetComponent<Rigidbody>();
			rigidBody.velocity = ray.direction.normalized * Random.Range(10, 50);
		}

		if (Input.GetButtonDown("Fire2"))
		{
			destroy = !destroy;
		}

		if (Input.GetKeyDown(KeyCode.C))
		{
			var oct = octreeObject.GetComponent<OctreeHandler>();
			var diam = sphereObject.transform.localScale.x / octreeObject.transform.localScale.x;
			var position = octreeObject.transform.InverseTransformPoint(sphereObject.transform.position);
			var sphere = new BoundingSphere(position, diam / 2);
			oct.Carve(sphere);
		}

		if (Input.GetKeyDown(KeyCode.S)) // <<=
		{
			var oct = octreeObject.GetComponent<OctreeHandler>();
			oct.ResetOct();
		}

		if (Input.GetKeyDown(KeyCode.W)) // =>>
		{
			var oct = octreeObject.GetComponent<OctreeHandler>();
			oct.Replay();
		}

		if (Input.GetKeyDown(KeyCode.D)) // ->
		{
			var oct = octreeObject.GetComponent<OctreeHandler>();
			oct.Step();
		}

		if (Input.GetKeyDown(KeyCode.A)) // <-
		{
			var oct = octreeObject.GetComponent<OctreeHandler>();
			oct.Undo();
		}

		if (destroy)
		{
			RayKill();
		}
	}

	void RayKill()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			var oh2 = hit.transform.gameObject.GetComponent<OctreeHandler>();
			if (oh2 != null)
				oh2.HitLocal(hit.transform.InverseTransformPoint(hit.point + (ray.direction.normalized * 0.00001f)));
		}
	}
}
