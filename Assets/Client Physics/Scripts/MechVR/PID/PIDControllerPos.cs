using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// PID controller to translate a Gameobject(rigidbody) to the target position
/// </summary>
public class PIDControllerPos : MonoBehaviour
{
	private VectorPid headingController;

	/// <summary>
	/// the part(gameobject) this PID controlls
	/// </summary>
	public GameObject part;

	/// <summary>
	/// tmp thing to plot the arm position locally
	/// </summary>
	public GameObject shoulder;

	/// <summary>
	/// the rigidbody of part (rigidbody must not change)
	/// </summary>
	private Rigidbody rBody;

	private Vector3 factor;

	public PIDControllerPos(GameObject shoulder, GameObject part, float p, float i, float d, Vector3 factor)
	{
		this.part = part;
		rBody = part.GetComponent<Rigidbody>();
		headingController = new VectorPid(p, i, d);
		//headingController = new VectorPIDValueLogger(p, i, d , part.name);
		this.factor = factor;
		this.shoulder = shoulder;
	}

	/// <summary>
	/// Has to be called every FixedUpdate
	/// Calculates the force with PID controllers to move the object to the target position
	/// </summary>
	/// <param name="rotation"></param>
	public void UpdateTarget(Vector3 pos)
	{
		/*
		Vector3 error = pos - part.transform.localPosition + shoulder.transform.localPosition;

		Vector3 correction = headingController.UpdateNormalPid(error, Time.fixedDeltaTime , pos , part.transform.localPosition + shoulder.transform.localPosition);
		ApplyForce(Vector3.Scale(correction, factor));
		*/
		Vector3 error = pos - part.transform.position;

		//Vector3 correction = headingController.UpdateNormalPid(error, Time.fixedDeltaTime, pos, part.transform.position);
		Vector3 correction = headingController.UpdateNormalPid(error, Time.fixedDeltaTime, pos, part.transform.position);
		ApplyForce(Vector3.Scale(correction, factor));
	}

	/// <summary>
	/// applys force to the part
	/// </summary>
	/// <param name="force"></param>
	public void ApplyForce(Vector3 force)
	{
		//rBody.AddForce(force, ForceMode.Acceleration);
		rBody.AddForce(new Vector3(Mathf.Clamp(force.x, -100, 100), Mathf.Clamp(force.y, -100, 100), Mathf.Clamp(force.z, -100, 100)), ForceMode.Acceleration);
	}


	/// <summary>
	/// dont want to do this... 
	/// </summary>
	public void appExit()
	{
		headingController.appExit();
	}
}