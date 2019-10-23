using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PID controller with 3 values at once
/// works like 3 pid controllers with 1 value each
/// </summary>
public class VectorPid : MonoBehaviour
{
	public float pFactor, iFactor, dFactor;

	protected Vector3 integral;
	protected Vector3 lastError;

	public VectorPid(float pFactor, float iFactor, float dFactor)
	{
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}

	public virtual Vector3 UpdateNormalPid(Vector3 currentError, float timeFrame, Vector3 target, Vector3 current)
	{
		integral += currentError * timeFrame;
		var deriv = (currentError - lastError) / timeFrame;
		lastError = currentError;
		return currentError * pFactor
			+ integral * iFactor
			+ deriv * dFactor;
	}

	public virtual Vector3 UpdateModifiedPid(Vector3 currentError, float timeFrame)
	{
		//reduce integrall with time to eliminate huge error when walking moving against walls, ez;
		//only neccesarry for rotations since the axis can change
		integral *= 0.98f;

		integral += currentError * timeFrame;
		var deriv = (currentError - lastError) / timeFrame;
		lastError = currentError;
		return currentError * pFactor
			+ integral * iFactor
			+ deriv * dFactor;
	}

	/// <summary>
	/// only used from child -.....
	/// </summary>
	public virtual void appExit()
	{
		return;
	}
}