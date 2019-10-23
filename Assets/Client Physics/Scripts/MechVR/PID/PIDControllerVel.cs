using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vel PID.
/// Also has special Method for the Mech movement control with a rolling ball
/// </summary>
public class PIDControllerVel : MonoBehaviour
{

	private VectorPid pid;

	public GameObject part;

	private Rigidbody rBody;

	private Vector3 factor;

	private float maxForce;

	/// <summary>
	/// factor is in this case (1,0,1)
	/// </summary>
	public PIDControllerVel(GameObject part, float p, float i, float d, Vector3 factor , float maxForce)
	{
		this.part = part;
		rBody = part.GetComponent<Rigidbody>();
		pid = new VectorPid(p, i, d);
		//pid = new VectorPIDValueLogger(p, i, d , part.name);
		this.factor = factor;
		this.maxForce = maxForce;
	}

	/// <summary>
	/// Has to be called every FixedUpdate
	/// Calculates the force with PID controllers to move the object to the target Vel
	/// </summary>
	/// <param name="rotation"></param>
	public void UpdateTarget(Vector3 vel, float airFactor)
	{
		Vector3 error = vel - rBody.velocity;

		Vector3 correction = pid.UpdateNormalPid(error, Time.fixedDeltaTime, vel , rBody.velocity);
		ApplyForce(Vector3.Scale(correction, factor) * airFactor);
	}

	/// <summary>
	/// applys force to the part as acceleration, and also as torque
	/// </summary>
	public void ApplyForce(Vector3 force )
	{
		//rBody.AddForce(force, ForceMode.Acceleration);
		//Debug.Log(new Vector3(Mathf.Clamp(force.x, -maxForce, maxForce), 0 * Mathf.Clamp(force.y, -maxForce, maxForce), Mathf.Clamp(force.z, -maxForce, maxForce)));
		//rBody.AddForce(new Vector3(Mathf.Clamp(force.x, -maxForce, maxForce), 0 * Mathf.Clamp(force.y, -maxForce, maxForce), Mathf.Clamp(force.z, -maxForce, maxForce)), ForceMode.Acceleration);
		rBody.AddForce(force, ForceMode.Acceleration);
		/*
		if (force.magnitude < maxForce)
		{
			rBody.AddForce(force , ForceMode.Acceleration);
			//Debug.Log(force);
		}
		else
		{
			rBody.AddForce(force.normalized * maxForce, ForceMode.Acceleration);
			//Debug.Log(force.normalized * maxForce);
		}*/
		rBody.AddTorque(new Vector3(force.z,0 , -force.x)*0.3f, ForceMode.Acceleration);
	}

	/// <summary>
	/// dont want to do this... 
	/// </summary>
	public void appExit()
	{
		pid.appExit();
	}
}