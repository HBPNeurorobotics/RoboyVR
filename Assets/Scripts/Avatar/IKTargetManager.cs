using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class IKTargetManager : MonoBehaviour {

    [SerializeField]
    private Transform avatarHeadTransform;
    [SerializeField]
    private Transform avatarBodyTransform;
    [SerializeField]
    private Transform avatarLeftHandTransform;
    [SerializeField]
    private Transform avatarRightHandTransform;
    [SerializeField]
    private Transform avatarLeftFootTransform;
    [SerializeField]
    private Transform avatarRightFootTransform;

    [SerializeField]
    private List<SteamVR_TrackedObject> trackedObjects = new List<SteamVR_TrackedObject>();
    
    private GameObject targetHead;
    private GameObject targetBody;
    private GameObject targetLeftHand;
    private GameObject targetRightHand;
    private GameObject targetLeftFoot;
    private GameObject targetRightFoot;

    private Pose targetOffsetLeftHand = new Pose(new Vector3(-0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, 90f) );
    private Pose targetOffsetRightHand = new Pose(new Vector3(0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, -90f));

    // Use this for initialization
    void Start () {
        
        foreach(SteamVR_TrackedObject trackedObject in trackedObjects)
        {
            Debug.Log(trackedObject.index);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region IK_TARGET_GETTERS
    public Transform GetIKTargetHead()
    {
        return targetHead.transform;
    }

    public Transform GetIKTargetBody()
    {
        return targetBody.transform;
    }

    public Transform GetIKTargetLeftHand()
    {
        return targetLeftHand.transform;
    }

    public Transform GetIKTargetRightHand()
    {
        return targetRightHand.transform;
    }

    public Transform GetIKTargetLeftFoot()
    {
        return targetLeftFoot.transform;
    }

    public Transform GetIKTargetRightFoot()
    {
        return targetRightFoot.transform;
    }
    #endregion IK_TARGET_GETTERS
}
