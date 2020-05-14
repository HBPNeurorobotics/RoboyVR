using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class UserAvatarIKControl : MonoBehaviour
{

    [SerializeField] public bool ikActive = true;
    [SerializeField] private TrackingIKTargetManager trackingIKTargetManager;
    [SerializeField] private Vector3 bodyHeadOffset = new Vector3(0, -1.0f, 0);
    [SerializeField] private Vector3 inferredBodyOffset = new Vector3(0, 0, 0);
    [SerializeField] private Pose manualBodyOffset;
    [SerializeField] private Transform nonGazeboZeroPoint;
    [SerializeField] private Transform nonGazeboBodyPoint;


    // Bachelor Thesis VRHand
    [SerializeField] private TrackingFKManager trackingFKManager;

    protected Animator animator;

    private Transform headTarget = null;
    private Transform bodyTarget = null;
    private Transform leftHandTarget = null;
    private Transform rightHandTarget = null;
    private Transform leftFootTarget = null;
    private Transform rightFootTarget = null;

    private Transform lookAtObj = null;

    private Queue<Vector3> groundCenterTrajectory = new Queue<Vector3>();
    private int groundCenterTrajectorySize = 20;
    public float coordStartAnchor;

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
    void Start()
    {
        animator = GetComponent<Animator>();
        if (DetermineController.Instance.UseKnucklesControllers())
        {
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator)
        {
            // initialize
            if (ikActive && trackingIKTargetManager.IsReady() && headTarget == null)
            {
                headTarget = trackingIKTargetManager.GetIKTargetHead();
                lookAtObj = trackingIKTargetManager.GetIKTargetLookAt();
                bodyTarget = trackingIKTargetManager.GetIKTargetBody();
                leftHandTarget = trackingIKTargetManager.GetIKTargetLeftHand();
                rightHandTarget = trackingIKTargetManager.GetIKTargetRightHand();
                leftFootTarget = trackingIKTargetManager.GetIKTargetLeftFoot();
                rightFootTarget = trackingIKTargetManager.GetIKTargetRightFoot();

                UserAvatarService.Instance.SpawnYBot();
            }

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                // position body
                if (bodyTarget != null)
                {
                    this.transform.position = bodyTarget.position + manualBodyOffset.position;
                    this.transform.rotation = new Quaternion(bodyTarget.rotation.x + manualBodyOffset.rotation.x, bodyTarget.rotation.y + manualBodyOffset.rotation.y, bodyTarget.rotation.z + manualBodyOffset.rotation.z, bodyTarget.rotation.w);
                }
                // no body target, but head and feet targets
                else if (headTarget != null && leftFootTarget != null && rightFootTarget != null)
                {
                    float leftFootHeight = leftFootTarget.position.y;
                    float rightFootHeight = rightFootTarget.position.y;

                    // determine ground center of stability
                    Vector3 groundCenter;
                    float thresholdFootOffGround = 1.5f * trackingIKTargetManager.feetTargetOffsetAboveGround;
                    // both feet on the ground
                    if (leftFootHeight < thresholdFootOffGround && rightFootHeight < thresholdFootOffGround)
                    {
                        groundCenter = 0.5f * (leftFootTarget.position + rightFootTarget.position);
                    }
                    // only left foot on the ground
                    else if (leftFootHeight < thresholdFootOffGround)
                    {
                        groundCenter = leftFootTarget.position;
                    }
                    // only right foot on the ground
                    else if (rightFootHeight < thresholdFootOffGround)
                    {
                        groundCenter = rightFootTarget.position;
                    }
                    // both feet in the air
                    else
                    {
                        groundCenter = 0.5f * (leftFootTarget.position + rightFootTarget.position);
                    }

                    // smoooth out trajectory of ground center
                    groundCenterTrajectory.Enqueue(groundCenter);
                    while (groundCenterTrajectory.Count > groundCenterTrajectorySize)
                    {
                        groundCenterTrajectory.Dequeue();
                    }
                    groundCenter = new Vector3();
                    foreach (Vector3 position in groundCenterTrajectory)
                    {
                        groundCenter += position;
                    }
                    groundCenter /= groundCenterTrajectory.Count;
                    Vector3 bodyUp = (headTarget.transform.position - this.transform.position).normalized;

                    Vector3 bodyRight = (headTarget.transform.right + leftFootTarget.transform.right + rightFootTarget.transform.right).normalized;
                    Vector3 bodyForward = Vector3.Cross(bodyRight, bodyUp).normalized;

                    // set body position
                    Vector3 bodyPosition = new Vector3();
                    if (!UserAvatarService.Instance.use_gazebo)
                    {

                        bodyPosition = new Vector3(groundCenter.x, nonGazeboBodyPoint.position.y - nonGazeboZeroPoint.position.y, groundCenter.z);
                        bodyPosition.z -= 0.2f * bodyForward.z;
                    }
                    else
                    {
                        float yCoord = 0.01f * (headTarget.transform.position.y - groundCenter.y);
                        bodyPosition = new Vector3(groundCenter.x, yCoord, groundCenter.z) - 0.1f * bodyForward;
                    }

                    this.transform.position = bodyPosition/* + inferredBodyOffset*/;
                    //Debug.Log("head + feet inferred pos:");
                    //Debug.Log(this.transform.position);

                    // set body rotation
                    Quaternion bodyRotation = Quaternion.LookRotation(bodyForward, bodyUp);
                    this.transform.rotation = bodyRotation;

                }
                // no body target, but head
                else if (headTarget != null)
                {
                    Vector3 inferredPos = headTarget.position + bodyHeadOffset;
                    this.transform.position = UserAvatarService.Instance.use_gazebo ? inferredPos : new Vector3(inferredPos.x, inferredPos.y - coordStartAnchor, inferredPos.z);// + Quaternion.FromToRotation(Vector3.up, interpolatedUpVector) * headToBodyOffset;

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
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);
                    //Quaternion rightQuaternion = Quaternion.Euler(rightFootTarget.eulerAngles);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, /*rightQuaternion*/rightFootTarget.rotation);
                }

                if (leftFootTarget != null)
                {
                    //leftFootTarget.up = Vector3.up;
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position);
                    //Quaternion leftQuaternion = Quaternion.Euler(leftFootTarget.eulerAngles);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, /*leftQuaternion*/leftFootTarget.rotation);
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (trackingIKTargetManager.IsReady() && DetermineController.Instance.UseKnucklesControllers())
        {
            getFingerTargetLeft();
            getFingerTargetRight();

            updateFingerTargetLeft();
            updateFingerTargetRight();
        }

    }

    private void getFingerTargetLeft()
    {
        leftThumb1 = trackingFKManager.GetTargetThumb1();
        leftThumb2 = trackingFKManager.GetTargetThumb2();
        leftThumb3 = trackingFKManager.GetTargetThumb3();
        leftThumb4 = trackingFKManager.GetTargetThumb4();

        leftIndex1 = trackingFKManager.GetTargetIndex1();
        leftIndex2 = trackingFKManager.GetTargetIndex2();
        leftIndex3 = trackingFKManager.GetTargetIndex3();
        leftIndex4 = trackingFKManager.GetTargetIndex4();

        leftMiddle1 = trackingFKManager.GetTargetMiddle1();
        leftMiddle2 = trackingFKManager.GetTargetMiddle2();
        leftMiddle3 = trackingFKManager.GetTargetMiddle3();
        leftMiddle4 = trackingFKManager.GetTargetMiddle4();

        leftRing1 = trackingFKManager.GetTargetRing1();
        leftRing2 = trackingFKManager.GetTargetRing2();
        leftRing3 = trackingFKManager.GetTargetRing3();
        leftRing4 = trackingFKManager.GetTargetRing4();

        leftPinky1 = trackingFKManager.GetTargetPinky1();
        leftPinky2 = trackingFKManager.GetTargetPinky2();
        leftPinky3 = trackingFKManager.GetTargetPinky3();
        leftPinky4 = trackingFKManager.GetTargetPinky4();
    }

    private void getFingerTargetRight()
    {
        rightThumb1 = trackingFKManager.GetTargetThumb1R();
        rightThumb2 = trackingFKManager.GetTargetThumb2R();
        rightThumb3 = trackingFKManager.GetTargetThumb3R();
        rightThumb4 = trackingFKManager.GetTargetThumb4R();

        rightIndex1 = trackingFKManager.GetTargetIndex1R();
        rightIndex2 = trackingFKManager.GetTargetIndex2R();
        rightIndex3 = trackingFKManager.GetTargetIndex3R();
        rightIndex4 = trackingFKManager.GetTargetIndex4R();

        rightMiddle1 = trackingFKManager.GetTargetMiddle1R();
        rightMiddle2 = trackingFKManager.GetTargetMiddle2R();
        rightMiddle3 = trackingFKManager.GetTargetMiddle3R();
        rightMiddle4 = trackingFKManager.GetTargetMiddle4R();

        rightRing1 = trackingFKManager.GetTargetRing1R();
        rightRing2 = trackingFKManager.GetTargetRing2R();
        rightRing3 = trackingFKManager.GetTargetRing3R();
        rightRing4 = trackingFKManager.GetTargetRing4R();

        rightPinky1 = trackingFKManager.GetTargetPinky1R();
        rightPinky2 = trackingFKManager.GetTargetPinky2R();
        rightPinky3 = trackingFKManager.GetTargetPinky3R();
        rightPinky4 = trackingFKManager.GetTargetPinky4R();
    }

    private void updateFingerTargetLeft()
    {
        lModelThumb1.transform.rotation = leftThumb1.rotation;
        lModelThumb2.transform.rotation = leftThumb2.rotation;
        lModelThumb3.transform.rotation = leftThumb3.rotation;
        lModelThumb4.transform.rotation = leftThumb4.rotation;

        lModelIndex1.transform.rotation = leftIndex1.rotation;
        lModelIndex2.transform.rotation = leftIndex2.rotation;
        lModelIndex3.transform.rotation = leftIndex3.rotation;
        lModelIndex4.transform.rotation = leftIndex4.rotation;

        lModelMiddle1.transform.rotation = leftMiddle1.rotation;
        lModelMiddle2.transform.rotation = leftMiddle2.rotation;
        lModelMiddle3.transform.rotation = leftMiddle3.rotation;
        lModelMiddle4.transform.rotation = leftMiddle4.rotation;

        lModelRing1.transform.rotation = leftRing1.rotation;
        lModelRing2.transform.rotation = leftRing2.rotation;
        lModelRing3.transform.rotation = leftRing3.rotation;
        lModelRing4.transform.rotation = leftRing4.rotation;

        lModelPinky1.transform.rotation = leftPinky1.rotation;
        lModelPinky2.transform.rotation = leftPinky2.rotation;
        lModelPinky3.transform.rotation = leftPinky3.rotation;
        lModelPinky4.transform.rotation = leftPinky4.rotation;
    }

    private void updateFingerTargetRight()
    {
        rModelThumb1.transform.rotation = rightThumb1.rotation;
        rModelThumb2.transform.rotation = rightThumb2.rotation;
        rModelThumb3.transform.rotation = rightThumb3.rotation;
        rModelThumb4.transform.rotation = rightThumb4.rotation;

        rModelIndex1.transform.rotation = rightIndex1.rotation;
        rModelIndex2.transform.rotation = rightIndex2.rotation;
        rModelIndex3.transform.rotation = rightIndex3.rotation;
        rModelIndex4.transform.rotation = rightIndex4.rotation;

        rModelMiddle1.transform.rotation = rightMiddle1.rotation;
        rModelMiddle2.transform.rotation = rightMiddle2.rotation;
        rModelMiddle3.transform.rotation = rightMiddle3.rotation;
        rModelMiddle4.transform.rotation = rightMiddle4.rotation;

        rModelRing1.transform.rotation = rightRing1.rotation;
        rModelRing2.transform.rotation = rightRing2.rotation;
        rModelRing3.transform.rotation = rightRing3.rotation;
        rModelRing4.transform.rotation = rightRing4.rotation;

        rModelPinky1.transform.rotation = rightPinky1.rotation;
        rModelPinky2.transform.rotation = rightPinky2.rotation;
        rModelPinky3.transform.rotation = rightPinky3.rotation;
        rModelPinky4.transform.rotation = rightPinky4.rotation;
    }
}
