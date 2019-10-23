using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[Header("Mech parts")]
	public GameObject cockpitTarget;
	public GameObject cockpit;
	public GameObject head;

	public GameObject armLeft;
	public GameObject armRight;

	public GameObject legLeft;
	public GameObject legRight;

	[Header("IK targets")]
	public GameObject targetArmLeft;
	public GameObject targetArmRight;
	public GameObject targetLegLeft;
	public GameObject targetLegRight;

	[Header("IK ellbow direction")]
	public GameObject elbowTargetArmL;
	public GameObject elbowTargetArmR;
	public GameObject elbowTargetLegL;
	public GameObject elbowTargetLegR;

	private FakeIK leftArmIK;
	private FakeIK rightArmIK;
	private FakeIK leftLegIK;
	private FakeIK rightLegIK;

	[Header("")]
	public float maxForce = 100000;
	public float defaultSpring = 200;
	public float defaultDampening = 50;

	public float footHeight = 1.05f;

	Quaternion cockpitOriginRot;
	Quaternion mechOriginRot;

	//for camera rotation
	Vector3 cockpitRot;

	//for cockpitposition;
	Vector3 cockpitPosition;

	GameObject[] armHingeRight;
	GameObject[] armHingeLeft;
	GameObject[] legHingeRight;
	GameObject[] legHingeLeft;

	Vector3 legOffsetLeft;
	Vector3 legOffsetRight;
	Vector3 armOffsetLeft;
	Vector3 armOffsetRight;

	//todo: elbow starting offset.
	Vector3 elbowPosArmR;
	Vector3 elbowPosArmL;
	Vector3 elbowPosLegR;
	Vector3 elbowPosLegL;
	// Use this for initialization




	void Start()
	{

		cockpitOriginRot = cockpit.transform.localRotation;
		mechOriginRot = cockpit.transform.parent.rotation;

		cockpitRot = Vector3.zero;
		armHingeRight = new GameObject[4];
		armHingeLeft = new GameObject[4];
		legHingeRight = new GameObject[4];
		legHingeLeft = new GameObject[4];

		//version with both arms
		/*
		Transform tmp;
		tmp = armLeft.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeLeft[i] = tmp.GetChild(i).gameObject;
			var rbody = armHingeLeft[i].AddComponent<Rigidbody>();
			rbody.useGravity = false;
		}

		tmp = armRight.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeRight[i] = tmp.GetChild(i).gameObject;
			var rbody = armHingeRight[i].AddComponent<Rigidbody>();
			rbody.useGravity = false;
		}

		tmp = legRight.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			legHingeRight[i] = tmp.GetChild(i).gameObject;
			var rbody = legHingeRight[i].AddComponent<Rigidbody>();
			rbody.useGravity = false;
		}
		tmp = legLeft.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			legHingeLeft[i] = tmp.GetChild(i).gameObject;
			var rbody = legHingeLeft[i].AddComponent<Rigidbody>();
			rbody.useGravity = false;
		}*/

		//version with 1 arm/leg
		Transform tmp;
		tmp = armLeft.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeLeft[i] = tmp.GetChild(i).gameObject;
			var rbody = armHingeLeft[i].AddComponent<Rigidbody>();
			rbody.useGravity = false;
		}

		armRight = Instantiate(armLeft);
		armRight.transform.SetParent(armLeft.transform.parent);

		/*
		Debug.Log(mechOriginRot.eulerAngles);

		armRight.transform.Rotate(mechOriginRot.eulerAngles, Space.World);

		//armRight.transform.localRotation = armRight.transform.localRotation *  mechOriginRot;


		armRight.transform.localRotation *= Quaternion.Euler(0, 180, 0);

		var why = armLeft.transform.localPosition;
		why.Scale(new Vector3(-1, 1, 1));
		armRight.transform.localPosition = why;
		*/
		armRight.transform.localPosition = new Vector3(-armLeft.transform.localPosition.x, armLeft.transform.localPosition.y, armLeft.transform.localPosition.z);

		tmp = armRight.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeRight[i] = tmp.GetChild(i).gameObject;
		}

		tmp = legLeft.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			legHingeLeft[i] = tmp.GetChild(i).gameObject;
			var rbody = legHingeLeft[i].AddComponent<Rigidbody>();
			rbody.useGravity = false;
		}

		legRight = Instantiate(legLeft);
		legRight.transform.SetParent(legLeft.transform.parent);
		//take rotation fix from above if needed
		legRight.transform.localPosition = new Vector3(-legLeft.transform.localPosition.x, legLeft.transform.localPosition.y, legLeft.transform.localPosition.z);
		tmp = legRight.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			legHingeRight[i] = tmp.GetChild(i).gameObject;
		}


		//give cockpit and cockpittarget rigidbody 
		var rigidbody = cockpit.AddComponent<Rigidbody>();
		rigidbody.useGravity = true;
		rigidbody.mass = 10;

		rigidbody = cockpitTarget.AddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;


		//Initialize IK
		//Todo fix elbows, so mech location can be changed without it effecting the elbow -> save startlocation of elbow and change every frame
		//rightArmIK = new IK(armHingeRight[0], armHingeRight[1], armHingeRight[2], armHingeRight[3], targetArmRight, elbowTargetArmR, maxForce, defaultSpring, defaultDampening);
		//leftArmIK = new IK(armHingeLeft[0], armHingeLeft[1], armHingeLeft[2], armHingeLeft[3], targetArmLeft, elbowTargetArmL, maxForce, defaultSpring, defaultDampening);
		//rightLegIK = new IK(legHingeRight[0], legHingeRight[1], legHingeRight[2], legHingeRight[3], targetLegRight, elbowTargetLegR, maxForce, defaultSpring, defaultDampening);
		//leftLegIK = new IK(legHingeLeft[0], legHingeLeft[1], legHingeLeft[2], legHingeLeft[3], targetLegLeft, elbowTargetLegL, maxForce, defaultSpring, defaultDampening);
		rightArmIK = new FakeIK(armHingeRight[0], armHingeRight[1], armHingeRight[2], armHingeRight[3], targetArmRight);
		leftArmIK = new FakeIK(armHingeLeft[0], armHingeLeft[1], armHingeLeft[2], armHingeLeft[3], targetArmLeft);
		rightLegIK = new FakeIK(legHingeRight[0], legHingeRight[1], legHingeRight[2], legHingeRight[3], targetLegRight);
		leftLegIK = new FakeIK(legHingeLeft[0], legHingeLeft[1], legHingeLeft[2], legHingeLeft[3], targetLegLeft);



		//setFixedJoint(cockpit, legHingeLeft[0]);
		//setFixedJoint(cockpit, legHingeRight[0]);
		//setFixedJoint(cockpit, armHingeLeft[0]);
		//setFixedJoint(cockpit, armHingeRight[0]);

		//create Joints
		//fine tune with values  + right mass on rbodys needed   (also a way to account for the leg mass)
		rigidbody = cockpit.GetComponent<Rigidbody>();
		var joint = cockpit.AddComponent<ConfigurableJoint>();
		joint.connectedBody = cockpitTarget.GetComponent<Rigidbody>();

		var drive = new JointDrive();
		drive.positionSpring = defaultSpring * 10;
		drive.positionDamper = defaultDampening * 10;
		drive.maximumForce = maxForce;

		joint.angularXDrive = drive;
		joint.angularYZDrive = drive;

		drive = new JointDrive();
		drive.positionSpring = defaultSpring;
		drive.positionDamper = defaultDampening;
		drive.maximumForce = maxForce;

		joint.xDrive = drive;
		joint.zDrive = drive;

		drive = new JointDrive();
		drive.positionSpring = defaultSpring * 3;
		drive.positionDamper = defaultDampening * 3;
		drive.maximumForce = maxForce;
		joint.yDrive = drive;

		joint.swapBodies = true;

		legOffsetLeft = legLeft.transform.localPosition;
		legOffsetRight = legRight.transform.localPosition;
		armOffsetRight = armRight.transform.localPosition;
		armOffsetLeft = armLeft.transform.localPosition;

		//temporary
		head.AddComponent<Rigidbody>();
		var fJoint = head.AddComponent<FixedJoint>();
		fJoint.connectedBody = cockpit.GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
	{
		var rbodyCockpit = cockpit.GetComponent<Rigidbody>();
		var rbodyCockpitTarget = cockpitTarget.GetComponent<Rigidbody>();


		//test distance to legpositon, if wrong, set legposition in direction we are moving.
		float dist = (targetLegRight.transform.position - legHingeRight[0].transform.position).magnitude;
		if (dist >= rightLegIK.length1 + rightLegIK.length2)
		{
			if (!replaceLeg(45, rbodyCockpit.velocity, legHingeRight[0], targetLegRight))
			{
				replaceLeg(30, rbodyCockpit.velocity, legHingeRight[0], targetLegRight);
			}
		}

		dist = (targetLegLeft.transform.position - legHingeLeft[0].transform.position).magnitude;
		if (dist >= leftLegIK.length1 + leftLegIK.length2)
		{
			if (!replaceLeg(45, rbodyCockpit.velocity, legHingeLeft[0], targetLegLeft))
			{
				replaceLeg(30, rbodyCockpit.velocity, legHingeLeft[0], targetLegLeft);
			}
		}

		dist = (targetArmRight.transform.position - armHingeRight[0].transform.position).magnitude;
		if (dist >= rightArmIK.length1 + rightArmIK.length2)
		{
			if (!replaceLeg(45, rbodyCockpit.velocity, armHingeRight[0], targetArmRight))
			{
				replaceLeg(30, rbodyCockpit.velocity, armHingeRight[0], targetArmRight);
			}
		}

		dist = (targetArmLeft.transform.position - armHingeLeft[0].transform.position).magnitude;
		if (dist >= leftArmIK.length1 + leftArmIK.length2)
		{
			if (!replaceLeg(45, rbodyCockpit.velocity, armHingeLeft[0], targetArmLeft))
			{
				replaceLeg(30, rbodyCockpit.velocity, armHingeLeft[0], targetArmLeft);
			}
		}

		//updates legposition
		armLeft.transform.rotation = cockpit.transform.rotation;
		armLeft.transform.position = cockpit.transform.position + cockpit.transform.rotation * Quaternion.Inverse(cockpitOriginRot) * armOffsetLeft;
		armRight.transform.rotation = cockpit.transform.rotation;
		armRight.transform.position = cockpit.transform.position + cockpit.transform.rotation * Quaternion.Inverse(cockpitOriginRot) * armOffsetRight;

		legLeft.transform.rotation = cockpit.transform.rotation;
		legLeft.transform.position = cockpit.transform.position + cockpit.transform.rotation * Quaternion.Inverse(cockpitOriginRot) * legOffsetLeft;
		legRight.transform.rotation = cockpit.transform.rotation;
		legRight.transform.position = cockpit.transform.position + cockpit.transform.rotation * Quaternion.Inverse(cockpitOriginRot) * legOffsetRight;

		//updates IK
		rightArmIK.UpdateAll();
		leftArmIK.UpdateAll();
		rightLegIK.UpdateAll();
		leftLegIK.UpdateAll();
	}

	//copied from mechscript
	bool replaceLeg(float angle, Vector3 velocity, GameObject leg, GameObject legTarget)
	{
		var rayInfo = new RaycastHit();
		var rot1 = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.down, velocity));
		if (Physics.Raycast(leg.transform.position + footHeight * Vector3.down, rot1 * Vector3.down, out rayInfo, rightLegIK.length1 + rightLegIK.length2, 1 << 0))
		{
			legTarget.transform.position = rayInfo.point + Vector3.up * footHeight;
			legTarget.transform.rotation = Quaternion.FromToRotation(Vector3.up, rayInfo.normal);
			return true;
		}
		else
		{
			return false;
		}
	}

	//copied from mechscript
	void setFixedJoint(GameObject one, GameObject two)
	{
		var joint = one.AddComponent<FixedJoint>();
		joint.connectedBody = two.GetComponent<Rigidbody>();
	}
}
