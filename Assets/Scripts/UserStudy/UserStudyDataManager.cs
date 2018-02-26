using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UserStudyDataManager {

    #region PRIVATE_MEMBER_VARIABLES
    /// <summary>
    /// Enum representing the names of the different stages used during the User Study.
    /// </summary>
    private enum StageName {OpenAre = 0, Pioneer = 1, Husky = 2};

    /// <summary>
    /// Enum representing the different control types of the avatar.
    /// </summary>
    private enum ControlMethod { Myo = 1, Controller = 0};

    /// <summary>
    /// Parser to store the survey data into a CSV
    /// </summary>
    private static CSVParser _parser = null;

    /// <summary>
    /// The maximal number of levels used in this user study.
    /// </summary>
    private static int _maxLevelCount = 5;

    /// <summary>
    /// Integer identifying the selected starting method of the study.
    /// </summary>
    private static int _startingMethod = 0;

    /// <summary>
    /// Integer representing the control method of the actual level.
    /// </summary>
    private static int _currentMethod = 0;

    /// <summary>
    /// Integer representing the current level of the user study.
    /// </summary>
    private static int _currentLevel = 0;

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
    private static string _identifier = "";

    /// <summary>
    /// The key used in the PlayerPrefs to store the user's identifier.
    /// </summary>
    private static string _keyIdentifier = "userID";

    /// <summary>
    /// The key used in the PlayerPrefs to store the start control method.
    /// </summary>
    private static string _keyStartControl = "startContr";

    /// <summary>
    /// The key used in the PlayerPrefs to store the current level.
    /// </summary>
    private static string _keyCurrentLevel = "level";

    #endregion

    // The static constructor is called when the game starts
    static UserStudyDataManager()
    {
        // Initialize the CSV parser
        _parser = new CSVParser("Saved_Data.csv");
        _parser.Start();
        
        // Write header of the CSV file if file not already exists
        _parser.writeHeader("identifier,order,stage,method,time", 5);

        // Read information from PlayerPrefs if availabl
        if (PlayerPrefs.HasKey(_keyIdentifier))
        {
            // Set identifier
            _identifier = PlayerPrefs.GetString(_keyIdentifier, "");

            if (PlayerPrefs.HasKey(_keyStartControl))
            {
                _startingMethod = PlayerPrefs.GetInt(_keyStartControl);
            }
            if (PlayerPrefs.HasKey(_keyCurrentLevel))
            {
                _currentLevel = PlayerPrefs.GetInt(_keyCurrentLevel);
            }

            // Set the current control method according to (((int)(level / 4) + startMethod) % 2
            _currentMethod = (((int)(_currentLevel / 3)) + _startingMethod) % 2;
            Debug.Log(_currentMethod);
        }
        else
        {
            // Determine starting control method
            _startingMethod = Random.Range(0, 2);
            _currentMethod = _startingMethod;
        }

    }
    
    /// <summary>
    /// Tells if the identifier is still needed or already available due to former levels.
    /// </summary>
    /// <returns></returns>
    public static bool identifierNeeded()
    {
        return _identifier.Equals("");
    }

    /// <summary>
    /// Sets the user's identifier to the given one if not already set.
    /// </summary>
    /// <param name="userID">A 7 digit code representing the user's identifier.</param>
    public static void setIdentifier(string userID)
    {
        if (_identifier.Equals(""))
        {
            _identifier = userID;
            PlayerPrefs.SetString(_keyIdentifier, _identifier);
            PlayerPrefs.SetInt(_keyStartControl, (_startingMethod));
        }
    }

    /// <summary>
    /// Returns the control method of the actual level.
    /// </summary>
    /// <returns></returns>
    public static int getCurrentControlMethod()
    {
        return _currentMethod;
    }

    /// <summary>
    /// Returns the control method of the actual level as a string.
    /// </summary>
    /// <returns></returns>
    public static string getCurrentcontrolMethodAsString()
    {
        return (ControlMethod)(_currentMethod)+"";
    }

    /// <summary>
    /// Returns the maximal number of levels.
    /// </summary>
    /// <returns></returns>
    public static int getMaxLevelCount()
    {
        return _maxLevelCount;
    }

    /// <summary>
    /// Returns the current level number.
    /// </summary>
    /// <returns></returns>
    public static int getCurrentLevel()
    {
        return _currentLevel;
    }

    /// <summary>
    /// This method stores the time data and user information into the CSV file.
    /// If this was not the last level of the survey, the user information are also stored into the PlayerPrefs.
    /// </summary>
    public static void endSurveyPart()
    {
        // Write to CSV
        _parser.appendValues(_identifier + "," + _currentLevel + "," + (StageName)(_currentLevel % 3) + "," + getCurrentcontrolMethodAsString() + "," + (_endTime - _startTime), 5, true);

        // Wrtie to PlayerPrefs
        if(_currentLevel < _maxLevelCount)
        {
            PlayerPrefs.SetInt(_keyCurrentLevel, (_currentLevel + 1));
        }
        else
        {
            PlayerPrefs.DeleteAll();
        }
        SceneManager.LoadScene("FinishMenu");
    }

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
    }
    #endregion 

}
