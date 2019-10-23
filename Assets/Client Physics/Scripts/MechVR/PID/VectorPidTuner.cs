using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PID controller with 3 values at once
/// works like 3 pid controllers with 1 value each
/// </summary>
public class VectorPidTuner : MonoBehaviour
{
	public float pFactor, iFactor, dFactor;
	public Vector3 p, i, d;
	public GameObject target;
	public GameObject cam;
	public GameObject self;

	private Rigidbody rBody;
	private Vector3 integral;
	private Vector3 lastError;

	List<Vector3> history1;
	List<Vector3> history2;
	List<Vector3> history3;

	Vector3 test;

	public AnimationCurve curveTest;

	/*
	public VectorPidTuner(float pFactor, float iFactor, float dFactor)
	{
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}*/

	public void Start()
	{
		rBody = self.GetComponent<Rigidbody>();
		history1 = new List<Vector3>();
		history2 = new List<Vector3>();
		history3 = new List<Vector3>();
		test = Vector3.zero;
	}

	public void FixedUpdate()
	{
		/*
		Vector3 error = target.transform.position - self.transform.position;
		Vector3 correction = calc(error, Time.fixedDeltaTime);
		ApplyForce(correction);*/

		
		var force = calcForce(target.transform.rotation, self.transform.rotation);
		force = Vector3.Lerp(force, test, 0.45f);
		test = force;
		ApplyRotForce(force);

		PlotList(history1, new Vector3(0, 0, 0), Color.red, 3);
		PlotList(history2, new Vector3(0, 0, 0), Color.green, 3);
		PlotList(history3, new Vector3(0, 0, 0), Color.black, 1);


	}

	public Vector3 calcForce(Quaternion targetRotation, Quaternion ownRotation)
	{

		//test at combining all errors
		Vector3 headingError = Vector3.zero;

		//Just add up the errors of all 3 axis and use one PID for all
		var desiredHeading = targetRotation * Vector3.forward;
		var currentHeading = ownRotation * Vector3.forward;
		headingError += Vector3.Cross(currentHeading, desiredHeading);

		desiredHeading = targetRotation * Vector3.right;
		currentHeading = ownRotation * Vector3.right;
		headingError += Vector3.Cross(currentHeading, desiredHeading);

		desiredHeading = targetRotation * Vector3.up;
		currentHeading = ownRotation * Vector3.up;
		headingError += Vector3.Cross(currentHeading, desiredHeading);

		//print(headingError.magnitude);

		var headingCorrection = calc(headingError, Time.fixedDeltaTime);

		return headingCorrection;
	}

	/// <summary>
	/// applies force to the part
	/// </summary>
	/// <param name="force"></param>
	public void ApplyRotForce(Vector3 force)
	{
		//rBody.AddTorque(new Vector3(Mathf.Clamp(force.x, -100, 100), Mathf.Clamp(force.y, -100, 100), Mathf.Clamp(force.z, -100, 100)), ForceMode.Acceleration);
		rBody.AddTorque(force, ForceMode.Acceleration);
	}

	/*
	public void UpdateTarget(Quaternion rotation)
	{
		if (rotation.x == float.NaN)
		{
			print("saved1");
			return;
		}
		//global:
		var force = calcForce(rotation, part.transform.rotation);
		ApplyForce(force);
	}*/

	public Vector3 calc(Vector3 currentError, float timeFrame)
	{
		//reduce integrall with time to eliminate huge error when walking against a wall
		//integral *= 0.99f;

		integral += currentError * timeFrame;
		var deriv = (currentError - lastError) / timeFrame;
		lastError = currentError;

		//curveTest.AddKey(Time.time, currentError.x);
		history1.Add(currentError);
		history2.Add(integral);
		history3.Add(deriv);
	
		return currentError * pFactor
			+ integral * iFactor
			+ deriv * dFactor;

		//return Vector3.Scale(currentError, p) + Vector3.Scale(integral, i) + Vector3.Scale(deriv, d);
	}

	/// <summary>
	/// applys force to the part
	/// </summary>
	/// <param name="force"></param>
	public void ApplyForce(Vector3 force)
	{
		rBody.AddForce(new Vector3(Mathf.Clamp(force.x, -100, 100), Mathf.Clamp(force.y, -100, 100), Mathf.Clamp(force.z, -100, 100)), ForceMode.Acceleration);


		/*
		if(force.magnitude >= 100)
		{
			rBody.AddForce(force.normalized * 20, ForceMode.Acceleration);
		}
		else
		{
			rBody.AddForce(force, ForceMode.Acceleration);
		}
		*/
	}

	public void PlotList(List<Vector3> vecList, Vector3 offset, Color color, float factor)
	{
		Vector3 newOffset = Vector3.zero;
		for (int i = 0; i < vecList.Count - 1; i++)
		{
			newOffset = offset + cam.transform.position + new Vector3(0, 50, -20);
			Debug.DrawLine(
				new Vector3(((float)i / vecList.Count - 0.5f) * 100 + newOffset.x, newOffset.y, vecList[i].x * factor + newOffset.z),
				new Vector3(((float)(i + 1) / vecList.Count - 0.5f) * 100 + newOffset.x, newOffset.y, vecList[1 + i].x * factor + newOffset.z),
				color);

			newOffset = offset + cam.transform.position + new Vector3(0, 50, 0);
			Debug.DrawLine(
				new Vector3(((float)i / vecList.Count - 0.5f) * 100 + newOffset.x, newOffset.y, vecList[i].y * factor + newOffset.z),
				new Vector3(((float)(i + 1) / vecList.Count - 0.5f) * 100 + newOffset.x, newOffset.y, vecList[1 + i].y * factor + newOffset.z),
				color);

			newOffset = offset + cam.transform.position + new Vector3(0, 50, 20);
			Debug.DrawLine(
				new Vector3(((float)i / vecList.Count - 0.5f) * 100 + newOffset.x, newOffset.y, vecList[i].z * factor + newOffset.z),
				new Vector3(((float)(i + 1) / vecList.Count - 0.5f) * 100 + newOffset.x, newOffset.y, vecList[1 + i].z * factor + newOffset.z),
				color);

			if (i % 20 == 0)
			{
				Debug.DrawLine(new Vector3(((float)i / vecList.Count - 0.5f) * 100, newOffset.y, 100), new Vector3(((float)i / vecList.Count - 0.5f) * 100, newOffset.y, -100), Color.white);
			}
		}
		Debug.DrawLine(new Vector3(-100, newOffset.y, -20), new Vector3(100, newOffset.y, -20), Color.yellow);
		Debug.DrawLine(new Vector3(-100, newOffset.y, 0), new Vector3(100, newOffset.y, 0), Color.yellow);
		Debug.DrawLine(new Vector3(-100, newOffset.y, 20), new Vector3(100, newOffset.y, 20), Color.yellow);
		print(vecList.Count);
	}
}