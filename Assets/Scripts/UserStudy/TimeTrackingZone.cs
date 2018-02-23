using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrackingZone : MonoBehaviour {

    public bool startTimeTracker = true;

    /// <summary>
    /// Starts the tracking when the user leaves the start tracking area.
    /// </summary>
    /// <param name="other">Collider of the avatar.</param>
    void OnTriggerExit(Collider other)
    {
        if (startTimeTracker && other.gameObject.name.Contains("avatar"))
        {
            UserStudyDataManager.startTimeTracking();
        }

    }

    /// <summary>
    /// Stops the tracking when the user enters the end tracking area.
    /// </summary>
    /// <param name="other">Collider of the avatar.</param>
    void OnTriggerEnter(Collider other)
    {
        if (!startTimeTracker && other.gameObject.name.Contains("avatar"))
        {
            UserStudyDataManager.endTimeTracking();
            UserStudyDataManager.endSurveyPart();
        }
    }
}
