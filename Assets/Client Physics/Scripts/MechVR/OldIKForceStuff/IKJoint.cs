using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// old version with configurable joints
/// </summary>
public class IKJoint
{

	public GameObject c0;
	public GameObject c1;
	public GameObject c2;
	public GameObject c3;

	public GameObject target;
	public GameObject elbowTarget;

	private ConfigurableJoint h0;
	private ConfigurableJoint h1;
	private ConfigurableJoint h2;

	public float length1;
	public float length2;

	private Quaternion targetRot0;
	private Quaternion targetRot1;
	private Quaternion targetRot2;

	public Vector3 forward;

	public float maxForce;
	public float defaultSpring;
	public float defaultDampening;

	public IKJoint(GameObject c0, GameObject c1, GameObject c2, GameObject c3, GameObject target, GameObject elbowTarget, float maxForce, float defaultSpring, float defaultDampening)
	{
		this.c0 = c0;
		this.c1 = c1;
		this.c2 = c2;
		this.c3 = c3;
		this.target = target;
		this.elbowTarget = elbowTarget;
		this.maxForce = maxForce;
		this.defaultSpring = defaultSpring;
		this.defaultDampening = defaultDampening;

		var parentTrans = c0.transform.parent;
		var rot = parentTrans.localRotation;
		length1 = c1.transform.localPosition.magnitude;
		length2 = c2.transform.localPosition.magnitude - length1;

		forward = c0.transform.parent.parent.localRotation * c1.transform.localPosition.normalized;

		InitializeParent(Quaternion.FromToRotation(Vector3.right, c1.transform.localPosition - c0.transform.localPosition));
		//parentTrans.localRotation = Quaternion.FromToRotation(Vector3.right, c1.transform.localPosition - c0.transform.localPosition);


		h0 = SetJoint(c0, c1);
		h1 = SetJoint(c1, c2);
		h2 = SetJoint(c2, c3);

		//we basicly have 1 weak point for both h0 and h1.
		//have to rotate parent in a way that it doesnt effect ik much DOESNT WORK i think
	}

	void InitializeParent(Quaternion newRotation)
	{
		var parent = c1.transform.parent;

		foreach (Transform child in parent)
		{
			child.localPosition = Quaternion.Inverse(newRotation) * child.localPosition;
			foreach (Transform childChild in child)
			{
				childChild.localPosition = Quaternion.Inverse(newRotation) * childChild.localPosition;
				childChild.localRotation = Quaternion.Inverse(newRotation);
			}
			//child.rotation = child.rotation * Quaternion.Inverse(newRotation);
		}
		//this if not changing childs
		//originRot = parent.rotation;

		//this if position of child is fixed? -> orientation of mesh has to be fixed now
		//originRot = new Quaternion(0, 0, 0, 1);

		parent.localRotation = parent.transform.localRotation * newRotation;
	}

	//only a bit right.(not used atm)
	void RotateParent2(Quaternion rotChange)
	{
		var parent = c1.transform.parent;
		parent.rotation = rotChange * parent.rotation;
		foreach (Transform child in parent)
		{
			child.localPosition = Quaternion.Inverse(rotChange) * child.localPosition;
			//child.rotation = child.rotation * Quaternion.Inverse(newRotation);

			foreach (Transform childChild in child)
			{
				childChild.localPosition = Quaternion.Inverse(rotChange) * childChild.localPosition;
				childChild.localRotation = childChild.localRotation * rotChange;
			}
		}
		//this if not changing childs
		//originRot = parent.rotation;

		//this if position of child is fixed? -> orientation of mesh has to be fixed now
	}

	ConfigurableJoint SetJoint(GameObject go, GameObject connectedBody)
	{
		var rot = go.transform.localRotation;
		var joint = go.AddComponent<ConfigurableJoint>();

		//TODO: !!!!! test remove * 100 later 
		var drive = new JointDrive();
		drive.positionSpring = defaultSpring * 100;
		drive.positionDamper = defaultDampening;
		drive.maximumForce = maxForce * 100;

		joint.connectedBody = connectedBody.GetComponent<Rigidbody>();
		joint.anchor = Vector3.zero;
		joint.axis = Vector3.zero;
		joint.secondaryAxis = Vector3.zero;
		joint.angularXDrive = drive;
		joint.angularYZDrive = drive;
		joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;

		return joint;
	}

	//old
	public void UpdateTargetRots()
	{
		//TODO: look of target or self has changed -> if not only call drive update

		//replace every h0.transform.parent with h0.transform to get movement relative to real location, not the the location the arm should be
		//with parent thing it is possible to have force of the legs/arms effect the cockpit reasonable
		//hand/food orientation you have to outcomment the right line

		float totalDistance = h0.transform.parent.InverseTransformPoint((target.transform.position)).magnitude;

		float elbowAngle = 0;
		if (totalDistance < length1 + length2)
		{
			elbowAngle = Mathf.Acos((length2 * length2 - totalDistance * totalDistance - length1 * length1) / (-2 * length1 * totalDistance)) * Mathf.Rad2Deg;
		}

		//offset rotates perpendicular to target
		Quaternion offsetQuat = Quaternion.AngleAxis(elbowAngle, Vector3.Cross(-h0.transform.parent.InverseTransformPoint(elbowTarget.transform.position), (h0.transform.parent.InverseTransformPoint(target.transform.position))));

		targetRot0 = Quaternion.FromToRotation(Vector3.right, offsetQuat * (h0.transform.parent.InverseTransformPoint(target.transform.position)));
		if (totalDistance < length1 + length2)
		{
			//old line:
			//targetRot1 = Quaternion.FromToRotation(test, h1.transform.InverseTransformPoint(target.transform.position));

			//massiv improvement: calc where h1 should be and then do above calc with that information
			//the pos h1 should be
			var pos = h0.transform.parent.rotation * targetRot0 * Vector3.right * length1 + h0.transform.parent.position;
			var posTmp = h1.transform.position;
			var rotTmp = h1.transform.rotation;

			h1.transform.position = pos;
			h1.transform.rotation = h0.transform.parent.rotation * targetRot0;
			targetRot1 = Quaternion.FromToRotation(Vector3.right, h1.transform.InverseTransformPoint(target.transform.position));

			h1.transform.position = posTmp;
			h1.transform.rotation = rotTmp;
		}
		else
		{
			targetRot1 = Quaternion.identity;
		}

		//the parent is not allowed to rotate. When adding an object to the hand add inverse of parrent rotation to it.
		//the 2. line is foot orientation fixed when mech rotates
		//targetRot2 = Quaternion.Inverse(c2.transform.rotation) * target.transform.rotation * c0.transform.parent.localRotation;
		targetRot2 = c0.transform.localRotation * Quaternion.Inverse(c2.transform.rotation) * target.transform.rotation * c0.transform.parent.localRotation;

		h0.targetRotation = targetRot0;
		h1.targetRotation = targetRot1;
		h2.targetRotation = targetRot2;
		UpdateDrive();
	}

	public void UpdateTargetRotsGlobal()
	{
		//TODO: look of target or self has changed -> if not only call drive update

		float totalDistance = h0.transform.InverseTransformPoint((target.transform.position)).magnitude;

		float elbowAngle = 0;
		if (totalDistance < length1 + length2)
		{
			elbowAngle = Mathf.Acos((length2 * length2 - totalDistance * totalDistance - length1 * length1) / (-2 * length1 * totalDistance)) * Mathf.Rad2Deg;
		}

		//offset rotates perpendicular to target
		Quaternion offsetQuat = Quaternion.AngleAxis(elbowAngle, Vector3.Cross(-elbowTarget.transform.position, (h0.transform.InverseTransformPoint(target.transform.position))));

		Vector3 tmp = h0.transform.InverseTransformPoint(target.transform.position);

		//version 2: More natural, no fuckup when at shoulder
		Quaternion tmp2 = Quaternion.Euler(0, 0, 90);

		//version 4, bit better but got some bad spots above/behind shoulder
		tmp2 = Quaternion.Euler(-45 * forward.x, 0, 45);

		tmp2 = Quaternion.Euler(-45 * forward.x, 0, 45);


		offsetQuat = Quaternion.AngleAxis(elbowAngle, Vector3.Cross(tmp2 * tmp, tmp));


		Debug.DrawRay(h0.transform.position, h0.transform.parent.parent.localRotation * tmp, Color.yellow);
		Debug.DrawRay(h0.transform.position, h0.transform.parent.parent.localRotation * Vector3.Cross(tmp2 * tmp, tmp), Color.blue);
		Debug.DrawRay(h0.transform.position, h0.transform.parent.parent.localRotation * tmp2 * tmp, Color.red);

		targetRot0 = Quaternion.FromToRotation(Vector3.right, offsetQuat * (h0.transform.InverseTransformPoint(target.transform.position)));
		if (totalDistance < length1 + length2)
		{
			//old line:
			//targetRot1 = Quaternion.FromToRotation(test, h1.transform.InverseTransformPoint(target.transform.position));

			//massiv improvement: calc where h1 should be and then do above calc with that information
			//the pos h1 should be
			var pos = h0.transform.rotation * targetRot0 * Vector3.right * length1 + h0.transform.position;
			var posTmp = h1.transform.position;
			var rotTmp = h1.transform.rotation;

			h1.transform.position = pos;
			h1.transform.rotation = h0.transform.rotation * targetRot0;
			targetRot1 = Quaternion.FromToRotation(Vector3.right, h1.transform.InverseTransformPoint(target.transform.position));

			h1.transform.position = posTmp;
			h1.transform.rotation = rotTmp;
		}
		else
		{
			targetRot1 = Quaternion.identity;
		}

		//the parent is not allowed to rotate. When adding an object to the hand add inverse of parrent rotation to it.

		//vive idle points upwards : Quaternion.FromToRotation(Vector3.up, Vector3.left)* 
		//targetRot2 = Quaternion.Inverse(c2.transform.rotation) * target.transform.localRotation * Quaternion.FromToRotation(forward, Vector3.right)  * Quaternion.Euler(0,0,90);
		//idle jhand back up
		//targetRot2 = Quaternion.Inverse(c2.transform.rotation) * target.transform.localRotation  * Quaternion.Euler(0, 90, -180);
		//Idle forward, Back to outside
		//targetRot2 = Quaternion.Inverse(c2.transform.rotation) * target.transform.localRotation * Quaternion.Euler(90, 90, 180) * Quaternion.FromToRotation(Quaternion.FromToRotation(Vector3.right, Vector3.up) * forward, Vector3.up);
		//Idle down, back to outside
		targetRot2 = Quaternion.Inverse(c2.transform.rotation) * target.transform.localRotation * Quaternion.Euler(180, 0, 90) * Quaternion.FromToRotation(Quaternion.FromToRotation(Vector3.right, Vector3.down) * forward, Vector3.down);
		//TODO: think Quaternion.Euler(180, 0, 90) is cockpitoriginrot


		h0.targetRotation = targetRot0;
		h1.targetRotation = targetRot1;
		h2.targetRotation = targetRot2;
		UpdateDrive();
	}

	void UpdateDrive()
	{
		//red = actual rotation, blue = targetrotation
		var rot0 = c1.transform.rotation;
		var rot1 = c2.transform.rotation;
		var rot2 = c3.transform.rotation;
		var rotTarget0 = h0.transform.rotation * targetRot0;
		var rotTarget1 = h1.transform.rotation * targetRot1;
		var rotTarget2 = h2.transform.rotation * targetRot2;

		Debug.DrawRay(h0.transform.position, rot0 * Vector3.right, Color.red);
		Debug.DrawRay(h1.transform.position, rot1 * Vector3.right, Color.red);
		Debug.DrawRay(h2.transform.position, rot2 * Vector3.right, Color.red);

		Debug.DrawRay(h0.transform.position, rotTarget0 * Vector3.right, Color.blue);
		Debug.DrawRay(h1.transform.position, rotTarget1 * Vector3.right, Color.blue);
		Debug.DrawRay(h2.transform.position, rotTarget2 * Vector3.right, Color.blue);

		//angle difference between real rot and target
		float distance0 = Quaternion.Angle(rot0, rotTarget0);
		float distance1 = Quaternion.Angle(rot1, rotTarget1);
		float distance2 = Quaternion.Angle(rot2, rotTarget2);

		//easy version, can be improved by looking at angular momentum
		//for damper the angular momentum is neccesarry or its likely to happen that the arm circles around the target

		var drive = new JointDrive();

		//TODO: reduce force of the outer joints

		//idea: another value for the forces: weight of the stuff in the hand (at least a bit)

		//for the forces, think hand is not as powerfull als other elements

		//first bone
		//drive.positionSpring = 100 + 4000 * (distance0 / 180);
		drive.positionSpring = CalcForce(distance0);
		//drive.positionDamper = 50 + 500 * (360 - distance0) / 360;
		drive.positionDamper = CalcDamper(distance0);
		//TODO: look at maxforce, change???
		drive.maximumForce = 500;

		h0.angularXDrive = drive;
		h0.angularYZDrive = drive;

		//second bone
		//drive.positionSpring = 100 + 2000 * (distance1 / 180);
		drive.positionSpring = CalcForce(distance1);
		//drive.positionDamper = 50 + 500 * (360 - distance1) / 360;
		drive.positionDamper = CalcDamper(distance1);

		h1.angularXDrive = drive;
		h1.angularYZDrive = drive;

		//hand
		drive.positionSpring = CalcForce(distance2) / 5;
		drive.positionDamper = CalcDamper(distance2);

		h2.angularXDrive = drive;
		h2.angularYZDrive = drive;


		var rb0 = c0.GetComponent<Rigidbody>();
		var rb1 = c1.GetComponent<Rigidbody>();
		var rb2 = c2.GetComponent<Rigidbody>();
		var rb3 = c3.GetComponent<Rigidbody>();



	}
	/// <summary>
	/// Calc force for joint
	/// </summary>
	/// <param name="x"> input is the angle diff of real and target rotation</param>
	/// <returns>force</returns>
	float CalcForce(float x)
	{
		float result = 0;
		//strong exponential, want more force when close and probably not that much when over 100
		result = 200 + 25 * x + x * x / 3f;
		//
		result = 200+ Mathf.Log10(x / 10 + 1) * 600 + 5*x;
		return result;
	}
	float CalcDamper(float x)
	{
		float result = 0;
		result = 70 - Mathf.Log10(x / 180) * 200;
		return result;
	}

	public void FixedUpdate()
	{
		var rb1 = c1.GetComponent<Rigidbody>();
		var rb2 = c2.GetComponent<Rigidbody>();
		var rb3 = c3.GetComponent<Rigidbody>();
		/*
		if (rb1.angularVelocity.magnitude > 1 * Time.deltaTime)
		{
			rb1.angularVelocity = rb1.angularVelocity.normalized * Time.deltaTime / 10000;
			rb1.angularVelocity = Vector3.zero;
		}
		if (rb2.angularVelocity.magnitude > 1 * Time.deltaTime)
		{
			rb2.angularVelocity = rb2.angularVelocity.normalized * Time.deltaTime / 10000;
			rb2.angularVelocity = Vector3.zero;
		}
		if (rb2.angularVelocity.magnitude > 1 * Time.deltaTime)
		{
			rb3.angularVelocity = rb3.angularVelocity.normalized * Time.deltaTime / 10000;
			rb3.angularVelocity = Vector3.zero;
		}
		rb1.angularVelocity = Vector3.zero;
		rb2.angularVelocity = Vector3.zero;
		rb3.angularVelocity = Vector3.zero;*/
	}
}
