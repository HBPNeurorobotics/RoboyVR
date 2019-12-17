using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class UserAvatarIKControl : MonoBehaviour {

    [SerializeField] private bool ikActive = true;
    [SerializeField] private IKTargetManager ikTargetManager;

    protected Animator animator;

    private Transform headTarget = null;
    private Transform bodyTarget = null;
    private Transform leftHandTarget = null;
    private Transform rightHandTarget = null;
    private Transform leftFootTarget = null;
    private Transform rightFootTarget = null;

    private Transform lookAtObj = null;

    public Vector3 bodyTargetOffset = new Vector3(0, -0.75f, 0);
    public Vector3 bodyHeadOffset = new Vector3(0, -1.0f, 0);
    [SerializeField]Vector3 footLeftOffset = new Vector3(0.08f, 0,0);
    [SerializeField] Vector3 footRightOffset = new Vector3(-0.08f, 0, 0);
    [SerializeField] Vector3 footLeftRotation = new Vector3(0, 90, 0);
    [SerializeField] Vector3 footRightRotation = new Vector3(0, -90, 0);

    // Bachelor Thesis VRHand
    private Transform leftThumb1 = null;
    private Transform leftThumb2 = null;
    private Transform leftThumb3 = null;
    private Transform leftThumb4 = null;
    private Transform rightThumb1 = null;
    private Transform rightThumb2 = null;
    private Transform rightThumb3 = null;
    private Transform rightThumb4 = null;

    private Transform leftIndex1 = null;
    private Transform leftIndex2 = null;
    private Transform leftIndex3 = null;
    private Transform leftIndex4 = null;
    private Transform rightIndex1 = null;
    private Transform rightIndex2 = null;
    private Transform rightIndex3 = null;
    private Transform rightIndex4 = null;

    private Transform leftMiddle1 = null;
    private Transform leftMiddle2 = null;
    private Transform leftMiddle3 = null;
    private Transform leftMiddle4 = null;
    private Transform rightMiddle1 = null;
    private Transform rightMiddle2 = null;
    private Transform rightMiddle3 = null;
    private Transform rightMiddle4 = null;

    private Transform leftRing1 = null;
    private Transform leftRing2 = null;
    private Transform leftRing3 = null;
    private Transform leftRing4 = null;
    private Transform rightRing1 = null;
    private Transform rightRing2 = null;
    private Transform rightRing3 = null;
    private Transform rightRing4 = null;

    private Transform leftPinky1 = null;
    private Transform leftPinky2 = null;
    private Transform leftPinky3 = null;
    private Transform leftPinky4 = null;
    private Transform rightPinky1 = null;
    private Transform rightPinky2 = null;
    private Transform rightPinky3 = null;
    private Transform rightPinky4 = null;

    public GameObject lModelHand;

    private GameObject lModelThumb1;
    private GameObject lModelThumb2;
    private GameObject lModelThumb3;
    private GameObject lModelThumb4;

    private GameObject lModelIndex1;
    private GameObject lModelIndex2;
    private GameObject lModelIndex3;
    private GameObject lModelIndex4;

    private GameObject lModelMiddle1;
    private GameObject lModelMiddle2;
    private GameObject lModelMiddle3;
    private GameObject lModelMiddle4;

    private GameObject lModelRing1;
    private GameObject lModelRing2;
    private GameObject lModelRing3;
    private GameObject lModelRing4;

    private GameObject lModelPinky1;
    private GameObject lModelPinky2;
    private GameObject lModelPinky3;
    private GameObject lModelPinky4;

    private GameObject rModelThumb1;
    private GameObject rModelThumb2;
    private GameObject rModelThumb3;
    private GameObject rModelThumb4;

    private GameObject rModelIndex1;
    private GameObject rModelIndex2;
    private GameObject rModelIndex3;
    private GameObject rModelIndex4;

    private GameObject rModelMiddle1;
    private GameObject rModelMiddle2;
    private GameObject rModelMiddle3;
    private GameObject rModelMiddle4;

    private GameObject rModelRing1;
    private GameObject rModelRing2;
    private GameObject rModelRing3;
    private GameObject rModelRing4;

    private GameObject rModelPinky1;
    private GameObject rModelPinky2;
    private GameObject rModelPinky3;
    private GameObject rModelPinky4;
    

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();

        rModelThumb1 = GameObject.Find("mixamorig_RightHandThumb1");
        rModelThumb2 = GameObject.Find("mixamorig_RightHandThumb2");
        rModelThumb3 = GameObject.Find("mixamorig_RightHandThumb3");
        rModelThumb4 = GameObject.Find("mixamorig_RightHandThumb4");
        lModelThumb1 = GameObject.Find("mixamorig_LeftHandThumb1");
        lModelThumb2 = GameObject.Find("mixamorig_LeftHandThumb2");
        lModelThumb3 = GameObject.Find("mixamorig_LeftHandThumb3");
        lModelThumb4 = GameObject.Find("mixamorig_LeftHandThumb4");

        rModelIndex1 = GameObject.Find("mixamorig_RightHandIndex1");
        rModelIndex2 = GameObject.Find("mixamorig_RightHandIndex2");
        rModelIndex3 = GameObject.Find("mixamorig_RightHandIndex3");
        rModelIndex4 = GameObject.Find("mixamorig_RightHandIndex4");
        lModelIndex1 = GameObject.Find("mixamorig_LeftHandIndex1");
        lModelIndex2 = GameObject.Find("mixamorig_LeftHandIndex2");
        lModelIndex3 = GameObject.Find("mixamorig_LeftHandIndex3");
        lModelIndex4 = GameObject.Find("mixamorig_LeftHandIndex4");

        rModelMiddle1 = GameObject.Find("mixamorig_RightHandMiddle1");
        rModelMiddle2 = GameObject.Find("mixamorig_RightHandMiddle2");
        rModelMiddle3 = GameObject.Find("mixamorig_RightHandMiddle3");
        rModelMiddle4 = GameObject.Find("mixamorig_RightHandMiddle4");
        lModelMiddle1 = GameObject.Find("mixamorig_LeftHandMiddle1");
        lModelMiddle2 = GameObject.Find("mixamorig_LeftHandMiddle2");
        lModelMiddle3 = GameObject.Find("mixamorig_LeftHandMiddle3");
        lModelMiddle4 = GameObject.Find("mixamorig_LeftHandMiddle4");

        rModelRing1 = GameObject.Find("mixamorig_RightHandRing1");
        rModelRing2 = GameObject.Find("mixamorig_RightHandRing2");
        rModelRing3 = GameObject.Find("mixamorig_RightHandRing3");
        rModelRing4 = GameObject.Find("mixamorig_RightHandRing4");
        lModelRing1 = GameObject.Find("mixamorig_LeftHandRing1");
        lModelRing2 = GameObject.Find("mixamorig_LeftHandRing2");
        lModelRing3 = GameObject.Find("mixamorig_LeftHandRing3");
        lModelRing4 = GameObject.Find("mixamorig_LeftHandRing4");

        rModelPinky1 = GameObject.Find("mixamorig_RightHandPinky1");
        rModelPinky2 = GameObject.Find("mixamorig_RightHandPinky2");
        rModelPinky3 = GameObject.Find("mixamorig_RightHandPinky3");
        rModelPinky4 = GameObject.Find("mixamorig_RightHandPinky4");
        lModelPinky1 = GameObject.Find("mixamorig_LeftHandPinky1");
        lModelPinky2 = GameObject.Find("mixamorig_LeftHandPinky2");
        lModelPinky3 = GameObject.Find("mixamorig_LeftHandPinky3");
        lModelPinky4 = GameObject.Find("mixamorig_LeftHandPinky4");
    }
	
	// Update is called once per frame
	void Update () {
    
	}

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator && ikTargetManager.IsReady())
        {
            if (headTarget == null)
            {
                headTarget = ikTargetManager.GetIKTargetHead();
                lookAtObj = ikTargetManager.GetIKTargetLookAt();
                bodyTarget = ikTargetManager.GetIKTargetBody();
                leftHandTarget = ikTargetManager.GetIKTargetLeftHand();
                rightHandTarget = ikTargetManager.GetIKTargetRightHand();
                leftFootTarget = ikTargetManager.GetIKTargetLeftFoot();
                rightFootTarget = ikTargetManager.GetIKTargetRightFoot();
                

                UserAvatarService.Instance.SpawnYBot();
            }

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                // position body
                if (bodyTarget != null)
                {
                    this.transform.position = bodyTarget.position + bodyTargetOffset;
                    this.transform.rotation = bodyTarget.rotation;
                }
                // no body target, but head and feet targets
                else if (headTarget != null && rightFootTarget != null && leftFootTarget != null)
                {
                    Vector3 feetCenter = 0.33f * (rightFootTarget.position + leftFootTarget.position + headTarget.position);
                    this.transform.position = new Vector3(feetCenter.x, headTarget.position.y + bodyHeadOffset.y, feetCenter.z);

                    Vector3 forward;
                    if (rightHandTarget != null && leftHandTarget != null)
                    {
                        Vector3 vec_controllers = rightHandTarget.position - leftHandTarget.position;
                        forward = Vector3.ProjectOnPlane(headTarget.forward, vec_controllers);
                    }
                    else
                    {
                        forward = headTarget.forward;
                    }
                    this.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, Vector3.up), Vector3.up);
                }
                // no body target, but head
                else if (headTarget != null)
                {
                    this.transform.position = headTarget.position + bodyHeadOffset; // + Quaternion.FromToRotation(Vector3.up, interpolatedUpVector) * headToBodyOffset;

                    Vector3 forward;
                    if (rightHandTarget != null && leftHandTarget != null)
                    {
                        Vector3 vec_controllers = rightHandTarget.position - leftHandTarget.position;
                        forward = Vector3.ProjectOnPlane(headTarget.forward, vec_controllers);
                    }
                    else
                    {
                        forward = headTarget.forward;
                    }
                    this.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, Vector3.up), Vector3.up);
                }

                if (lookAtObj != null)
                {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(lookAtObj.position);
                }

                if (rightHandTarget != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
                }

                if (leftHandTarget != null)
                {

                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                }
                
                if (rightFootTarget != null)
                {
                    //rightFootTarget.up = Vector3.up;
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position + footRightOffset);
                    Quaternion rightQuaternion = Quaternion.Euler(rightFootTarget.eulerAngles + footRightRotation);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, rightQuaternion);
                }
                
                if (leftFootTarget != null)
                {
                    //leftFootTarget.up = Vector3.up;
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position + footLeftOffset);
                    Quaternion leftQuaternion = Quaternion.Euler(leftFootTarget.eulerAngles + footLeftRotation);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftQuaternion);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (ikTargetManager.IsReady())
        {
            getFingerTargetLeft();
            getFingerTargetRight();

            updateFingerTargetLeft();
            updateFingerTargetRight();
        }
        
    }

    private void getFingerTargetLeft()
    {
        leftThumb1 = ikTargetManager.GetTargetThumb1();
        leftThumb2 = ikTargetManager.GetTargetThumb2();
        leftThumb3 = ikTargetManager.GetTargetThumb3();
        leftThumb4 = ikTargetManager.GetTargetThumb4();

        leftIndex1 = ikTargetManager.GetTargetIndex1();
        leftIndex2 = ikTargetManager.GetTargetIndex2();
        leftIndex3 = ikTargetManager.GetTargetIndex3();
        leftIndex4 = ikTargetManager.GetTargetIndex4();

        leftMiddle1 = ikTargetManager.GetTargetMiddle1();
        leftMiddle2 = ikTargetManager.GetTargetMiddle2();
        leftMiddle3 = ikTargetManager.GetTargetMiddle3();
        leftMiddle4 = ikTargetManager.GetTargetMiddle4();

        leftRing1 = ikTargetManager.GetTargetRing1();
        leftRing2 = ikTargetManager.GetTargetRing2();
        leftRing3 = ikTargetManager.GetTargetRing3();
        leftRing4 = ikTargetManager.GetTargetRing4();

        leftPinky1 = ikTargetManager.GetTargetPinky1();
        leftPinky2 = ikTargetManager.GetTargetPinky2();
        leftPinky3 = ikTargetManager.GetTargetPinky3();
        leftPinky4 = ikTargetManager.GetTargetPinky4();
    }

    private void getFingerTargetRight()
    {
        rightThumb1 = ikTargetManager.GetTargetThumb1R();
        rightThumb2 = ikTargetManager.GetTargetThumb2R();
        rightThumb3 = ikTargetManager.GetTargetThumb3R();
        rightThumb4 = ikTargetManager.GetTargetThumb4R();

        rightIndex1 = ikTargetManager.GetTargetIndex1R();
        rightIndex2 = ikTargetManager.GetTargetIndex2R();
        rightIndex3 = ikTargetManager.GetTargetIndex3R();
        rightIndex4 = ikTargetManager.GetTargetIndex4R();

        rightMiddle1 = ikTargetManager.GetTargetMiddle1R();
        rightMiddle2 = ikTargetManager.GetTargetMiddle2R();
        rightMiddle3 = ikTargetManager.GetTargetMiddle3R();
        rightMiddle4 = ikTargetManager.GetTargetMiddle4R();

        rightRing1 = ikTargetManager.GetTargetRing1R();
        rightRing2 = ikTargetManager.GetTargetRing2R();
        rightRing3 = ikTargetManager.GetTargetRing3R();
        rightRing4 = ikTargetManager.GetTargetRing4R();

        rightPinky1 = ikTargetManager.GetTargetPinky1R();
        rightPinky2 = ikTargetManager.GetTargetPinky2R();
        rightPinky3 = ikTargetManager.GetTargetPinky3R();
        rightPinky4 = ikTargetManager.GetTargetPinky4R();
    }

    private void updateFingerTargetLeft()
    {
        //Vector3 rot = leftThumb1.rotation.eulerAngles;
        //rot = new Vector3(rot.x - 20, rot.y - 40, rot.z - 50);
        lModelThumb1.transform.SetPositionAndRotation(leftThumb1.position, leftThumb1.rotation);
        lModelThumb2.transform.SetPositionAndRotation(leftThumb2.position, leftThumb2.rotation);
        lModelThumb3.transform.SetPositionAndRotation(leftThumb3.position, leftThumb3.rotation);
        lModelThumb4.transform.SetPositionAndRotation(leftThumb4.position, leftThumb4.rotation);

        lModelIndex1.transform.SetPositionAndRotation(leftIndex1.position, leftIndex1.rotation);
        lModelIndex2.transform.SetPositionAndRotation(leftIndex2.position, leftIndex2.rotation);
        lModelIndex3.transform.SetPositionAndRotation(leftIndex3.position, leftIndex3.rotation);
        lModelIndex4.transform.SetPositionAndRotation(leftIndex4.position, leftIndex4.rotation);

        lModelMiddle1.transform.SetPositionAndRotation(leftMiddle1.position, leftMiddle1.rotation);
        lModelMiddle2.transform.SetPositionAndRotation(leftMiddle2.position, leftMiddle2.rotation);
        lModelMiddle3.transform.SetPositionAndRotation(leftMiddle3.position, leftMiddle3.rotation);
        lModelMiddle4.transform.SetPositionAndRotation(leftMiddle4.position, leftMiddle4.rotation);

        lModelRing1.transform.SetPositionAndRotation(leftRing1.position, leftRing1.rotation);
        lModelRing2.transform.SetPositionAndRotation(leftRing2.position, leftRing2.rotation);
        lModelRing3.transform.SetPositionAndRotation(leftRing3.position, leftRing3.rotation);
        lModelRing4.transform.SetPositionAndRotation(leftRing4.position, leftRing4.rotation);

        lModelPinky1.transform.SetPositionAndRotation(leftPinky1.position, leftPinky1.rotation);
        lModelPinky2.transform.SetPositionAndRotation(leftPinky2.position, leftPinky2.rotation);
        lModelPinky3.transform.SetPositionAndRotation(leftPinky3.position, leftPinky3.rotation);
        lModelPinky4.transform.SetPositionAndRotation(leftPinky4.position, leftPinky4.rotation);
    }

    private void updateFingerTargetRight()
    {
        rModelThumb1.transform.SetPositionAndRotation(rightThumb1.position, rightThumb1.rotation);
        rModelThumb2.transform.SetPositionAndRotation(rightThumb2.position, rightThumb2.rotation);
        rModelThumb3.transform.SetPositionAndRotation(rightThumb3.position, rightThumb3.rotation);
        rModelThumb4.transform.SetPositionAndRotation(rightThumb4.position, rightThumb4.rotation);

        rModelIndex1.transform.SetPositionAndRotation(rightIndex1.position, rightIndex1.rotation);
        rModelIndex2.transform.SetPositionAndRotation(rightIndex2.position, rightIndex2.rotation);
        rModelIndex3.transform.SetPositionAndRotation(rightIndex3.position, rightIndex3.rotation);
        rModelIndex4.transform.SetPositionAndRotation(rightIndex4.position, rightIndex4.rotation);

        rModelMiddle1.transform.SetPositionAndRotation(rightMiddle1.position, rightMiddle1.rotation);
        rModelMiddle2.transform.SetPositionAndRotation(rightMiddle2.position, rightMiddle2.rotation);
        rModelMiddle3.transform.SetPositionAndRotation(rightMiddle3.position, rightMiddle3.rotation);
        rModelMiddle4.transform.SetPositionAndRotation(rightMiddle4.position, rightMiddle4.rotation);

        rModelRing1.transform.SetPositionAndRotation(rightRing1.position, rightRing1.rotation);
        rModelRing2.transform.SetPositionAndRotation(rightRing2.position, rightRing2.rotation);
        rModelRing3.transform.SetPositionAndRotation(rightRing3.position, rightRing3.rotation);
        rModelRing4.transform.SetPositionAndRotation(rightRing4.position, rightRing4.rotation);

        rModelPinky1.transform.SetPositionAndRotation(rightPinky1.position, rightPinky1.rotation);
        rModelPinky2.transform.SetPositionAndRotation(rightPinky2.position, rightPinky2.rotation);
        rModelPinky3.transform.SetPositionAndRotation(rightPinky3.position, rightPinky3.rotation);
        rModelPinky4.transform.SetPositionAndRotation(rightPinky4.position, rightPinky4.rotation);
    }
}
