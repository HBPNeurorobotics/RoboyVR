using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// used to controll menu ui
/// This cript is needed to start animations by button presses
/// </summary>
public class UserInterface : MonoBehaviour
{
	/// <summary>
	/// the animator
	/// </summary>
	public Animator animator;

	/// <summary>
	/// call to toggle the test ui right pannel
	/// </summary>
	public void ToogleRight()
	{
		animator.SetTrigger("Right");
	}

	public void ChangeCockpitMode()
	{
		GlobalVariables.ControllLayer3 = !GlobalVariables.ControllLayer3;
	}

	public void ReloadLevel()
	{
		int scene = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(scene, LoadSceneMode.Single);
	}

	public void SetQuality(int qualityLevel)
	{
		if(qualityLevel < 3)
		{
			SteamVR_Camera.sceneResolutionScale = 1;
			QualitySettings.SetQualityLevel(qualityLevel, true);
		}
		else
		{
			SteamVR_Camera.sceneResolutionScale = 1.5f;
			QualitySettings.SetQualityLevel(2, true);
		}

	}
}
