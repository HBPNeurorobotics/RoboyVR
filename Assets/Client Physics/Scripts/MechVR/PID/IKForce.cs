using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the hole arm movement/rotation with PID Controllers
/// </summary>
public class IKForce : MonoBehaviour
{
	/// <summary>
	/// 0 = arm;
	/// 1 = leg;
	/// </summary>
	public int type;

	public GameObject c0;
	public GameObject c1;
	public GameObject c2;
	public GameObject c3;

	private ConfigurableJoint h0;
	private ConfigurableJoint h1;
	private ConfigurableJoint h2;

	public GameObject target;

	public float length1;
	public float length2;

	private Quaternion targetRot0;
	private Quaternion targetRot1;
	private Quaternion targetRot2;

	//private PIDControllerRot PID0;
	private PIDControllerRot PID1;
	private PIDControllerRot PID2;
	private PIDControllerRot PID3;
	/// <summary>
	/// IKJoints definition
	/// </summary>
	public Vector3 forward;

	public float maxForce;

	public IKForce(GameObject c0, GameObject c1, GameObject c2, GameObject c3, GameObject target, float maxForce, int type)
	{
		this.c0 = c0;
		this.c1 = c1;
		this.c2 = c2;
		this.c3 = c3;
		this.target = target;
		this.maxForce = maxForce;
		this.type = type;

		var parentTrans = c0.transform.parent;
		var rot = parentTrans.localRotation;
		length1 = c1.transform.localPosition.magnitude;
		length2 = c2.transform.localPosition.magnitude - length1;


		h0 = SetJoint(c0, c1);
		h1 = SetJoint(c1, c2);
		h2 = SetJoint(c2, c3);

		//Needs tuning, but how? i mean i can choose values but i have 3 per arm...
		//PID0 = new PIDControllerRot(c0 , 100, 15, 25);
		/*
		PID1 = new PIDControllerRot(c1, 200, 15, 45);
		PID2 = new PIDControllerRot(c2, 200, 15, 45);
		PID3 = new PIDControllerRot(c3, 100, 8, 28);*/

		float testIFactor = 1;

		if (type == 0)
		{
			//arm
			PID1 = new PIDControllerRot(c1, 210, 55 * testIFactor, 125);
			PID2 = new PIDControllerRot(c2, 220, 55 * testIFactor, 135);
			//PID3 = new PIDControllerRot(c3, 150, 20, 66);
			PID3 = new PIDControllerRot(c3, 100, 20 * testIFactor, 33);
			//PID3 = new PIDControllerRot(c3, 38, 20, 7);
			//PID3 = new PIDControllerRot(c3, 55, 20, 10);

			initializeLimb(new float[] { 1, 10, 10, 5 });
		}
		else
		{
			//leg
			PID1 = new PIDControllerRot(c1, 220, 100, 135);
			PID2 = new PIDControllerRot(c2, 220, 100, 135);
			PID3 = new PIDControllerRot(c3, 150, 20, 66);
			initializeLimb(new float[] { 1, 10, 10, 15 });
		}

		forward = c0.transform.parent.parent.localRotation * c1.transform.localPosition.normalized;
		//only results in 1,0,0
		//forward = c1.transform.localPosition.normalized;

		//we basicly have 1 weak point for both h0 and h1.
		//have to rotate parent in a way that it doesnt effect ik much DOESNT WORK i think

		//sets the parts of the limb inertia tensor and center of mass
		//so the pid controller rotate it efficiently
		//it basicly makes everything stable, BUT need to test if change of center of mass messes up to much?
		//better solution would be to add torque at position -> would have to programm it
		//to change inertia i would have to apply inertia to the force -> probably possible but im cant get lokal pid to run

	}

	/// <summary>
	/// sets innertia tensor to (1,1,1) and center of mass to (0,0,0)
	/// </summary>
	private void initializeLimb(float[] mass)
	{
		inizializeRigidbody(c0.GetComponent<Rigidbody>(), mass[0], Vector3.zero);
		inizializeRigidbody(c1.GetComponent<Rigidbody>(), mass[1], c0.transform.localPosition - c1.transform.localPosition);
		inizializeRigidbody(c2.GetComponent<Rigidbody>(), mass[2], c1.transform.localPosition - c2.transform.localPosition);
		inizializeRigidbody(c3.GetComponent<Rigidbody>(), mass[3], Vector3.zero);

	}

	/// <summary>
	/// sets innertia tensor to (1,1,1) and center of mass to (0,0,0)
	/// </summary>
	/// <param name="rBody"></param>
	private void inizializeRigidbody(Rigidbody rBody, float mass, Vector3 centerOfMass)
	{
		rBody.centerOfMass = centerOfMass;
		rBody.mass = mass;
		rBody.inertiaTensor = new Vector3(mass, mass, mass);
		rBody.maxAngularVelocity = 25;
	}

	ConfigurableJoint SetJoint(GameObject go, GameObject connectedBody)
	{
		var rot = go.transform.localRotation;
		var joint = go.AddComponent<ConfigurableJoint>();


		joint.connectedBody = connectedBody.GetComponent<Rigidbody>();
		joint.anchor = Vector3.zero;
		joint.axis = Vector3.zero;
		joint.secondaryAxis = Vector3.zero;
		joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;

		return joint;
	}

	//Can throw some error when target is in h0
	public void UpdateTargetRotsGlobal()
	{
		if (type == 1)
		{
			//Covers Leg rotation
			//LOCAL positions
			Vector3 pos1;
			Vector3 pos2;
			Vector3 pos3;

			Quaternion rot1;
			Quaternion rot2;
			Quaternion rot3;

			Vector3 tPosition2 = target.transform.localPosition - c0.transform.parent.parent.localPosition;

			float totalDistance = tPosition2.magnitude; // (target.transform.position - c0.transform.position).magnitude;

			//NAN testFix
			if (totalDistance <= 0.3f)
			{
				Debug.Log("saved");
				return;
			}

			float elbowAngle = 0;
			if (totalDistance < length1 + length2)
			{
				elbowAngle = Mathf.Acos((length2 * length2 - totalDistance * totalDistance - length1 * length1) / (-2 * length1 * totalDistance)) * Mathf.Rad2Deg;
				//http://www.arndt-bruenner.de/mathe/scripts/Dreiecksberechnung.htm#SSS
			}

			//offset rotates perpendicular to target
			Quaternion offsetQuat = new Quaternion();//Quaternion.AngleAxis(elbowAngle, Vector3.Cross(-elbowTarget.transform.position, (h0.transform.InverseTransformPoint(target.transform.position))));

			//Vector3 tmp = h0.transform.InverseTransformPoint(target.transform.position);

			//version 2: More natural, no fuckup when at shoulder
			Quaternion tmp2 = Quaternion.Euler(0, 0, -90 * forward.x);

			//version 4, bit better but got some bad spots above/behind shoulder
			tmp2 = Quaternion.Euler(45, 0, 45);

			offsetQuat = Quaternion.AngleAxis(elbowAngle, Vector3.Cross(tmp2 * tPosition2, tPosition2));
			offsetQuat = Quaternion.AngleAxis(elbowAngle, new Vector3(1, 0, 0));

			pos1 = offsetQuat * Quaternion.FromToRotation(Vector3.forward, tPosition2) * Vector3.forward * length1;
			pos2 = Quaternion.FromToRotation(Vector3.forward, tPosition2 - pos1) * Vector3.forward * length2 + pos1;
			if (elbowAngle == 0f)
			{
				rot1 = Quaternion.FromToRotation(Vector3.down, pos1);
				rot2 = rot1;
			}
			else
			{
				rot1 = Quaternion.LookRotation(pos1, pos2) * Quaternion.FromToRotation(Vector3.down, Vector3.forward);
				rot2 = Quaternion.LookRotation(pos2 - pos1, -pos1) * Quaternion.FromToRotation(Vector3.down, Vector3.forward);
			}
			//i dont rotate the wheel atm
			//rot3 = target.transform.localRotation * Quaternion.Euler(180, 180, -90 * forward.x);

			//inverse x rotation and swap y and z
			//instead of swap y and z could possibly fix the code above but this is faster. REWORK LATER??
			rot1.x *= -1;
			rot2.x *= -1;

			var tmp = rot1.y;
			rot1.y = rot1.z;
			rot1.z = tmp;

			tmp = rot2.y;
			rot2.y = rot2.z;
			rot2.z = tmp;

			PID1.UpdateTarget(c0.transform.parent.parent.localRotation * c0.transform.localRotation * rot1);
			PID2.UpdateTarget(c0.transform.parent.parent.localRotation * c0.transform.localRotation * rot2);
		}
		if (type == 0)
		{
			//coveres arm orientation
			//LOCAL positions
			Vector3 pos1;
			Vector3 pos2;
			Vector3 pos3;

			Quaternion rot1;
			Quaternion rot2;
			Quaternion rot3;

			Vector3 tPosition2 = c0.transform.InverseTransformPoint(target.transform.position);
			tPosition2.x *= -forward.x;



			float totalDistance = (target.transform.position - c0.transform.position).magnitude;

			//NAN testFix
			if (totalDistance <= 0.8f)
			{
				Debug.Log("Arm NAN Saved");
				return;
			}

			float elbowAngle = 0;
			if (totalDistance < length1 + length2)
			{
				elbowAngle = Mathf.Acos((length2 * length2 - totalDistance * totalDistance - length1 * length1) / (-2 * length1 * totalDistance)) * Mathf.Rad2Deg;
			}
			//limit the min elbow so the arm can never be totaly straight (it looks a bit strange when he does it
			//probaly also make a smooth transition, so not hard cap at 4?????? (probably
			elbowAngle = Mathf.Max(4, elbowAngle);

			//offset rotates perpendicular to target
			Quaternion offsetQuat = new Quaternion();//Quaternion.AngleAxis(elbowAngle, Vector3.Cross(-elbowTarget.transform.position, (h0.transform.InverseTransformPoint(target.transform.position))));

			//Vector3 tmp = h0.transform.InverseTransformPoint(target.transform.position);

			//version 2: More natural, no fuckup when at shoulder
			Quaternion tmp2 = Quaternion.Euler(0, 0, -90 * forward.x);

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


			if (forward.x > 0)
			{
				//Fix Left arm rotations
				//invers y and z axis of the rotations
				rot1.y *= -1;
				rot1.z *= -1;

				rot2.y *= -1;
				rot2.z *= -1;

				PID1.UpdateTarget(c0.transform.parent.parent.localRotation * c0.transform.localRotation * rot1);
				PID2.UpdateTarget(c0.transform.parent.parent.localRotation * c0.transform.localRotation * rot2);
			}
			else
			{
				//right hand version
				PID1.UpdateTarget(c0.transform.parent.parent.localRotation * c0.transform.localRotation * rot1);
				PID2.UpdateTarget(c0.transform.parent.parent.localRotation * c0.transform.localRotation * rot2);
			}

			if (c3.GetComponent<FixedJoint>() == null)
			{
				PID3.UpdateTarget(rot3);
			}
			else if (c3.GetComponent<FixedJoint>().connectedBody == null)
			{
				//connected to object without rigidbody
				PID3.UpdateTarget(rot3);
			}
			else
			{
				//connected to rigidbody (weapon or non weapon)
				PID3.UpdateTarget(rot3, c3.GetComponent<FixedJoint>().connectedBody);
			}
		}
	}

	//some debug stuff because unity loves to only print 1 float digit
	public void printVector(Vector3 vec)
	{
		Debug.Log("(" + vec.x + " " + vec.y + " " + vec.z + ")");
	}

	public void printQuat(Quaternion quat)
	{
		Debug.Log("(" + quat.x + " " + quat.y + " " + quat.z + " " + quat.w + ")");
	}

	/// <summary>
	/// dont want to do this... 
	/// </summary>
	public void appExit()
	{
		PID1.appExit();
		PID2.appExit();
		if (type == 0)
		{
			PID3.appExit();
		}
	}
}
