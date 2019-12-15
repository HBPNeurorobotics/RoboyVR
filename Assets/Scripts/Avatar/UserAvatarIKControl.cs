using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class UserAvatarIKControl : MonoBehaviour
{

    [SerializeField] public bool ikActive = true;
    [SerializeField] private TrackingIKTargetManager trackingIKTargetManager;

    protected Animator animator;

    private Transform headTarget = null;
    private Transform bodyTarget = null;
    private Transform leftHandTarget = null;
    private Transform rightHandTarget = null;
    private Transform leftFootTarget = null;
    private Transform rightFootTarget = null;

    private Transform lookAtObj = null;
    
    public Vector3 bodyHeadOffset = new Vector3(0, -1.0f, 0);


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
                    this.transform.position = bodyTarget.position;
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
}