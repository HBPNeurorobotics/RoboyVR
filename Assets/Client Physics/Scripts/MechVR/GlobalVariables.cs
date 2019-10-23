using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// global variables of the Mech the player controls.  --> TODO: change system when starting iwth multipalyer?
/// </summary>
public static class GlobalVariables
{
	/// <summary>
	/// when at least one hand is free and can controll ui
	/// </summary>
	public static bool UiActive = false;
	/// <summary>
	///  when the player controlls the mech (layer 3)
	///  -> cant move in layer 2
	/// </summary>
	public static bool ControllLayer3 = false;
	public static float health = 1f;
}
