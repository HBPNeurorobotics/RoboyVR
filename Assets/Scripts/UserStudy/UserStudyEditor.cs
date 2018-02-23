using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class UserStudyEditor {

    /// <summary>
    /// This menu is used to delete all key-value pairs currently stored in the PlayerPrefs.
    /// </summary>
	[MenuItem("UserStudy/Reset PlayerPrefs")]
    static void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
