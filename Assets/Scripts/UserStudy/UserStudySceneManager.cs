using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserStudySceneManager : MonoBehaviour {

    #region PRIVATE_MEMBER_VARIABLES

    /// <summary>
    /// Private GameObject reference to the avatar.
    /// </summary>
    private GameObject _avatar;

    /// <summary>
    /// The identifier to uniquely identify the user's avatar and the corresponding topics
    /// </summary>
    private string _avatarId = "";

    /// <summary>
    /// Boolean used to trigger the addition of a collider to the avatar.
    /// </summary>
    private bool _addCollider = true;

    /// <summary>
    /// Boolean used to distinguish between the first level and all other levels.
    /// </summary>
    private bool _firstSurvey = true;

    /// <summary>
    /// Boolean indicating if the control method needs to be set or not.
    /// </summary>
    private bool _setControlMethod = true;

    /// <summary>
    /// Integer representing the current level of the user study.
    /// </summary>
    private int _currentLevel = 0;

    #endregion

    #region PUBLIC_MEMBER_VARIABLES

    public enum SceneType{ MainMenu, Survey, FinishMenu};

    public SceneType currentScene = SceneType.Survey;

    /// <summary>
    /// This gameObjects encompasses the whole survey instruction which is only visible before the first level.
    /// </summary>
    public GameObject surveyInstruction;

    /// <summary>
    /// Reference to the surveyCount.
    /// </summary>
    public Text surveyCount;

    /// <summary>
    /// Reference to the text field which shows the next survey method.
    /// </summary>
    public Text nextSurveyMethod;

    /// <summary>
    /// Reference to the inputField which contains the user's identifier.
    /// </summary>
    public InputField identifier;

    /// <summary>
    /// This vector array contains the starting positions of the avatar for the respective stage.
    /// </summary>
    public Vector3[] avatarPosition = new Vector3[3];

    /// <summary>
    /// This vector array contains the positions of the robot for the respective stage.
    /// </summary>
    public Vector3[] robotPosition = new Vector3[3];

    /// <summary>
    /// Indicates the current stage of the user study.
    /// </summary>
    public int currentStage = 1;

    /// <summary>
    /// Boolean indicating if the stage set in the inspector should be considered or not.
    /// </summary>
    public bool enableManuallySetStage = false;

    #endregion

    // Use this for initialization
    void Start () {
        _currentLevel = UserStudyDataManager.getCurrentLevel();

        if(currentScene == SceneType.Survey)
        {
            if (!enableManuallySetStage)
            {
                // Set current stage
                currentStage = (int)(_currentLevel / 2);
            }

            // Set desired avatar position
            FindObjectOfType<NRPBackendManager>().avatarPosition = avatarPosition[currentStage];

            // Initialize start time tracking area
            GameObject startTracker = Instantiate(Resources.Load("UserStudy/TriggerArea") as GameObject, new Vector3(avatarPosition[currentStage].x, avatarPosition[currentStage].y, avatarPosition[currentStage].z), Quaternion.identity);
            startTracker.GetComponent<TimeTrackingZone>().startTimeTracker = true;
            // Initialize end time tracking area
            GameObject endTracker = Instantiate(Resources.Load("UserStudy/TriggerArea") as GameObject, new Vector3(robotPosition[currentStage].x, robotPosition[currentStage].y, robotPosition[currentStage].z), Quaternion.identity);
            endTracker.GetComponent<TimeTrackingZone>().startTimeTracker = false;
        }
        else
        {
            int maxLevelCount = UserStudyDataManager.getMaxLevelCount();
            if (currentScene == SceneType.MainMenu)
            {
                // Determine which parts of the menu need to be shown depending on the current level
                _firstSurvey = UserStudyDataManager.identifierNeeded();
                if (_firstSurvey)
                {
                    surveyCount.gameObject.SetActive(false);
                }
                else
                {
                    surveyInstruction.SetActive(false);
                    surveyCount.gameObject.SetActive(true);
                    surveyCount.text = "Survey level " + (_currentLevel+1) + " / " + (maxLevelCount+1);

                }
                nextSurveyMethod.text = "Current method: " + UserStudyDataManager.getCurrentcontrolMethodAsString();
            }
            else if (currentScene == SceneType.FinishMenu)
            {
                surveyCount.text = "Completed survey level " + (_currentLevel+1) + " / " + (maxLevelCount+1);
                if (_currentLevel == maxLevelCount)
                {
                    surveyCount.text = surveyCount.text + "\n \n Thank you very much for participating in my survey! \n You really saved me. :)";
                }
            }
        }
        
    }
	
	// Update is called once per frame
	void Update () {

        if (currentScene == SceneType.Survey)
        {
            // Add a  CapsuleCollider to the avatar
            if (_avatarId != "" && _addCollider)
            {
                if (_avatar != null)
                {
                    CapsuleCollider coll = _avatar.AddComponent<CapsuleCollider>();
                    coll.center = new Vector3(0, 1, 0);
                    coll.radius = 0.2f;
                    _addCollider = false;
                }
                else
                {
                    _avatar = GameObject.Find("user_avatar_" + _avatarId);
                }
            }
            else
            {
                _avatarId = GzBridgeManager.Instance.avatarId;
            }

            // Sets the control method in the AvatarMovement sccript
            if (_setControlMethod)
            {
                AvatarMovement movementObject = FindObjectOfType<AvatarMovement>();
                if (movementObject != null)
                {
                    movementObject.contrType = (AvatarMovement.ControlType)(UserStudyDataManager.getCurrentControlMethod());
                    _setControlMethod = false;
                }
            }
        }
        
    }

    public void StartNextSurvey()
    {
        UserStudyDataManager.setIdentifier(identifier.text);
        SceneManager.LoadScene("NRPClient");
    }

}
