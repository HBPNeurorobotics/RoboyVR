using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public static class UserStudyEditor{

    /// <summary>
    /// This menu is used to delete all key-value pairs currently stored in the PlayerPrefs.
    /// </summary>
	[MenuItem("UserStudy/Reset PlayerPrefs")]
    static void ResetPlayerPrefs()
    {
        if (!Application.isPlaying)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Resetted teh PlayerPrefs!");
        }
    }

    [MenuItem("UserStudy/Finish SurveyPart")]
    static void FinishSurveyPart()
    {
        if (Application.isPlaying)
        {
            UserStudyDataManager.endTimeTracking();
            UserStudyDataManager.endSurveyPart();
        }
    }

    [MenuItem("UserStudy/Change Starting Method/Myo")]
    static void ChangeStartingMethodToMyo()
    {
        if (Application.isPlaying)
        {
            UserStudyDataManager.setStartingControlMethod(1);
            GameObject.FindObjectOfType<UserStudySceneManager>().UpdateCurrentMethod();
        }
    }

    [MenuItem("UserStudy/Change Starting Method/Controller")]
    static void ChangeStartingMethodToController()
    {
        if (Application.isPlaying)
        {
            UserStudyDataManager.setStartingControlMethod(0);
            GameObject.FindObjectOfType<UserStudySceneManager>().UpdateCurrentMethod();
        }
    }
}
