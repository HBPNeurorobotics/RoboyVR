using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// legless variant of Mech
/// </summary>
public class MechScript2 : MonoBehaviour
{
	[Header("Mech parts")]
	public GameObject cockpitTarget;
	public GameObject cockpit;

	public GameObject armLeft;
	public GameObject armRight;

	public GameObject leg;

	public GameObject leftArmFrontPart;
	public GameObject rightArmFrontPart;

	//todo: dynamic weapon pickup 
	//public GameObject weapon;

	public GameObject UserInterface;

	[Header("IK targets")]
	public GameObject targetArmLeft;
	public GameObject targetArmRight;
	public GameObject targetLeg;

	[Header("VR stuff (TODO: probably can be found by script)")]
	public GameObject controllerLeft;
	public GameObject controllerRight;
	public GameObject viveControllerLeft;
	public GameObject viveControllerRight;
	public GameObject head;

	IKForce leftArmIK;
	IKForce rightArmIK;
	IKForce legIK;

	/*
	[Header("PID Tuner stuff")]
	public bool currentTuning = false;
	public float tuneP;
	public float tuneI;
	public float tuneD;
	public GameObject tuneCamera;*/

	[Header("")]
	/// <summary>
	/// the controller of the Health Bar User Interface
	/// atm its empty at about y = -900
	/// </summary>
	public MaskController healthMask;
	public MiddleMarker middleMarker;
	public GameObject dummy;
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
	/// <summary>
	/// when true: cockpit movement translates to mech movement, if false, cockpit movement does nothing
	/// </summary>
	bool cockpitMovementType;
	/// <summary>
	/// false when the user just starded the game, true after button pressed and when the controller is initialized
	/// </summary>
	bool gameStart;

	Quaternion cockpitOriginRot;
	Quaternion mechOriginRot;

	//for camera mouse rotation not used anymore, can be deleted sooner or later
	Vector3 cockpitRot;

	//for cockpitposition;
	Vector3 cockpitPosition;

	GameObject[] armHingeLeft;
	GameObject[] armHingeRight;
	GameObject[] legHinge;

	GameObject weaponRight;
	GameObject weaponLeft;

	//todo: elbow starting offset.
	//Vector3 elbowPosArmR;
	//Vector3 elbowPosArmL;
	//Vector3 elbowPosLegR;
	//Vector3 elbowPosLegL;

	//Pid
	PIDControllerPos pidPos;
	PIDControllerRot pidRot;
	PIDControllerVel pidVel;

	//just to get values(plott):
	//PIDControllerVel pidVelCockpit;
	//PIDControllerPos rightHand;

	/// <summary>
	/// head position on the previous frame (only updated when controlling Layer 3)
	/// </summary>
	Vector3 prevHeadPosition;
	/// <summary>
	/// last head position that was used with UI mode OFF
	/// </summary>
	Vector3 lastHeadPosition;
	/// <summary>
	/// last head position that was used with ControllerLayer3 ON
	/// </summary>
	Vector3 lastHeadPosition2;
	/// <summary>
	/// last headForward. Used when current head rotation is not applied to Mech
	/// </summary>
	Vector3 lastHeadForward;
	/// <summary>
	/// last head position on the fixed timesteps
	/// </summary>
	Vector3 lastHeadPositionFixed;
	/// <summary>
	/// head dif of the last head kinda summed up over the history. is added to the mech vel target with a factor
	/// </summary>
	Vector3 velHead;
	/// <summary>
	/// this is the head position and NOT the HMD position
	/// </summary>
	Vector3 realHeadPosition;

	/*
	/// <summary>
	/// for moving hand smoothly
	/// </summary>
	Vector3 handPositionTMPSAVE;
	float tmpCounter = 0;*/

	bool onGround;

	void Start()
	{
		SteamVR_Camera.sceneResolutionScale = 1f;

		cockpitOriginRot = cockpit.transform.localRotation;
		mechOriginRot = cockpit.transform.parent.rotation;

		cockpitRot = Vector3.zero;
		armHingeRight = new GameObject[4];
		armHingeLeft = new GameObject[4];
		legHinge = new GameObject[4];

		//version with both arms

		//left arm
		Transform tmp;
		tmp = armLeft.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeLeft[i] = tmp.GetChild(i).gameObject;
			var rbody = armHingeLeft[i].AddComponent<Rigidbody>();
			rbody.useGravity = true;
		}
		//right arm
		tmp = armRight.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeRight[i] = tmp.GetChild(i).gameObject;
			var rbody = armHingeRight[i].AddComponent<Rigidbody>();
			rbody.useGravity = true;
		}
		//middle leg
		tmp = leg.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			Debug.Log(tmp.GetChild(i).gameObject);
			legHinge[i] = tmp.GetChild(i).gameObject;
			var rbody = legHinge[i].AddComponent<Rigidbody>();
			rbody.useGravity = true;
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

		Debug.Log(mechOriginRot.eulerAngles);

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


		//give cockpit and cockpittarget rigidbody 
		var rigidbody = cockpit.AddComponent<Rigidbody>();
		rigidbody.useGravity = true;
		rigidbody.mass = 100;
		//rigidbody.inertiaTensor = Vector3.Scale(rigidbody.inertiaTensor, new Vector3(10, 1, 10));
		Debug.Log(rigidbody.inertiaTensor);
		rigidbody.inertiaTensor = new Vector3(130, 130, 100);
		Debug.Log(rigidbody.inertiaTensor);


		//Initialize IK
		//Todo fix elbows, so mech location can be changed without it effecting the elbow -> save startlocation of elbow and change every frame

		//rightArmIK = new IKJoint(armHingeRight[0], armHingeRight[1], armHingeRight[2], armHingeRight[3], targetArmRight, elbowTargetArmR, maxForce, defaultSpring, defaultDampening);
		//leftArmIK = new IKJoint(armHingeLeft[0], armHingeLeft[1], armHingeLeft[2], armHingeLeft[3], targetArmLeft, elbowTargetArmL, maxForce, defaultSpring, defaultDampening);

		rightArmIK = new IKForce(armHingeRight[0], armHingeRight[1], armHingeRight[2], armHingeRight[3], targetArmRight, maxForce, 0);
		leftArmIK = new IKForce(armHingeLeft[0], armHingeLeft[1], armHingeLeft[2], armHingeLeft[3], targetArmLeft, maxForce, 0);
		legIK = new IKForce(legHinge[0], legHinge[1], legHinge[2], legHinge[3], targetLeg, maxForce, 1);




		//connect arms with cockpit
		setFixedJoint(cockpit, armHingeLeft[0]);
		setFixedJoint(cockpit, armHingeRight[0]);
		setFixedJoint(cockpit, legHinge[0]);


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

		//pidPos = new PIDControllerPos(cockpit, 38, 5, 8, new Vector3(0, 1, 0));
		//pidRot = new PIDControllerRot(cockpit, 50, 5, 10);
		pidRot = new PIDControllerRot(cockpit, 90, 5, 30);

		//todo
		//i believe for best results i have to apply the force to all rigidbodys (arms only about 70% to see them moving a bit)
		//pidVel = new PIDControllerVel(cockpit, 35, 0, 0.6f, new Vector3(1, 0, 1));
		//TODO reset it to old
		//pidVel = new PIDControllerVel(legHinge[3], 35, 0, 0.6f, new Vector3(1, 0, 1), 100);
		pidVel = new PIDControllerVel(legHinge[3], 35, 0, 0.6f, new Vector3(1, 0, 1), 100);

		//value plotting
		//pidVelCockpit = new PIDControllerVel(cockpit, 0, 0, 0, new Vector3(1, 0, 1), 100);
		//rightHand = new PIDControllerPos(armHingeRight[0], armHingeRight[3], 0, 0, 0, new Vector3(1, 1, 1));

		onGround = false;

		middleMarker.InitializeVariables(viveControllerLeft, viveControllerRight, head, controllerLeft, controllerRight);
		//middleMarker.InitializeVariables(viveControllerLeft, viveControllerRight, dummy);

		tiltType = true;
		cockpitMovementType = true;
		gameStart = false;

		//set head to normaluser hieght when no vr is connected. TODO also set camera?
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
			//Time.timeScale = 0;
			lastHeadPosition = new Vector3(0, 1.8f, 0);
			lastHeadPosition2 = new Vector3(0, 1.8f, 0);
			lastHeadForward = Vector3.forward;
		}
		else
		{
			//Time.timeScale = 0;
		}
	}

	void Update()
	{
		realHeadPosition = head.transform.localPosition - head.transform.forward * 0.14f;
		//At the start of the game.
		//TODO: think i get rid of timescale and instead dont coltroll mech rotation if one controler is in ui mode
		if (!gameStart)
		{
			//Now works with every button press. (also touchpad ;( )
			if (Input.GetKeyUp("joystick button 0") || Input.GetKeyUp("joystick button 2") || Input.GetKeyUp("joystick button 14") || Input.GetKeyUp("joystick button 15"))
			//if (Input.anyKey)
			{
				//set the controller device ids
				//viveControllerLeft.GetComponent<ControllerUI>().deviceId = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
				//viveControllerRight.GetComponent<ControllerUI>().deviceId = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
				viveControllerLeft.GetComponent<ControllerUI>().deviceId = (int)viveControllerLeft.GetComponent<SteamVR_TrackedObject>().index;
				viveControllerRight.GetComponent<ControllerUI>().deviceId = (int)viveControllerRight.GetComponent<SteamVR_TrackedObject>().index;

				//TODO: if something happens in the above part???
				gameStart = true;

				Debug.Log("any Key pressed");
			}
			lastHeadPosition = new Vector3(0, 1.8f, 0);
			lastHeadPosition2 = new Vector3(0, 1.8f, 0);
			//return;
		}

		bool leftControllerUiActive = viveControllerLeft.GetComponent<ControllerUI>().IsInUiMode();
		bool rightControllerUiActive = viveControllerRight.GetComponent<ControllerUI>().IsInUiMode();

		//checks and controlls UI mode
		//also triggers animations
		if (!GlobalVariables.UiActive && (leftControllerUiActive || rightControllerUiActive))
		{
			GlobalVariables.UiActive = true;
			UserInterface.GetComponent<Animator>().SetBool("Up", true);
			Debug.Log("test1");
		}
		else if (GlobalVariables.UiActive && (!leftControllerUiActive && !rightControllerUiActive))
		{
			GlobalVariables.UiActive = false;
			UserInterface.GetComponent<Animator>().SetBool("Up", false);
			Debug.Log("test2");
		}

		//checks and controlls ControlLayer3
		//could also trigger animations (like slerp to middle when ControllLayer3 starts
		if (!GlobalVariables.ControllLayer3 && (!leftControllerUiActive || !rightControllerUiActive))
		{
			GlobalVariables.ControllLayer3 = true;
		}
		else if (GlobalVariables.ControllLayer3 && (leftControllerUiActive && rightControllerUiActive))
		{
			GlobalVariables.ControllLayer3 = false;
		}

		//set lastHeadPosition
		if (!GlobalVariables.UiActive)
		{
			lastHeadPosition = realHeadPosition;
		}
		//set LastHeadPosition2
		if (GlobalVariables.ControllLayer3)
		{
			lastHeadPosition2 = realHeadPosition;
		}

		if (Input.GetKeyDown("joystick button 0") || Input.GetKeyDown("joystick button 2"))
		{
			cockpitMovementType = !cockpitMovementType;
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
			weaponLeft = AttachDetachWeapon(armHingeLeft[3], weaponLeft, leftArmIK.forward, leftArmFrontPart);
		}
		//now same for right
		if ((controllerRight.GetComponent<HandController>().connectedViveController != null && SteamVR_Controller.Input(controllerRight.GetComponent<HandController>().controllerID).GetHairTriggerDown()) || Input.GetKeyDown(KeyCode.E))
		{
			weaponRight = AttachDetachWeapon(armHingeRight[3], weaponRight, rightArmIK.forward, rightArmFrontPart);
		}

		var rbodyCockpit = cockpit.GetComponent<Rigidbody>();


		//for mech rotation:
		//different ways possible:
		//mech rotatas like head -> shoud only rotates very little if in about 30, and fully after more that 30
		//mech rotates to hands
		//mech rotates to average of head / arms   <-- we have a winner
		//mech rotates with controler input

		//Version 2 set roboter to average hand rotation + 2 times head + 1 times cockpit TODO: for all, set y 0 and then normalize

		Vector3 handR = controllerRight.transform.localPosition - lastHeadPosition;
		Vector3 handL = controllerLeft.transform.localPosition - lastHeadPosition;

		//TODO check if works like intended
		if (controllerRight.GetComponent<HandController>().connectedViveController == null)
		{
			handR = controllerRight.transform.localPosition - lastHeadPosition;
			//handR = controllerRight.transform.localPosition - realHeadPosition;
		}
		else
		{
			//handR = controllerRight.transform.localPosition - lastHeadPosition;
			handR = controllerRight.transform.localPosition - realHeadPosition;
		}

		if (controllerLeft.GetComponent<HandController>().connectedViveController == null)
		{
			handL = controllerLeft.transform.localPosition - lastHeadPosition;
			//handL = controllerLeft.transform.localPosition - realHeadPosition;
		}
		else
		{
			//handL = controllerLeft.transform.localPosition - lastHeadPosition;
			handL = controllerLeft.transform.localPosition - realHeadPosition;
		}

		handR.y = 0;
		handL.y = 0;


		//Fix mech rotation when looking up or down (more than 90°)
		//if (head.transform.forward.y > 0.5)
		//{
		//	headForward = head.transform.forward + -head.transform.up;
		//}
		//else if (head.transform.forward.y < -0.5)
		//{
		//	headForward = head.transform.forward + head.transform.up;
		//}
		//else
		//{
		//	headForward = head.transform.forward;
		//}


		//smooth version of the above version. works great

		Vector3 headForward = Vector3.zero;
		//When head.transform.forward.y is positive the head.transform.down is added to the forward
		//When head.transform.forward.y is negative the head.transform.up is added to the forward
		headForward = head.transform.forward - head.transform.up * head.transform.forward.y;
		headForward.y = 0;

		var combinedForward = handR.normalized + handL.normalized + cockpit.transform.forward;



		//when controlling mech directly only use head rotation if no hand is in UI mode
		if ((GlobalVariables.ControllLayer3 && !GlobalVariables.UiActive) || !SteamVR.active)
		{
			combinedForward += 2.5f * headForward.normalized;
			lastHeadForward = headForward;
		}
		else
		{
			combinedForward += 2.5f * lastHeadForward.normalized;
		}
		combinedForward.y = 0;
		//TODO: factor 50 is not jet tested
		//also have to adjust the distance we adjust the leg position
		// old: * Quaternion.Euler(50 * head.transform.localPosition.z + 10* Input.GetAxis("Vertical"), 0, -50 * head.transform.localPosition.x  - 10 * Input.GetAxis("Horizontal"))
		// needed to do: slerp the added rotation of vel so it doesnt look stange with this  + the physics introduced rotation
		//var rotation = Quaternion.Euler(50 * head.transform.localPosition.z, 0, -50 * head.transform.localPosition.x) * Quaternion.FromToRotation(Vector3.forward, combinedForward);
		//var rotation = Quaternion.FromToRotation(Vector3.forward, combinedForward);
		var rotation = Quaternion.LookRotation(Vector3.up, combinedForward);
		cockpitTarget.transform.rotation = Quaternion.Slerp(cockpitTarget.transform.rotation, rotation, 0.5f);

		//not used but yea...
		cockpitTarget.transform.position = cockpit.transform.position;


		//TODO: tilt robot in direction hes moving (only a bit)
		//TODO: tilt robot in direction hes facing when sneaking

		//set cameraRig Position to roboter
		if (SteamVR.active)
		{
			//in vr
			if (!GlobalVariables.ControllLayer3)
			{
				//idea is to move around in cockpit

				head.transform.parent.position = cockpit.transform.position;
			}
			else
			{
				//is 1,1,1 when height is also freezed, else its 1,0,1  need to do more when height is freezed
				Vector3 scaleVector = new Vector3(1, 0, 1);

				//idea is to lock user to middle of cockpit.
				//probably allow up/down movement

				head.transform.parent.position = cockpit.transform.position - Vector3.Scale(realHeadPosition, scaleVector);

				viveControllerLeft.GetComponent<ControllerUI>().UpdateHandController();
				viveControllerRight.GetComponent<ControllerUI>().UpdateHandController();

				//difference of the head to the last frame;
				var difHeadPosition = prevHeadPosition - realHeadPosition;

				//fix movement of controler (only do it when we dont have it in hand)
				if (controllerLeft.GetComponent<HandController>().connectedViveController == null)
				{
					controllerLeft.transform.position -= Vector3.Scale(difHeadPosition, scaleVector);
				}
				if (controllerRight.GetComponent<HandController>().connectedViveController == null)
				{
					controllerRight.transform.position -= Vector3.Scale(difHeadPosition, scaleVector);
				}

				prevHeadPosition = realHeadPosition;
			}

			//this tilts the vr area with the cockpit 
			if (tiltType)
			{
				head.transform.parent.rotation = Quaternion.Inverse(cockpitOriginRot) * cockpit.transform.rotation;
				head.transform.parent.rotation = Quaternion.Euler(-head.transform.parent.rotation.eulerAngles.x, 0, head.transform.parent.rotation.eulerAngles.y);
			}
			else
			{
				//resets the tilt. theoretically only needed on tilt switch.
				head.transform.parent.rotation = new Quaternion();
			}


		}
		else
		{
			//non vr version here:

			//moving in layer 1 doenst move in layer 2 since we controll layer 3
			head.transform.parent.parent.position = cockpit.transform.position - Vector3.Scale(realHeadPosition, new Vector3(1, 0, 1));




			//this tilts the vr area with the cockpit
			head.transform.parent.parent.rotation = Quaternion.Inverse(cockpitOriginRot) * cockpit.transform.rotation;
			head.transform.parent.parent.rotation = Quaternion.Euler(-head.transform.parent.parent.rotation.eulerAngles.x, 0, head.transform.parent.parent.rotation.eulerAngles.y);
		}

		//old rotation approach
		//cockpitTarget.transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * Time.deltaTime * 100, Vector3.forward);
		//cockpitTarget.transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * Time.deltaTime * 100, Vector3.right);



		//set target arms according to controler position
		float armFactor = 5.5f;

		//shoulder height estimate
		Vector3 shoulderPos;
		if (GlobalVariables.ControllLayer3)
		{
			shoulderPos = realHeadPosition;

		}
		else
		{
			shoulderPos = lastHeadPosition2;
		}
		//factor 0.78 because im ~1.8 and my shoulders are at ~1.4
		//shoulderPos.Scale(new Vector3(1f, 0.78f, 1f));
		//0.2 because distance between middle of head to the shoulder is about 20 cm
		shoulderPos -= new Vector3(0, 0.2f, 0);


		var rotTest = armHingeLeft[0].transform.localRotation.eulerAngles;
		rotTest.x = 0;
		rotTest.y = 0;
		//rotTest.z = 0;

		//targetArmLeft.transform.position = armHingeLeft[0].transform.TransformPoint(armHingeLeft[0].transform.parent.parent.rotation * (controllerLeft.transform.localPosition - shoulderPos) * armFactor);
		targetArmLeft.transform.position = armHingeLeft[0].transform.TransformPoint(Quaternion.Euler(-rotTest) * armHingeLeft[0].transform.parent.parent.rotation * (controllerLeft.transform.localPosition - shoulderPos) * armFactor);
		targetArmLeft.transform.position += armHingeLeft[0].transform.right * armLeft.transform.localPosition.x;
		//cockpit forward offset
		targetArmLeft.transform.position += cockpit.transform.up * 1f;
		//upwards offset
		targetArmLeft.transform.position += Vector3.up * 0.4f;

		//now right arm
		rotTest = armHingeLeft[0].transform.localRotation.eulerAngles;
		rotTest.x = 0;
		rotTest.y = 0;
		//rotTest.z = 0;

		targetArmRight.transform.position = armHingeRight[0].transform.TransformPoint(Quaternion.Euler(-rotTest) * armHingeRight[0].transform.parent.parent.rotation * (controllerRight.transform.localPosition - shoulderPos) * armFactor);
		targetArmRight.transform.position += armHingeRight[0].transform.right * armRight.transform.localPosition.x;
		//cockpit forward offset
		targetArmRight.transform.position += cockpit.transform.up * 1f;
		//upwards offset
		targetArmRight.transform.position += Vector3.up * 0.4f;

		//Version 2: uses real hand position (not effected by head position
		//targetArmLeft.transform.position = controllerLeft.transform.localPosition * humanRobotFactor + new Vector3(cockpitPosition.x, cockpitRayDownInfo.point.y, cockpitPosition.z);
		//targetArmRight.transform.position = controllerRight.transform.localPosition * humanRobotFactor + new Vector3(cockpitPosition.x, cockpitRayDownInfo.point.y, cockpitPosition.z);

		//Debug.Log(armHingeLeft[3].transform.localRotation.eulerAngles + " target: " + targetArmLeft.transform.rotation.eulerAngles);

		//set hand rotation, with slight tilt so it feels better
		targetArmLeft.transform.rotation = controllerLeft.transform.localRotation * Quaternion.Euler(-35, 0, 0);
		targetArmRight.transform.rotation = controllerRight.transform.localRotation * Quaternion.Euler(-35, 0, 0);


		//set leg position (depending on head height)
		//1.65 because im 1.8 and the wheel looks good at -3 default. -0.7 also looks good
		//targetLeg.transform.localPosition = new Vector3(0, head.transform.localPosition.y * -0.65f - 1f, -0.25f) - Quaternion.Inverse(cockpitTarget.transform.rotation * cockpitOriginRot) * Vector3.Scale(lastHeadPosition2, new Vector3(2, 0, 2));
		targetLeg.transform.localPosition = new Vector3(0, realHeadPosition.y * -0.65f - 1f, -0.25f);
		//Debug.Log(head.transform.localPosition);

		//updates IK
		//rightArmIK.UpdateTargetRotsGlobal();
		//leftArmIK.UpdateTargetRotsGlobal();

		//even needed?
		if (onGround)
		{
			if (!checkGround())
			{
				Debug.Log("left ground");
				onGround = false;
				//add gravity to all
			}
		}
		else
		{
			if (checkGround())
			{
				Debug.Log("on Ground");
				onGround = true;
				//remove gravity from all
			}
		}

		updateUserInterface();
	}

	private void LateUpdate()
	{
		//TODO:
		//put camera position here, its directly after the physics simulation
		//research for this first
	}

	private void FixedUpdate()
	{
		//also update this here because im not sure if update is always called befor this
		realHeadPosition = head.transform.localPosition - head.transform.forward * 0.14f;


		//Jumping
		//TODO: fix hover (lets only allow jump if on ground? ... + internal cooldown)
		//also add extra down force so its really like 9.8 gravity. (probably add gravity to arms when body is not on ground)
		if (Input.GetKey(KeyCode.Space))
		{
			cockpit.GetComponent<Rigidbody>().AddForce(Vector3.up * 20f, ForceMode.Acceleration);
		}

		//Mech movement here
		//idea: trackpat direts the Velocity the cockpit should have.
		//->	use pid to control the xz velocity
		//		and pid to control the x position to over the ground (disable when jumping)



		float speedfaktor = 8;
		Vector3 targetVel = cockpitTarget.transform.rotation * cockpitOriginRot * new Vector3(Input.GetAxis("Horizontal") * speedfaktor, 0, Input.GetAxis("Vertical") * speedfaktor);

		//more complex variant of calculating target vel: probably with square, but i leave it as it currently is.
		//Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		//float length = input.magnitude;
		//input = input * 



		//using head position diff from last physics calc to add to targetVel
		//Still todo: some kind of lerp with the diff so it is more than one physics calc
		if (GlobalVariables.ControllLayer3 && cockpitMovementType)
		{
			velHead += (realHeadPosition - lastHeadPositionFixed);
			velHead = velHead * 0.8f;
			targetVel += velHead * 120;
			targetVel.y = 0;
		}

		float airfactor = 1;

		//add extra break if its supposed to stand  (BUT ONLY ON GROUND)
		if (onGround)
		{
			var cockpitVel = cockpit.GetComponent<Rigidbody>().velocity;
			cockpitVel.y = 0;
			if (targetVel.magnitude < 0.2f && cockpitVel.magnitude > 0.05f)
			{
				//idea was to extra break. seems not neccesary?
				//targetVel += -cockpitVel;
				//targetVel = -cockpitVel * Mathf.Sqrt(Mathf.Sqrt(cockpitVel.magnitude));
				//Debug.Log("velSave");
			}
		}
		else
		{
			//targetVel *= 0.5f;
			airfactor = 0.2f;
		}

		//Debug.Log("targetVel: " + targetVel + " Cockpitvel : " + cockpitVel);


		//Sinus acceleration, independent on break;
		/*
		float factor = 0.01f;
		if (tmpCounter < 230)
		{
			targetVel = new Vector3(0, 0, 0);
		}
		else if ((tmpCounter - 230) * factor < Mathf.PI)
		{
			targetVel = new Vector3(0, 0, Mathf.Sin((tmpCounter - 230) * factor) * 10);
		}
		//else if (tmpCounter < 430)
		//{
		//	targetVel = new Vector3(0, 0, 1) * speedfaktor;
		//}
		else
		{
			targetVel = Vector3.zero;
		}*/

		/*
		//hand movement.
		float factor = 0.03f;
		if (tmpCounter == 330)
		{
			handPositionTMPSAVE = targetArmRight.transform.position;
		}else if(tmpCounter < 330)
		{

		}
		else if ((tmpCounter - 330) * factor < Mathf.PI *2)
		{
			//targetArmRight.transform.position= handPositionTMPSAVE + new Vector3(0, Mathf.Sin((tmpCounter - 330) * factor) * 3,0);
			//targetArmRight.transform.position = handPositionTMPSAVE + new Vector3( Mathf.Sin((tmpCounter - 330) * factor) * 3,0, 0);
			targetArmRight.transform.position = handPositionTMPSAVE + new Vector3((Mathf.Cos((tmpCounter - 330) * factor)-1) * -2, 0, 0);

			//targetArmRight.transform.position = handPositionTMPSAVE + new Vector3(Mathf.Sin((tmpCounter - 330) * factor) * 2.5f, 0, 0);
		}
		else
		{
			targetArmRight.transform.position = handPositionTMPSAVE;
		}
		*/

		//apply mech target velocity to ball
		pidVel.UpdateTarget(targetVel, airfactor);

		//TODO remove
		//pidVelCockpit.UpdateTarget(targetVel, airfactor);
		//rightHand.UpdateTarget(targetArmRight.transform.position);


		//update fixed update stuff in IK
		legIK.UpdateTargetRotsGlobal();
		rightArmIK.UpdateTargetRotsGlobal();
		leftArmIK.UpdateTargetRotsGlobal();
		pidRot.UpdateTarget(cockpitTarget.transform.rotation);
		//unused
		//pidPos.UpdateTarget(cockpitTarget.transform.position);

		lastHeadPositionFixed = realHeadPosition;

		/*

		if (tmpCounter <= 300)
		{
			Debug.Log(tmpCounter);
		}
		if (tmpCounter == 300)
		{
			Debug.Log("GO");
		}

		if(tmpCounter == 330)
		{
			Debug.Log(Time.time);
		}
		if ((tmpCounter - 330) * factor >= Mathf.PI * 2)
		{
			Debug.Log(Time.time);
			Debug.Log(tmpCounter);
		}
		tmpCounter++;*/


		//restart if falling to low:
		if (cockpit.transform.position.y < -50)
		{
			int scene = SceneManager.GetActiveScene().buildIndex;
			SceneManager.LoadScene(scene, LoadSceneMode.Single);
		}

	}

	/// <summary>
	/// detaches weapon or checks if new weapon is in front of hand an attaches it
	/// </summary>
	/// <param name="hand"></param>
	/// <param name="weapon"></param>
	/// <param name="handForward"></param>
	/// <returns></returns>
	private GameObject AttachDetachWeapon(GameObject hand, GameObject weapon, Vector3 handForward, GameObject armFrontPart)
	{
		if (weapon == null)
		{
			//find what arm is colliding with:
			//todo: ray or spherecast from hand, and look for collision with a new collider layer named pickable
			var weaponRay = new RaycastHit();
			LayerMask mask = LayerMask.GetMask(new string[] { "Weapon" });
			//Debug.DrawRay(hand.transform.position, hand.transform.rotation * -handForward * 2, Color.green);
			if (Physics.Raycast(hand.transform.position, hand.transform.rotation * -handForward * 2, out weaponRay, 5, mask))
			{
				//first collsion, rotate hand to rotation of object + also position, then create fixed joint between both, and let Joint + IK handle the rest
				//weaponRay.rigidbody.isKinematic = false;
				GameObject pickup = weaponRay.transform.gameObject;
				weapon = pickup;
				hand.transform.rotation = pickup.transform.rotation * (handForward.x < 0 ? Quaternion.Euler(-90, 0, 0) : Quaternion.Euler(-90, 180, 0));
				//hand.transform.rotation = pickup.transform.rotation * Quaternion.FromToRotation(Quaternion.FromToRotation(Vector3.right, Vector3.forward) * handForward, Vector3.down);
				hand.transform.position = pickup.transform.position - pickup.transform.rotation * Vector3.right * 0.35f;
				var joint = hand.AddComponent<FixedJoint>();
				joint.connectedBody = pickup.GetComponent<Rigidbody>();

				//decativate collision between arm part and weapon
				var weaponColliders = weapon.GetComponentsInChildren<Collider>();
				foreach (var i in weaponColliders)
				{
					Physics.IgnoreCollision(armFrontPart.GetComponent<Collider>(), i);
				}
			}
			else
			{
				var nonWeaponRay = new RaycastHit();
				mask = LayerMask.GetMask(new string[] { "Default" });

				if (Physics.Raycast(hand.transform.position, hand.transform.rotation * -handForward * 2, out nonWeaponRay, 1, mask))
				{
					//just create fixed joint to the object in front of hand
					GameObject pickup = nonWeaponRay.transform.gameObject;
					weapon = pickup;
					var joint = hand.AddComponent<FixedJoint>();
					var rBody = pickup.GetComponent<Rigidbody>();

					if (rBody != null)
					{
						joint.connectedBody = rBody; ;
					}


					//decativate collision between arm part and weapon
					var weaponColliders = weapon.GetComponentsInChildren<Collider>();
					foreach (var i in weaponColliders)
					{
						Physics.IgnoreCollision(armFrontPart.GetComponent<Collider>(), i);
					}
				}
			}



		}
		else
		{
			//detach weapon:
			//remove the fixedjoint:
			Destroy(hand.GetComponent<FixedJoint>());

			//weapon.GetComponent<Rigidbody>().isKinematic = true;


			//reactivate collision between arm and weapon again
			var weaponColliders = weapon.GetComponentsInChildren<Collider>();
			foreach (var i in weaponColliders)
			{
				Physics.IgnoreCollision(armFrontPart.GetComponent<Collider>(), i, false);
			}
			weapon = null;
		}
		return weapon;
	}

	void setFixedJoint(GameObject one, GameObject two)
	{
		var joint = one.AddComponent<FixedJoint>();
		joint.connectedBody = two.GetComponent<Rigidbody>();
	}

	public void changeTiltStyle()
	{
		tiltType = !tiltType;
		Debug.Log("works?" + tiltType);
	}

	/// <summary>
	/// checks if the ball is standing on ground
	/// </summary>
	/// <returns></returns>
	public bool checkGround()
	{
		var rayInfo = new RaycastHit();
		//TODO: need to adjust the mask    theoretically want mask that is NOT mech? possible?
		LayerMask mask = LayerMask.GetMask(new string[] { "Default" });
		return Physics.SphereCast(legHinge[3].transform.position + Vector3.up, 0.45f, Vector3.down * 1f, out rayInfo, 1.1f, mask);
	}

	/// <summary>
	/// updates all user interface stuff
	/// 
	/// FRIENDLY REMINDER:	delte raycast thing on canvas since ew have no mouse ezez
	///				ALSO	remember to set ui layer to ui or uiInteraction
	/// </summary>
	public void updateUserInterface()
	{
		healthMask.moveMask((Mathf.Sin(Time.time * 0.5f) + 1) * 0.5f);

		middleMarker.UpdateGroundMarker(Vector3.Scale(realHeadPosition, new Vector3(1, 0, 1)));
		middleMarker.UpdateFrontMarker(cockpitTarget.transform.rotation);
	}

	/// <summary>
	/// dont want to do this... 
	/// </summary>
	public void OnApplicationQuit()
	{
		/*
		leftArmIK.appExit();
		rightArmIK.appExit();
		legIK.appExit();
		pidVel.appExit();
		pidVelCockpit.appExit();
		rightHand.appExit();
		*/
	}
}