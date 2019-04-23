using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class UserAvatarVisualsIKControl : MonoBehaviour {

    protected Animator animator;
    public bool ikActive = true;

    public Transform headTarget = null;
    public Transform bodyTarget = null;
    public Transform leftHandTarget = null;
    public Transform rightHandTarget = null;
    public Transform leftFootTarget = null;
    public Transform rightFootTarget = null;

    public Transform lookAtObj = null;

    public Vector3 bodyTargetOffset = new Vector3(0, -0.75f, 0);
    public Vector3 bodyHeadOffset = new Vector3(0, -1.0f, 0);
    [SerializeField]Vector3 footLeftOffset = new Vector3(0.08f, 0,0);
    [SerializeField] Vector3 footRightOffset = new Vector3(-0.08f, 0, 0);
    [SerializeField] Vector3 footLeftRotation = new Vector3(0, 90, 0);
    [SerializeField] Vector3 footRightRotation = new Vector3(0, -90, 0);


    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator)
        {

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                // position body
                if (bodyTarget != null)
                {
                    this.transform.position = bodyTarget.position + bodyTargetOffset;
                    this.transform.rotation = bodyTarget.rotation;
                }
                else if (headTarget != null)
                {
                    this.transform.position = headTarget.position + bodyHeadOffset;
                    this.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(headTarget.forward, Vector3.up), Vector3.up);
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
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position + footRightOffset);
                    Quaternion rightQuaternion = Quaternion.Euler(rightFootTarget.eulerAngles + footRightRotation);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, rightQuaternion);
                }
                
                if (leftFootTarget != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position + footLeftOffset);
                    Quaternion leftQuaternion = Quaternion.Euler(leftFootTarget.eulerAngles + footLeftRotation);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftQuaternion);
                }
            }
        }
    }
}
