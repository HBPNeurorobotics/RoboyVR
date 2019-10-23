using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// script that updates the thing on the ground that marks where the middle of the playspace is
/// </summary>
public class MiddleMarker : MonoBehaviour
{
	//for ground marker;
	public GameObject playerGround;
	public GameObject shaft;

	//for miniature front marker
	public GameObject miniaturePlayer;
	public GameObject miniatureHead;
	public GameObject arrow;
	public GameObject border;
	public GameObject armLeft;
	public GameObject armRight;

	GameObject handL;
	GameObject handR;
	GameObject head;
	/// <summary>
	/// the controller in the cockpit
	/// </summary>
	GameObject handRController;
	/// <summary>
	/// the controller in the cockpit
	/// </summary>
	GameObject handLController;

	Vector3 headOriginPos;
	Quaternion headOriginRot;

	//like in mech script
	GameObject[] armHingeLeft;
	GameObject[] armHingeRight;

	FakeIK IKLeft;
	FakeIK IKRight;

	/// <summary>
	/// is true if it was close to bpundary on the last frame
	/// </summary>
	bool closeToBoundary;

	/// <summary>
	/// initializes the variables. is called from Mech2
	/// </summary>
	/// <param name="handL"></param>
	/// <param name="handR"></param>
	public void InitializeVariables(GameObject handL, GameObject handR, GameObject head, GameObject handLController, GameObject handRController)
	{
		this.handL = handL;
		this.handR = handR;
		this.head = head;
		this.handLController = handLController;
		this.handRController = handRController;
		headOriginPos = miniatureHead.transform.localPosition;
		headOriginRot = miniatureHead.transform.localRotation;

		armHingeRight = new GameObject[4];
		armHingeLeft = new GameObject[4];

		//left arm
		Transform tmp;
		tmp = armLeft.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeLeft[i] = tmp.GetChild(i).gameObject;
			//var rbody = armHingeLeft[i].AddComponent<Rigidbody>();
			//rbody.useGravity = true;
		}
		//right arm
		tmp = armRight.transform.GetChild(0);
		for (int i = 0; i < 4; i++)
		{
			armHingeRight[i] = tmp.GetChild(i).gameObject;
			//var rbody = armHingeRight[i].AddComponent<Rigidbody>();
			//rbody.useGravity = true;
		}

		IKLeft = new FakeIK(armHingeLeft[0], armHingeLeft[1], armHingeLeft[2], armHingeLeft[3], armHingeLeft[3]);
		IKRight = new FakeIK(armHingeRight[0], armHingeRight[1], armHingeRight[2], armHingeRight[3], armHingeRight[3]);


		var rect = new Valve.VR.HmdQuad_t();

		if (!SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size.Calibrated, ref rect))
		{
			print("Not able to get vr bounds");
			SteamVR_PlayArea.GetBounds(SteamVR_PlayArea.Size._400x300, ref rect);
		}

		float xDim = Mathf.Abs(rect.vCorners0.v0 - rect.vCorners1.v0);
		float zDim = Mathf.Abs(rect.vCorners0.v2 - rect.vCorners3.v2);
		border.transform.localScale = new Vector3(xDim, zDim, 1);


	}

	/// <summary>
	/// Updates the middle marker on the ground:
	/// problem, only seen when looking down or moving backwards.
	/// </summary>
	/// <param name="playerLocation"></param>
	public void UpdateGroundMarker(Vector3 playerLocation)
	{
		playerGround.transform.localPosition = Vector3.zero;
		//playerGround.transform.localPosition = playerLocation;
		shaft.transform.localPosition = playerLocation / 2;
		shaft.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, playerLocation);
		shaft.transform.localScale = new Vector3(1, 1, playerLocation.magnitude * 5);
	}

	/// <summary>
	/// Updates middleMarker in front of the player
	/// minature of the player (body head leg arms) with an arrow pointing to the middle
	/// first version will only be a capsule with arrow to middle 
	/// </summary>
	public void UpdateFrontMarker(Quaternion mechRotation)
	{
		//Miniature Size is right(in blender) and then scaled down in the parent.
		//this way we dont have to scale the player position and the distances are accurate
		//or probably scale the distance a bit to encourage the user to go back to middle
		//also possible: draw circle around to show walls??  <-- or even show playspace boundary

		Vector3 playerLocation = Vector3.Scale(head.transform.localPosition - head.transform.forward * 0.14f, new Vector3(1, 0, 1));


		miniaturePlayer.transform.localPosition = playerLocation;
		Quaternion tmpRot = head.transform.rotation;
		tmpRot.x = 0;
		tmpRot.z = 0;
		miniaturePlayer.transform.localRotation = tmpRot * headOriginRot;
		miniatureHead.transform.localRotation = head.transform.rotation * headOriginRot;
		miniatureHead.transform.localPosition = playerLocation + headOriginPos;


		// TODO: arm is dependent on the body rotation! -> needs to be independent
		armHingeLeft[3].transform.localPosition = Quaternion.Inverse(armRight.transform.parent.localRotation) * handL.transform.localPosition - armLeft.transform.localPosition + new Vector3(0, 0, -0.7f) - Quaternion.Inverse(armRight.transform.parent.localRotation) * armRight.transform.parent.localPosition;
		armHingeRight[3].transform.localPosition = Quaternion.Inverse(armRight.transform.parent.localRotation) * handR.transform.localPosition - armRight.transform.localPosition + new Vector3(0, 0, -0.7f) - Quaternion.Inverse(armRight.transform.parent.localRotation) * armRight.transform.parent.localPosition;

		armHingeLeft[3].transform.localRotation = Quaternion.Inverse(armRight.transform.parent.rotation) * handL.transform.localRotation;
		armHingeRight[3].transform.localRotation = Quaternion.Inverse(armRight.transform.parent.rotation) * handR.transform.localRotation;

		arrow.transform.localPosition = playerLocation / 2;
		arrow.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, playerLocation);
		arrow.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, playerLocation) * Quaternion.Euler(-90, 0, 180);
		arrow.transform.localScale = new Vector3(1, playerLocation.magnitude, 1);

		miniaturePlayer.transform.parent.localRotation = Quaternion.Inverse(mechRotation);




		IKLeft.UpdateAll();
		IKRight.UpdateAll();

		//Border to red flashing (or somewhat) when one part is near the border
		//dim of the border is the distance from upper to lower border
		float minDistance = float.PositiveInfinity;
		//head;
		minDistance = Mathf.Min(border.transform.localScale.x / 2f - Mathf.Abs(head.transform.localPosition.x), minDistance);
		minDistance = Mathf.Min(border.transform.localScale.y / 2f - Mathf.Abs(head.transform.localPosition.z), minDistance);
		//same for rest
		minDistance = Mathf.Min(border.transform.localScale.x / 2f - Mathf.Abs(handL.transform.localPosition.x), minDistance);
		minDistance = Mathf.Min(border.transform.localScale.y / 2f - Mathf.Abs(handL.transform.localPosition.z), minDistance);

		minDistance = Mathf.Min(border.transform.localScale.x / 2f - Mathf.Abs(handR.transform.localPosition.x), minDistance);
		minDistance = Mathf.Min(border.transform.localScale.y / 2f - Mathf.Abs(handR.transform.localPosition.z), minDistance);
		//unit is meter so under 20 cm
		if (minDistance < 0.50f)
		{
			float factor = 1 - (Mathf.Max(minDistance, 0f) * 2f);
			var color = Color.Lerp(Color.white, Color.red, factor);
			closeToBoundary = true;
			var renderer = border.GetComponent<Renderer>();
			//renderer.material.color = Color.Lerp(Color.white, Color.red, (1 - Mathf.Pow(Mathf.Sin(Time.unscaledTime * 80) / 2f + 0.5f, 2)) * factor);
			renderer.material.color = color;

			renderer = arrow.GetComponent<Renderer>();
			renderer.material.color = color;

			for (int i = 0; i < handLController.transform.childCount; i++)
			{
				renderer = handLController.transform.GetChild(i).GetComponent<Renderer>();
				renderer.material.color = color;
			}
			for (int i = 0; i < handRController.transform.childCount; i++)
			{
				renderer = handRController.transform.GetChild(i).GetComponent<Renderer>();
				renderer.material.color = color;
			}

		}
		else if (closeToBoundary)
		{
			//reset color to white once when its out of the close part
			var renderer = border.GetComponent<Renderer>();
			renderer.material.color = Color.white;

			renderer = arrow.GetComponent<Renderer>();
			renderer.material.color = Color.white;

			for (int i =0; i < handLController.transform.childCount; i++)
			{
				renderer = handLController.transform.GetChild(i).GetComponent<Renderer>();
				renderer.material.color = Color.white;
			}
			for (int i = 0; i < handRController.transform.childCount; i++)
			{
				renderer = handRController.transform.GetChild(i).GetComponent<Renderer>();
				renderer.material.color = Color.white;
			}
		}
	}
}
