using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PID controller to rotate a gameobject to a Quaternion rotation with unity forces
/// code initially from http://answers.unity3d.com/questions/199055/addtorque-to-rotate-rigidbody-to-look-at-a-point.html
/// and heavily reworked
/// </summary>
public class PIDControllerRot : MonoBehaviour
{
	private VectorPid headingController;

	/// <summary>
	/// the part(gameobject) this PID controlls
	/// </summary>
	public GameObject part;

	/// <summary>
	/// the rigidbody of part (rigidbody must not change)
	/// </summary>
	private Rigidbody rBody;

	/// <summary>
	/// the last force so we can lerp with new force -> still smooth for the easier rotation
	/// </summary>
	private Vector3 lastForce;

	public PIDControllerRot(GameObject part, float p, float i, float d)
	{
		this.part = part;
		rBody = part.GetComponent<Rigidbody>();
		headingController = new VectorPid(p, i, d);
		//headingController = new VectorPIDValueLogger(p, i, d , part.name);
	}

	/// <summary>
	/// Has to be called every FixedUpdate
	/// Calculates the force with PID controllers to rotate the object to the GLOBALrotation
	/// </summary>
	/// <param name="rotation"></param>
	public void UpdateTarget(Quaternion rotation)
	{
		if (rotation.x == float.NaN)
		{
			Debug.Log("Saved NAN in PIDControllerRot");
			return;
		}
		//global:
		var force = calcForce(rotation, part.transform.rotation);
		//TODO: probably have to increase the lerp factor to 0.6f
		force = Vector3.Lerp(force, lastForce, 0.5f);
		lastForce = force;
		ApplyForce(force);
	}

	/// <summary>
	/// UNUSED because it doesnt work
	/// Has to be called every FixedUpdate
	/// Calculates the force with PID controllers to rotate the object to the LOCALrotation
	/// </summary>
	/// <param name="rotation"></param>
	[System.Obsolete("method was never working like intended")]
	public void UpdateTargetLocal(Quaternion rotation)
	{
		if (rotation.x == float.NaN)
		{
			Debug.Log("saved1");
			return;
		}
		//global:
		//var force = calcForce(rotation , part.transform.rotation);
		//ApplyForce(force);

		//local
		var force = calcForce(rotation, part.transform.localRotation);
		ApplyForceLocal(force);
	}

	/// <summary>
	/// Has to be called every FixedUpdate
	/// </summary>
	/// <param name="rotation"></param>
	/// <param name="connectedBody">the rigidbody this object is connected with a fixed joint</param>
	public void UpdateTarget(Quaternion rotation, Rigidbody connectedBody)
	{
		if (rotation.x == float.NaN)
		{
			Debug.Log("saved2");
			return;
		}
		var force = calcForce(rotation, part.transform.rotation);
		force = Vector3.Lerp(force, lastForce, 0.5f);
		lastForce = force;
		ApplyForce(force, connectedBody);
	}

	public Vector3 calcForce(Quaternion targetRotation, Quaternion ownRotation)
	{

		//test at combining all errors
		Vector3 headingError = Vector3.zero;

		//Just add up the errors of all 3 axis and use one PID for all
		Vector3 desiredHeading = targetRotation * Vector3.forward;
		Vector3 currentHeading = ownRotation * Vector3.forward;
		headingError += Vector3.Cross(currentHeading, desiredHeading);

		desiredHeading = targetRotation * Vector3.right;
		currentHeading = ownRotation * Vector3.right;
		headingError += Vector3.Cross(currentHeading, desiredHeading);

		desiredHeading = targetRotation * Vector3.up;
		currentHeading = ownRotation * Vector3.up;
		headingError += Vector3.Cross(currentHeading, desiredHeading);

		//Debug.Log(headingError.magnitude);

		var headingCorrection = headingController.UpdateModifiedPid(headingError, Time.fixedDeltaTime);

		return headingCorrection;
	}

	/// <summary>
	/// applies force to the part
	/// </summary>
	/// <param name="force"></param>
	public void ApplyForce(Vector3 force)
	{
		rBody.AddTorque(force, ForceMode.Acceleration);

	}

	/// <summary>
	/// idea to counteract the extra weight of the weapon
	/// </summary>
	/// <param name="force"></param>
	/// <param name="connectedBody"></param>
	public void ApplyForce(Vector3 force, Rigidbody connectedBody)
	{
		//of changing the forcemode 
		//rBody.AddTorque(force, ForceMode.Acceleration);
		//Quaternion test = rBody.transform.rotation * rBody.inertiaTensorRotation;
		//Vector3 tmp = test * Vector3.Scale(rBody.inertiaTensor, Quaternion.Inverse(test) * force);

		//test = connectedBody.transform.rotation * connectedBody.inertiaTensorRotation;
		//tmp += test * Vector3.Scale(connectedBody.inertiaTensor, Quaternion.Inverse(test) * force);

		//rBody.AddTorque(tmp, ForceMode.Force);

		rBody.AddTorque(force, ForceMode.Acceleration);

		//only apply the force when the connected body is a weapon not a random object
		//NOT WORKING LIKE THAT
		
		LayerMask mask = LayerMask.GetMask(new string[] { "Weapon" });
		if (LayerMask.NameToLayer("Weapon") == connectedBody.gameObject.layer)
		{
			connectedBody.AddTorque(force, ForceMode.Acceleration);
			Debug.Log("weapon");
		}

		//if(connectedBody.gameObject)
		

		//when values are local space, currently not working
		//rBody.AddRelativeTorque(force * 1f, ForceMode.Force);
	}


	/// <summary>
	/// UNUSED because it doesnt work
	/// applies force to the part
	/// Seems to be a failed experiment
	/// </summary>
	/// <param name="force"></param>
	[System.Obsolete("method was never working like intended")]
	public void ApplyForceLocal(Vector3 force)
	{
		rBody.AddRelativeTorque(force, ForceMode.Acceleration);
	}

	/// <summary>
	/// dont want to do this... 
	/// </summary>
	public void appExit()
	{
		headingController.appExit();
	}
}