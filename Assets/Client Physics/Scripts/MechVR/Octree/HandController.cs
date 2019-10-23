using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// simple storage script attached to the hand in the cockpit, so i can read out what controller id is connected tot he handCOntroller
/// </summary>
public class HandController : MonoBehaviour
{

	/// <summary>
	/// the vive controller or null
	/// </summary>
	public  GameObject connectedViveController;

	/// <summary>
	/// Device id of the vive Controller
	/// </summary>
	public int controllerID;
}
