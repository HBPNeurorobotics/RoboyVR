using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    #endregion

    #region PUBLIC_MEMBER_VARIABLES

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

    #endregion

    // Use this for initialization
    void Start () {
        
        // Set desired avatar position
        FindObjectOfType<NRPBackendManager>().avatarPosition = avatarPosition[currentStage];

        // Initialize start time tracking area
        GameObject startTracker = Instantiate(Resources.Load("UserStudy/TriggerArea") as GameObject, new Vector3(avatarPosition[currentStage].x, avatarPosition[currentStage].y, avatarPosition[currentStage].z), Quaternion.identity);
        startTracker.GetComponent<TimeTrackingZone>().startTimeTracker = true;
        // Initialize end time tracking area
        GameObject endTracker = Instantiate(Resources.Load("UserStudy/TriggerArea") as GameObject, new Vector3(robotPosition[currentStage].x, robotPosition[currentStage].y, robotPosition[currentStage].z), Quaternion.identity);
        endTracker.GetComponent<TimeTrackingZone>().startTimeTracker = false;

    }
	
	// Update is called once per frame
	void Update () {

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
    }
}
