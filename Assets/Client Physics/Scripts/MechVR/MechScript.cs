using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechScript : MonoBehaviour
{
	[Header("Mech parts")]
	public GameObject cockpitTarget;
	public GameObject cockpit;

	public GameObject armLeft;
	public GameObject armRight;

	public GameObject legLeft;
	public GameObject legRight;

	//todo: dynamic weapon pickup 
	//public GameObject weapon;

	public GameObject UserInterface;

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

	[Header("VR stuff (TODO: probably can be found by script)")]
	public GameObject controllerLeft;
	public GameObject controllerRight;
	public GameObject viveControllerLeft;
	public GameObject viveControllerRight;
	public GameObject head;

	private IKForce leftArmIK;
	private IKForce rightArmIK;
	private FakeIK leftLegIK;
	private FakeIK rightLegIK;

	[Header("")]
	public float maxForce = 100000;
	public float defaultSpring = 200;
	public float defaultDampening = 50;

	public float footHeight = 1.05f;
	public float legHeight = 5f;
	public float humanRobotFactor = 5f;


	/// <summary>
	/// true means vr tils with cockpit
	/// </summary>
	public bool tiltType;

	Quaternion cockpitOriginRot;
	Quaternion mechOriginRot;

	//for camera mouse rotation not used anymore, can be deleted sooner or later
	Vector3 cockpitRot;

	//for cockpitposition;
	Vector3 cockpitPosition;

	GameObject[] armHingeRight;
	GameObject[] armHingeLeft;
	GameObject[] legHingeRight;
	GameObject[] legHingeLeft;

	GameObject weaponRight;
	GameObject weaponLeft;

	Vector3 legOffsetLeft;
	Vector3 legOffsetRight;

	//todo: elbow starting offset.
	//Vector3 elbowPosArmR;
	//Vector3 elbowPosArmL;
	//Vector3 elbowPosLegR;
	//Vector3 elbowPosLegL;

	Vector3 oldHeadPosition;

	//Pid
	PIDControllerPos pidPos;
	PIDControllerRot pidRot;
	PIDControllerVel pidVel;




	void Start()
	{
		SteamVR_Camera.sceneResolutionScale = 2f;

		cockpitOriginRot = cockpit.transform.localRotation;
		mechOriginRot = cockpit.transform.parent.rotation;

		cockpitRot = Vector3.zero;
		armHingeRight = new GameObject[4];
		armHingeLeft = new GameObject[4];
		legHingeRight = new GameObject[4];
		legHingeLeft = new GameObject[4];

		//version with both arms

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

		/*
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
		/*
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

		print(mechOriginRot.eulerAngles);

		armRight.transform.Rotate(mechOriginRot.eulerAngles, Space.World);

		//armRight.transform.localRotation = armRight.transform.localRotation *  mechOriginRot;


		armRight.transform.localRotation *= Quaternion.Euler(0, 180, 0);

		var why = armLeft.transform.localPosition;
		why.Scale(new Vector3(-1, 1, 1));
		armRight.transform.localPosition = why;

		tmp = armRight.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeRight[i] = tmp.GetChild(i).gameObject;
		}

		*/
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
			//var rbody = legHingeRight[i].AddComponent<Rigidbody>();
			//rbody.useGravity = false;
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

		//rightArmIK = new IKJoint(armHingeRight[0], armHingeRight[1], armHingeRight[2], armHingeRight[3], targetArmRight, elbowTargetArmR, maxForce, defaultSpring, defaultDampening);
		//leftArmIK = new IKJoint(armHingeLeft[0], armHingeLeft[1], armHingeLeft[2], armHingeLeft[3], targetArmLeft, elbowTargetArmL, maxForce, defaultSpring, defaultDampening);

		rightArmIK = new IKForce(armHingeRight[0], armHingeRight[1], armHingeRight[2], armHingeRight[3], targetArmRight, maxForce, 1);
		leftArmIK = new IKForce(armHingeLeft[0], armHingeLeft[1], armHingeLeft[2], armHingeLeft[3], targetArmLeft, maxForce, 1);

		//rightLegIK = new IK(legHingeRight[0], legHingeRight[1], legHingeRight[2], legHingeRight[3], targetLegRight, elbowTargetLegR, maxForce, defaultSpring, defaultDampening);
		//leftLegIK = new IK(legHingeLeft[0], legHingeLeft[1], legHingeLeft[2], legHingeLeft[3], targetLegLeft, elbowTargetLegL, maxForce, defaultSpring, defaultDampening);
		rightLegIK = new FakeIK(legHingeRight[0], legHingeRight[1], legHingeRight[2], legHingeRight[3], targetLegRight);
		leftLegIK = new FakeIK(legHingeLeft[0], legHingeLeft[1], legHingeLeft[2], legHingeLeft[3], targetLegLeft);



		//setFixedJoint(cockpit, legHingeLeft[0]);
		//setFixedJoint(cockpit, legHingeRight[0]);
		setFixedJoint(cockpit, armHingeLeft[0]);
		setFixedJoint(cockpit, armHingeRight[0]);


		//create Joints
		//fine tune with values  + right mass on rbodys needed   (also a way to account for the leg mass)
		/*
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

		joint.swapBodies = true;*/

		//PID VERSION:
		/*
		pidPos = new PIDControllerPos(cockpit, 38, 0.3f, 10);
		pidRot = new PIDControllerRot(cockpit, 70, 0.3f, 20);*/

		pidPos = new PIDControllerPos(cockpit , cockpit, 38, 5, 8, new Vector3(0, 1, 0));
		pidRot = new PIDControllerRot(cockpit, 50, 5, 10);
		pidVel = new PIDControllerVel(cockpit, 35, 0, 0.6f, new Vector3(1, 0, 1), 100);


		legOffsetLeft = legLeft.transform.localPosition;
		legOffsetRight = legRight.transform.localPosition;

		if (!SteamVR.active)
		{
			head.transform.localPosition = new Vector3(0, 1.8f, 0);

			//
			//controllerLeft.transform.localPosition = new Vector3(-0.5f, 1.4f, 0.25f);
			//controllerRight.transform.localPosition = new Vector3(0.5f, 1.4f, 0.25f);
		}

		//todo:
		//temporary
		/*
		weapon.transform.position = armHingeRight[3].transform.position;
		var fjoint = weapon.AddComponent<FixedJoint>();
		fjoint.connectedBody = armHingeRight[3].GetComponent<Rigidbody>();
		*/

		//TODO: get rid of this: and disable robot rotation in the begining
		if (SteamVR.active)
		{
			Time.timeScale = 0;
		}
		else
		{
			//Time.timeScale = 0;
		}
		oldHeadPosition = head.transform.position;
	}

	void Update()
	{
		//At the start of the game.
		//TODO: think i get rid of timescale and instead dont coltroll mech rotation if one controler is in ui mode
		if (Time.timeScale == 0)
		{
			if (Input.GetKeyUp("joystick button 0") || Input.GetKeyUp("joystick button 2") || Input.GetKeyUp(KeyCode.P))
			{
				Time.timeScale = 1;

				//set the controller device ids
				viveControllerLeft.GetComponent<ControllerUI>().deviceId = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
				viveControllerRight.GetComponent<ControllerUI>().deviceId = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);

			}
			oldHeadPosition = head.transform.position;
			return;
		}


		//checks and controlls UI mode
		if (!GlobalVariables.UiActive && (viveControllerLeft.GetComponent<ControllerUI>().IsInUiMode() || viveControllerRight.GetComponent<ControllerUI>().IsInUiMode()))
		{
			GlobalVariables.UiActive = true;
			UserInterface.GetComponent<Animator>().SetBool("Up", true);

		}
		else if (GlobalVariables.UiActive && (!viveControllerLeft.GetComponent<ControllerUI>().IsInUiMode() && !viveControllerRight.GetComponent<ControllerUI>().IsInUiMode()))
		{
			GlobalVariables.UiActive = false;
			UserInterface.GetComponent<Animator>().SetBool("Up", false);
		}

		//TODO: find better solution so the hands dont move when in cockpit
		//think i leave this part out, it is NOT helping with motion sickness if the controlers hovers with you wheny ou move ALSO sometimes unreachable
		/*
		if (controllerLeft.GetComponent<Collider>().enabled)
		{
			controllerLeft.transform.position += head.transform.position - oldHeadPosition;
		}
		if (controllerRight.GetComponent<Collider>().enabled)
		{
			controllerRight.transform.position += head.transform.position - oldHeadPosition;
		}*/


		//Attach or Detach Weapon / shield
		//if (SteamVR_Controller.Input(viveControllerLeft.GetComponent<ControllerUI>().deviceId).GetHairTriggerDown())
		if ((controllerLeft.GetComponent<HandController>().connectedViveController != null && SteamVR_Controller.Input(controllerLeft.GetComponent<HandController>().controllerID).GetHairTriggerDown()) || Input.GetKeyDown(KeyCode.Q))
		{
			weaponLeft = AttachDetachWeapon(armHingeLeft[3], weaponLeft, leftArmIK.forward);
		}
		//now same for right
		if ((controllerRight.GetComponent<HandController>().connectedViveController != null && SteamVR_Controller.Input(controllerRight.GetComponent<HandController>().controllerID).GetHairTriggerDown()) || Input.GetKeyDown(KeyCode.E))
		{
			weaponRight = AttachDetachWeapon(armHingeRight[3], weaponRight, rightArmIK.forward);
		}


		//mech controls here. could multiply with Invert of original cockpit rot to make it more reasonable(and always work on all types of mechs?)

		//ray down (worldspace) to see if ground is there, if yes, set cockpitTarget distance x above gorund.
		//if not deactivate force in this direction so it falls down (gravity) (but how?) one idea : setcockpittarget on cockpit in this axis
		//better idea: make the joint forces from the cockpitTarget to the cockpit, so the force directions dont get fucked up  ALSO UNFREEZE Y IN cockpittarget

		//think i also have to add forces for movement(and reduce joint forces, to get better impacts) (or increase weight pf stuff we hit, have to test ou)t

		//todo: adjust center of mass by  giving some kind of legs some mass

		var rbodyCockpit = cockpit.GetComponent<Rigidbody>();
		var rbodyCockpitTarget = cockpitTarget.GetComponent<Rigidbody>();

		//MouseRotation:
		//cockpitRot += new Vector3(0, Input.GetAxis("Mouse X") * Time.deltaTime * 100, 0);
		//cockpitTarget.transform.rotation = Quaternion.Euler(cockpitRot) * cockpitOriginRot;

		//for mech rotation:
		//different ways possible:
		//mech rotatas like head -> shoud only rotates very little if in about 30, and fully after more that 30
		//mech rotates to hands
		//mech rotates to average of head / arms
		//mech rotates with controler input
		if ((!viveControllerLeft.GetComponent<ControllerUI>().IsInUiMode() && !viveControllerRight.GetComponent<ControllerUI>().IsInUiMode()) || !SteamVR.active)
		{
			//IDEA: add button to disable automatic rotation

			//cockpitRot = head.transform.localRotation.eulerAngles;
			//cockpitRot.Scale(new Vector3(0, 1, 0)); //better use Quaternion.axisAngle or how its called
			//cockpitTarget.transform.rotation = Quaternion.Euler(cockpitRot) * cockpitOriginRot;

			//Version 1 set roboter to head rotation:
			var rotation = Quaternion.AngleAxis(head.transform.localEulerAngles.y, Vector3.up) * cockpitOriginRot;

			//use angle difference as factor
			float diff = Mathf.Abs(rotation.eulerAngles.y - cockpitTarget.transform.localEulerAngles.y);
			cockpitTarget.transform.rotation = rotation;

			//works but is not nice (rotates in steps)
			//if (Mathf.Abs(rotation.eulerAngles.y - cockpitTarget.transform.localEulerAngles.y) >= 90)
			//{
			//	cockpitTarget.transform.rotation = rotation;
			//}

			//Version 2 set roboter to average hand rotation + 2 times head + 1 times cockpit TODO: for all, set y 0 and then normalize
			var handR = controllerLeft.transform.localPosition - head.transform.localPosition;
			var handL = controllerRight.transform.localPosition - head.transform.localPosition;
			handR.y = 0;
			handL.y = 0;
			var combinedHand = handR.normalized + handL.normalized + head.transform.forward * 2 + cockpit.transform.forward;
			combinedHand.y = 0;
			rotation = Quaternion.FromToRotation(Vector3.forward, combinedHand);
			cockpitTarget.transform.rotation = rotation * cockpitOriginRot;
		}

		//TODO: tilt robot in direction hes moving (only a bit)
		//TODO: tilt robot in direction hes facing when sneaking

		//set cameraRig Position to roboter
		if (SteamVR.active)
		{
			//in vr
			if (GlobalVariables.ControllLayer3)
			{
				//idea is to move around in cockpit

				head.transform.parent.position = cockpit.transform.position;
			}
			else
			{
				//idea is to lock user to middle of cockpit.
				//probably allow up/down movement
				head.transform.parent.position = cockpit.transform.position - Vector3.Scale(head.transform.localPosition, new Vector3(1, 0, 1));
			}

			//TODO: ingame option to torn off/on, atm its only for absolute madmans (seems better with PID now)
			//this tilts the vr area with the cockpit 
			if (tiltType)
			{
				head.transform.parent.rotation = Quaternion.Inverse(cockpitOriginRot) * cockpit.transform.rotation;
				head.transform.parent.rotation = Quaternion.Euler(-head.transform.parent.rotation.eulerAngles.x, 0, head.transform.parent.rotation.eulerAngles.y);
			}
			else
			{
				//probalby have to set rotation to zero somewhere so it is not tiltet when switching to no tilt mode
			}

		}
		else
		{
			//non vr
			head.transform.parent.parent.position = cockpit.transform.position - Vector3.Scale(head.transform.localPosition, new Vector3(1, 0, 1));

			//this tilts the vr area with the cockpit
			head.transform.parent.parent.rotation = Quaternion.Inverse(cockpitOriginRot) * cockpit.transform.rotation;
			head.transform.parent.parent.rotation = Quaternion.Euler(-head.transform.parent.parent.rotation.eulerAngles.x, 0, head.transform.parent.parent.rotation.eulerAngles.y);

		}



		//cockpitTarget.transform.position += cockpitTarget.transform.rotation * Vector3.up * Input.GetAxis("Vertical") * Time.deltaTime * 5;
		//cockpitTarget.transform.position -= cockpitTarget.transform.rotation * Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * 5;

		//TODO: move cockpitPosition with mech when jumping
		cockpitPosition += cockpitTarget.transform.rotation * Vector3.up * Input.GetAxis("Vertical") * Time.deltaTime * 5;
		cockpitPosition -= cockpitTarget.transform.rotation * Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * 5;


		float tmpY = cockpitTarget.transform.position.y;
		cockpitTarget.transform.position = cockpitPosition + head.transform.localPosition * humanRobotFactor;

		cockpitTarget.transform.position = new Vector3(cockpitTarget.transform.position.x, tmpY, cockpitTarget.transform.position.z);

		Vector3 difference = (cockpitTarget.transform.position - cockpit.transform.position);
		difference.y = 0;

		//cap distance of cockpittarget to cockpit at max 1 (TODO: find better value instead of 1)
		if (difference.magnitude > 1)
		{
			difference = difference.normalized * 1;
			cockpitTarget.transform.position = cockpit.transform.position + difference;
			cockpitPosition = cockpit.transform.position + difference - head.transform.localPosition * humanRobotFactor;
			//cockpitPosition.y = cockpit.transform.position.y + difference.y;
			cockpitPosition.y = 0;
		}

		//set cockpittransform above ground if not jumping or to high
		//probably do more than just one ray down?
		var cockpitRayDownInfo = new RaycastHit();
		if (Physics.Raycast(cockpitTarget.transform.position, Vector3.down, out cockpitRayDownInfo, 9, 1 << 0))
		{
			if (Input.GetKey(KeyCode.Space))
			{
				rbodyCockpitTarget.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
				rbodyCockpit.AddForce(Vector3.up * 2000 * Time.deltaTime * rbodyCockpit.mass);
			}
			else
			{
				var tmp = cockpitTarget.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				//VR version 1
				cockpitTarget.transform.position = cockpitRayDownInfo.point + Vector3.up * head.transform.localPosition.y * 1.5f + Vector3.up * 2;
				//non VR version:
				//cockpitTarget.transform.position = rayInfo.point + Vector3.up * legHeight;
			}
		}
		else
		{
			rbodyCockpitTarget.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
		}


		//old rotation approach
		//cockpitTarget.transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * Time.deltaTime * 100, Vector3.forward);
		//cockpitTarget.transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * Time.deltaTime * 100, Vector3.right);

		//updates legposition
		legLeft.transform.rotation = cockpit.transform.rotation;
		legLeft.transform.position = cockpit.transform.position + cockpit.transform.rotation * Quaternion.Inverse(cockpitOriginRot) * legOffsetLeft;
		legRight.transform.rotation = cockpit.transform.rotation;
		legRight.transform.position = cockpit.transform.position + cockpit.transform.rotation * Quaternion.Inverse(cockpitOriginRot) * legOffsetRight;

		//make legs fit
		//robably add to distance test and angle test. (different values in different directions, so legs dont cross)
		//TODO: add fluent leg movement insteadof insta jump


		//test distance to legpositon, if wrong, set legposition in direction we are moving.
		float dist = (targetLegRight.transform.position - legHingeRight[0].transform.position).magnitude;
		if (dist >= rightLegIK.length1 + rightLegIK.length2)
		{
			if (!replaceLeg(30, rbodyCockpit.velocity, legHingeRight[0], targetLegRight))
			{
				replaceLeg(20, rbodyCockpit.velocity, legHingeRight[0], targetLegRight);
			}
		}

		dist = (targetLegLeft.transform.position - legHingeLeft[0].transform.position).magnitude;
		if (dist >= leftLegIK.length1 + leftLegIK.length2)
		{
			if (!replaceLeg(30, rbodyCockpit.velocity, legHingeLeft[0], targetLegLeft))
			{
				replaceLeg(20, rbodyCockpit.velocity, legHingeLeft[0], targetLegLeft);
			}
		}


		//set target arms according to controler position
		//version 1
		var shoulderPosition = head.transform.localPosition;
		//factor 0.78 because im ~1.8 and my shoulders are at ~1.4
		shoulderPosition.Scale(new Vector3(1f, 0.78f, 1f));
		//possible: need to offfset to right/left to get shoulder?

		//set hand position
		targetArmLeft.transform.position = (controllerLeft.transform.localPosition - shoulderPosition) * 8 + armHingeLeft[0].transform.position;
		targetArmLeft.transform.position += armHingeLeft[0].transform.right * armLeft.transform.localPosition.x;
		targetArmLeft.transform.position += cockpitTarget.transform.up * 1.5f;

		targetArmRight.transform.position = (controllerRight.transform.localPosition - shoulderPosition) * 8 + armHingeRight[0].transform.position;
		targetArmRight.transform.position += armHingeRight[0].transform.right * armRight.transform.localPosition.x;
		targetArmRight.transform.position += cockpitTarget.transform.up * 1.5f;

		//Version 2: uses real hand position (not effected by head position
		//targetArmLeft.transform.position = controllerLeft.transform.localPosition * humanRobotFactor + new Vector3(cockpitPosition.x, cockpitRayDownInfo.point.y, cockpitPosition.z);
		//targetArmRight.transform.position = controllerRight.transform.localPosition * humanRobotFactor + new Vector3(cockpitPosition.x, cockpitRayDownInfo.point.y, cockpitPosition.z);

		//set hand rotation, TODO: probaly add slight tilt so its more confortable
		targetArmLeft.transform.rotation = controllerLeft.transform.localRotation * Quaternion.Euler(-35, 0, 0);
		targetArmRight.transform.rotation = controllerRight.transform.localRotation * Quaternion.Euler(-35, 0, 0);

		//updates IK
		//rightArmIK.UpdateTargetRotsGlobal();
		//leftArmIK.UpdateTargetRotsGlobal();
		rightLegIK.UpdateAll();
		leftLegIK.UpdateAll();

		oldHeadPosition = head.transform.position;
	}

	private void FixedUpdate()
	{
		//Mech movement here
		//idea: trackpat direts the Velocity the cockpit should have.
		//->	use pid to control the xz velocity
		//		and pid to control the x position to over the ground (disable when jumping)

		//TODO: move cockpitPosition with mech when jumping

		float speedfaktor = 10;
		Vector3 targetVel = new Vector3(Input.GetAxis("Horizontal") * speedfaktor, 0, Input.GetAxis("Vertical") * speedfaktor);

		pidVel.UpdateTarget(targetVel, 1);



		//update fixed update stuff in IK
		rightArmIK.UpdateTargetRotsGlobal();
		leftArmIK.UpdateTargetRotsGlobal();
		pidRot.UpdateTarget(cockpitTarget.transform.rotation);
		pidPos.UpdateTarget(cockpitTarget.transform.position);
	}

	/// <summary>
	/// detaches weapon or checks if new weapon is in front of hand an attaches it
	/// </summary>
	/// <param name="hand"></param>
	/// <param name="weapon"></param>
	/// <param name="handForward"></param>
	/// <returns></returns>
	private GameObject AttachDetachWeapon(GameObject hand, GameObject weapon, Vector3 handForward)
	{
		if (weapon == null)
		{
			//find what arm is colliding with:
			//todo: ray or spherecast from hand, and look for collision with a new collider layer named pickable
			var weaponRay = new RaycastHit();
			LayerMask mask = LayerMask.GetMask(new string[] { "Weapon" });
			//Debug.DrawRay(hand.transform.position, hand.transform.rotation * -handForward * 2, Color.green);
			if (Physics.Raycast(hand.transform.position, hand.transform.rotation * -handForward * 2, out weaponRay, 9, mask))
			{
				//first collsion, rotate hand to rotation of object + also position, then create fixed joint between both, and let Joint + IK handle the rest
				weaponRay.rigidbody.isKinematic = false;
				GameObject pickup = weaponRay.rigidbody.gameObject;
				weapon = pickup;
				hand.transform.rotation = pickup.transform.rotation * (handForward.x < 0 ? Quaternion.Euler(-90, 0, 0) : Quaternion.Euler(-90, 180, 0));
				//hand.transform.rotation = pickup.transform.rotation * Quaternion.FromToRotation(Quaternion.FromToRotation(Vector3.right, Vector3.forward) * handForward, Vector3.down);
				hand.transform.position = pickup.transform.position;
				var joint = hand.AddComponent<FixedJoint>();
				joint.connectedBody = pickup.GetComponent<Rigidbody>();
			}
		}
		else
		{
			//detach weapon:
			//remove the fixedjoint:
			Destroy(hand.GetComponent<FixedJoint>());
			weapon.GetComponent<Rigidbody>().isKinematic = true;
			weapon = null;
		}
		return weapon;
	}

	/// <summary>
	/// tries to sets a new leg position
	/// </summary>
	/// <returns>true if it legPos changed</returns>
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

	void setFixedJoint(GameObject one, GameObject two)
	{
		var joint = one.AddComponent<FixedJoint>();
		joint.connectedBody = two.GetComponent<Rigidbody>();
	}

	public void changeTiltStyle()
	{
		tiltType = !tiltType;
		print("works?" + tiltType);
	}
}