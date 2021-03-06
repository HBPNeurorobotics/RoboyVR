﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class UserAvatarIKControl : MonoBehaviour
{
    [SerializeField] public bool ikActive = true;
    [SerializeField] private TrackingIKTargetManager trackingIKTargetManager;
    [SerializeField] private TrackingHandManager trackingHandManager;
    [SerializeField] private Vector3 bodyHeadOffset = new Vector3(0, -1.0f, 0);
    [SerializeField] private Vector3 inferredBodyOffset = new Vector3(0, 0, 0);
    [SerializeField] private Pose manualBodyOffset;
    [SerializeField] private Transform nonGazeboZeroPoint;
    [SerializeField] private Transform nonGazeboBodyPoint;

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

    //public GameObject lModelHand;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
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
                headTarget = trackingIKTargetManager.GetIKTargetTransform(TrackingIKTargetManager.BODY_PART.HEAD);
                lookAtObj = trackingIKTargetManager.GetIKTargetTransform(TrackingIKTargetManager.BODY_PART.VIEWING_DIRECTION);
                bodyTarget = trackingIKTargetManager.GetIKTargetTransform(TrackingIKTargetManager.BODY_PART.HIP);
                leftHandTarget = trackingIKTargetManager.GetIKTargetTransform(TrackingIKTargetManager.BODY_PART.HAND_LEFT);
                rightHandTarget = trackingIKTargetManager.GetIKTargetTransform(TrackingIKTargetManager.BODY_PART.HAND_RIGHT);
                leftFootTarget = trackingIKTargetManager.GetIKTargetTransform(TrackingIKTargetManager.BODY_PART.FOOT_LEFT);
                rightFootTarget = trackingIKTargetManager.GetIKTargetTransform(TrackingIKTargetManager.BODY_PART.FOOT_RIGHT);

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
            UpdateFingerTargets();
        }
    }

    private void UpdateFingerTargets()
    {
        // enum HumanBodyBones lowest finger index is LeftThumbProximal, highest finger index is RightLittleDistal
        for (int i = (int)HumanBodyBones.LeftThumbProximal; i <= (int)HumanBodyBones.RightLittleDistal; i++)
        {
            HumanBodyBones bone = (HumanBodyBones)i;
            this.animator.GetBoneTransform(bone).rotation = trackingHandManager.GetRemotePoseTarget(bone).rotation;
        }
    }
}
