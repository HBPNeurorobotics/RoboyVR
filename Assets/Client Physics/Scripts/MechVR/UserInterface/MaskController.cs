using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script has to be on mask
/// what it supposed to do:
/// allows moving the mask without moving the childs
/// 
/// Currently specificly used for helth bars
/// </summary>
public class MaskController : MonoBehaviour
{
	public Vector3 startPosition;
	public Vector3 currentPosition;
	public Vector3 maxPosition;

	/// <summary>
	/// moves mask in dircetion of the moveVector
	/// </summary>
	/// <param name="moveVector"></param>
	public void moveMask(Vector3 moveVector)
	{
		this.gameObject.transform.localPosition += moveVector;
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			gameObject.transform.GetChild(i).transform.localPosition -= moveVector;
		}
		currentPosition = gameObject.transform.localPosition;
	}

	/// <summary>
	/// moves mask to factor % in direction of maxPosition
	/// </summary>
	/// <param name="factor"></param>
	public void moveMask(float factor)
	{
		Vector3 targetPos = startPosition + (maxPosition - startPosition) * (1 - factor);
		gameObject.transform.localPosition = targetPos;
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			gameObject.transform.GetChild(i).transform.localPosition = -targetPos;
		}
		currentPosition = gameObject.transform.localPosition;

		UpdateColor(factor);
	}

	/// <summary>
	/// changes color of the first child (yes ugly hardcoded, will look at ot later
	/// </summary>
	/// <param name="factor"></param>
	public void UpdateColor(float factor)
	{
		Color newColor;
		if (factor < 0.25f)
		{
			newColor = Color.Lerp(Color.red, Color.yellow, factor * 4);
		}
		else
		{
			newColor = Color.Lerp(Color.yellow, Color.green, (factor - 0.25f) * (1 / 0.75f));
		}
		gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = newColor;
	}
}
