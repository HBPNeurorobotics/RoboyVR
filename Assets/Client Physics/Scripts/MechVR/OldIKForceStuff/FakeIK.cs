using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IK without Physics
/// </summary>
public class FakeIK
{
	public GameObject c0;
	public GameObject c1;
	public GameObject c2;
	public GameObject c3;

	public GameObject target;

	/// <summary>
	/// forward direction of the LOCAL space
	/// </summary>
	public Vector3 forward;

	//length of the leg parts
	public float length1;
	public float length2;

	public FakeIK(GameObject c0, GameObject c1, GameObject c2, GameObject c3, GameObject target)
	{
		this.c0 = c0;
		this.c1 = c1;
		this.c2 = c2;
		this.c3 = c3;
		this.target = target;

		length1 = c1.transform.localPosition.magnitude;
		length2 = c2.transform.localPosition.magnitude - length1;

		//! differend def of forward that in IKJOINT
		forward = c0.transform.parent.parent.localRotation * c1.transform.localPosition.normalized;
	}

	/// <summary>
	/// Update the fake legs. Called every frame
	/// Can throw some error when target is in h0
	/// </summary>
	public void UpdateAll()
	{
		/*
		Vector3 tPosition = c0.transform.InverseTransformPoint(target.transform.position);

		float totalDistance = (target.transform.position - c0.transform.position).magnitude;

		float elbowAngle = 0;
		if (totalDistance < length1 + length2)
		{
			elbowAngle = Mathf.Acos((length2 * length2 - totalDistance * totalDistance - length1 * length1) / (-2 * length1 * totalDistance)) * Mathf.Rad2Deg;
		}

		//offset towards elbowtarget

		//still some error here (when tPosition is in c0.transform)
		Quaternion offsetQuat = Quaternion.AngleAxis(elbowAngle, Vector3.Cross(elbowTarget.transform.localPosition, tPosition));



		c1.transform.localPosition = offsetQuat * Quaternion.FromToRotation(forward, tPosition) * forward * length1;
		c1.transform.localRotation = Quaternion.FromToRotation(forward, c1.transform.localPosition);

		c2.transform.localPosition = Quaternion.FromToRotation(forward, tPosition - c1.transform.localPosition) * forward * length2 + c1.transform.localPosition;
		c2.transform.localRotation = Quaternion.FromToRotation(forward, c2.transform.localPosition - c1.transform.localPosition);

		c3.transform.localPosition = c2.transform.localPosition;
		c3.transform.localRotation =  Quaternion.Inverse(target.transform.rotation);

		*/

		//coveres arm orientation
		//LOCAL positions
		Vector3 pos1;
		Vector3 pos2;
		Vector3 pos3;

		Quaternion rot1;
		Quaternion rot2;
		Quaternion rot3;

		Vector3 tPosition2 = target.transform.localPosition;
		//tPosition2.x *= -forward.x;



		float totalDistance = (target.transform.localPosition).magnitude;

		float oldLength1 = length1;
		float oldLength2 = length2;
		if (totalDistance > (length1 + length2) * 0.98f)
		{
			length1 = (totalDistance * 1.02f) * (oldLength1 / (oldLength1 + oldLength2));
			length2 = (totalDistance * 1.02f) * (oldLength2 / (oldLength1 + oldLength2));
			c1.transform.localScale = new Vector3(length1 / oldLength1, 1, 1);
			c2.transform.localScale = new Vector3(length2 / oldLength2, 1, 1);
		}
		else
		{
			c1.transform.localScale = Vector3.one;
			c2.transform.localScale = Vector3.one;
		}

		//NAN testFix
		if (totalDistance <= 0.8f)
		{
			//print("saved");
			//return;
		}

		float elbowAngle = 0;
		if (totalDistance < length1 + length2)
		{
			elbowAngle = Mathf.Acos((length2 * length2 - totalDistance * totalDistance - length1 * length1) / (-2 * length1 * totalDistance)) * Mathf.Rad2Deg;
		}

		//offset rotates perpendicular to target
		Quaternion offsetQuat = new Quaternion();//Quaternion.AngleAxis(elbowAngle, Vector3.Cross(-elbowTarget.transform.position, (h0.transform.InverseTransformPoint(target.transform.position))));

		//Vector3 tmp = h0.transform.InverseTransformPoint(target.transform.position);

		//version 2: More natural, no fuckup when at shoulder
		Quaternion tmp2 = Quaternion.Euler(0, 0, -90 * forward.x);




		//TODO: if distance is longer than arm: scale the axis of the arms and all positions by faktor

		if (forward.x < 0)
		{
			//version 4, bit better but got some bad spots above/behind shoulder
			tmp2 = Quaternion.Euler(45, 0, -45);

			offsetQuat = Quaternion.AngleAxis(elbowAngle, Vector3.Cross(tmp2 * tPosition2, tPosition2));


			pos1 = offsetQuat * Quaternion.FromToRotation(Vector3.right, tPosition2) * Vector3.right * length1;
			pos2 = Quaternion.FromToRotation(Vector3.right, tPosition2 - pos1) * Vector3.right * length2 + pos1;
			if (elbowAngle == 0f)
			{
				rot1 = Quaternion.FromToRotation(Vector3.left, pos1);
				rot2 = rot1;
			}
			else
			{
				rot1 = Quaternion.LookRotation(pos1, pos2) * Quaternion.FromToRotation(Vector3.left, Vector3.forward);
				rot2 = Quaternion.LookRotation(pos2 - pos1, -pos1) * Quaternion.FromToRotation(Vector3.left, Vector3.forward);
			}
			rot3 = target.transform.localRotation * Quaternion.Euler(180, 180, -90 * forward.x);
		}
		else
		{
			//version 4, bit better but got some bad spots above/behind shoulder
			tmp2 = Quaternion.Euler(45, 0, 45);

			offsetQuat = Quaternion.AngleAxis(elbowAngle, Vector3.Cross(tmp2 * tPosition2, tPosition2));


			pos1 = offsetQuat * Quaternion.FromToRotation(Vector3.right, tPosition2) * Vector3.right * length1;
			pos2 = Quaternion.FromToRotation(Vector3.right, tPosition2 - pos1) * Vector3.right * length2 + pos1;

			if (elbowAngle == 0f)
			{
				rot1 = Quaternion.FromToRotation(Vector3.right, pos1);
				rot2 = rot1;
			}
			else
			{
				rot1 = Quaternion.LookRotation(pos1, pos2) * Quaternion.FromToRotation(Vector3.right, Vector3.forward);
				rot2 = Quaternion.LookRotation(pos2 - pos1, -pos1) * Quaternion.FromToRotation(Vector3.right, Vector3.forward);
			}
			rot3 = target.transform.localRotation * Quaternion.Euler(180, 180, -90 * forward.x);
		}

		c1.transform.localPosition = pos1;
		c2.transform.localPosition = pos2;
		c1.transform.localRotation = rot1;
		c2.transform.localRotation = rot2;

		length1 = oldLength1;
		length2 = oldLength2;
	}
}
