using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
 * for setup: 
 * put collider(trigger) + rigidbody on controler, have it in ui layer
 * Give buttons a cube with collider
 * put button in UI layer
 * Change color tilt if you want to see it
 * 
 * !!!!!!Navigation to none!!
 * 
 * TODO: make animations or spire stuff for button presses?
 * 
 * TODO: for later: .getType to see type of selectable
 */
public class ControllerUI : MonoBehaviour
{
	/// <summary>
	/// vive controler device id
	/// </summary>
	public int deviceId;

	/// <summary>
	/// list of colliders the vive controller is currently in contact with
	/// </summary>
	List<Collider> colliders;

	/// <summary>
	/// the GameObject that is currently held by the vive controler, or else null
	/// </summary>
	private GameObject connectedHand;

	// Use this for initialization
	void Start()
	{
		colliders = new List<Collider>();
		//initialize connected hand with null;
		connectedHand = null;
	}

	// Update is called once per frame
	void Update()
	{
		/*
		for (int i = 0; i < 20; i++)
		{
			if (Input.GetKeyDown("joystick button " + i))
			{
				Debug.Log("joystick button " + i);
			}
		}*/

		//NOT WORKING
		//Debug.Log(Input.GetAxis("GripAxisRight"));
		//Debug.Log(Input.GetAxis("GripAxisLeft"));


		//testing the different buttons
		/*
		if (SteamVR_Controller.Input(deviceId).GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
		{
			//when trigger is pressed deep
			Debug.Log("trigger getpress spams");
		}

		if (SteamVR_Controller.Input(deviceId).GetHairTrigger())
		{
			//when trigger is pressed slightly
			Debug.Log("GetHairTrigger");
		}


		if (SteamVR_Controller.Input(deviceId).GetHairTriggerDown())
		{
			//somewhen when its moves down, is not spammed but not truly 1 time
			Debug.Log("GetHairTriggerDown");
			Debug.Log(colliders);
		}

		if (SteamVR_Controller.Input(deviceId).GetHairTriggerUp())
		{
			//always after the triggerdown
			Debug.Log("GetHairTriggerUp");
		}

		if (SteamVR_Controller.Input(deviceId).GetPress(Valve.VR.EVRButtonId.k_EButton_Grip))
		{
			//spammed when pressed
			Debug.Log("grip works");
		}
		*/


		if (colliders.Count != 0)
		{
			colliders.Sort((one, two) => (one.transform.position - this.transform.position).sqrMagnitude.CompareTo((two.transform.position - this.transform.position).sqrMagnitude));

			var button = colliders[0].transform.parent.GetComponent<Selectable>();


			if (button != null)
			{
				//we control a button here

				//new system, uses steamvr
				var pointer = new PointerEventData(EventSystem.current); // pointer event for Execut
				if (SteamVR_Controller.Input(deviceId).GetHairTriggerDown() || Input.GetKeyDown(KeyCode.R))
				{
					//somewhen when its moves down, is not spammed but not truly 1 time
					Debug.Log("GetHairTriggerDown");

					//fires click event + press down + pressup animation -> both animations fuck up
					//ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.submitHandler);
					//fires click event without animation
					ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
					//then trigger the pressed animation
					button.animator.SetTrigger("Pressed");
				}
			}
			else
			{
				//we controll the hand controller here

				if (SteamVR_Controller.Input(deviceId).GetHairTriggerDown())
				{
					//code to set hand controler to vive controler
					connectedHand = colliders[0].gameObject;
					this.GetComponent<Collider>().enabled = false;
					colliders[0].enabled = false;
					connectedHand.GetComponent<HandController>().connectedViveController = this.gameObject;
					connectedHand.GetComponent<HandController>().controllerID = deviceId;
					//connectedHand.transform.position = this.transform.position;
					//connectedHand.transform.rotation = this.transform.rotation;
					//connectedHand.transform.SetParent(this.transform);

					//go though all other colliders and call ontriggerexit
					for (int i = 1; i < colliders.Count; i++)
					{
						OnTriggerExit(colliders[i]);
					}
					colliders.Clear();
				}
			}
		}

		//disconnect body: TODO: have to change when grip button gets other usage because it is spammed
		if (connectedHand != null && deviceId >= 0 && SteamVR_Controller.Input(deviceId).GetPress(Valve.VR.EVRButtonId.k_EButton_Grip))
		{
			connectedHand.GetComponent<Collider>().enabled = true;
			connectedHand.GetComponent<HandController>().connectedViveController = null;
			connectedHand = null;
			this.GetComponent<Collider>().enabled = true;
		}



	}

	public void UpdateHandController()
	{
		//set the HandTorus to the controler position rotation
		if (connectedHand != null)
		{
			connectedHand.transform.position = this.transform.position;
			connectedHand.transform.rotation = this.transform.rotation;
		}
	}

	public void test()
	{
		Debug.Log("huhu");
	}

	/*
	void OnTriggerStay(Collider col)
	{

		//problems: can miss down or up event because not called every frame 
		//-> here only some kind of hold down, but i need submitHandler or pointerClickHandler for the click ti register

		var button = (Button)col.transform.parent.GetComponent<Selectable>();

		var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
		if (Input.GetKeyDown(KeyCode.H)) // force hover
		{
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
		}
		if (Input.GetKeyDown(KeyCode.U)) // un-hover (end hovering)
		{
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
		}
		if (Input.GetKeyDown(KeyCode.S)) // submit (~click)
		{
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.submitHandler);
		}
		if (Input.GetKeyDown(KeyCode.P)) // down: press
		{
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
		}
		if (Input.GetKeyUp(KeyCode.P)) // up: release
		{
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
		}
		if (Input.GetKeyUp(KeyCode.R)) // fires click event without color change
		{
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
		}
	}*/

	void OnTriggerEnter(Collider col)
	{
		var button = (Button)col.transform.parent.GetComponent<Selectable>();

		if (button != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
		}
		colliders.Add(col);

	}

	void OnTriggerExit(Collider col)
	{
		//probably have to clean up stuff if leaving during press???

		var button = (Button)col.transform.parent.GetComponent<Selectable>();

		if (button != null)
		{
			var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute

			//end hover mode (important: select button mode to none so it works if clicked)
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
			ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
		}
		colliders.Remove(col);
	}

	/// <summary>
	/// check if this vive controller currnetly interacts with UI or not
	/// </summary>
	/// <returns></returns>
	public bool IsInUiMode()
	{
		if (SteamVR.active)
		{
			return connectedHand == null;
		}
		else
		{
			return false;
		}
	}
}
