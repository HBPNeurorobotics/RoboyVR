using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserStudyDataManager {

    #region PRIVATE_MEMBER_VARIABLES

    /// <summary>
    /// Time when the avatar leaves the start tracking area.
    /// </summary>
    private static float _startTime = 0;

    /// <summary>
    /// Time when the avatar reaches the goal tracking area.
    /// </summary>
    private static float _endTime = 0;

    /// <summary>
    /// The identifier is a 7 digit code which is used to uniquely and anonymously identify the user.
    /// </summary>
    private static string _identifier;

    #endregion



    #region Time tracking
    /// <summary>
    /// Starts with the time tracking.
    /// </summary>
    public static void startTimeTracking()
    {
        _startTime = Time.time;
    }

    /// <summary>
    /// Ends the time tracking.
    /// </summary>
    public static void endTimeTracking()
    {
        _endTime = Time.time;
        Debug.Log(_startTime - _endTime);
    }
    #endregion 

}
