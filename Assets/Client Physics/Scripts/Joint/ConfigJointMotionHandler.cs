using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointMotionHandler : MonoBehaviour
{

    public GameObject target;
    public float angularVelocityScale = 3f;

    public JointDrive xDrive = new JointDrive();
    public JointDrive yDrive = new JointDrive();
    public JointDrive zDrive = new JointDrive();

    public JointDrive angularXDrive = new JointDrive();
    public JointDrive angularYZDrive = new JointDrive();

    ConfigurableJointMotion motion = ConfigurableJointMotion.Limited;

    Quaternion startOrientation;

    Vector3 previousAngularVelocity;

    ConfigurableJoint[] joints;
    Rigidbody rb;

    bool useIndividualAxes;
    AvatarManager avatarManager;

    // Use this for initialization
    void Start()
    {
        joints = GetComponents<ConfigurableJoint>();
        rb = GetComponent<Rigidbody>();

        avatarManager = GameObject.FindGameObjectWithTag("Avatar").GetComponent<AvatarManager>();
        useIndividualAxes = avatarManager.useIndividualAxes;

        xDrive.positionSpring = yDrive.positionSpring = zDrive.positionSpring = 2500;
        xDrive.positionDamper = yDrive.positionDamper = zDrive.positionDamper = 300;
        xDrive.maximumForce = yDrive.maximumForce = zDrive.maximumForce = 10000;

        angularXDrive.positionSpring = angularYZDrive.positionSpring = 3000;
        angularXDrive.positionDamper = angularYZDrive.positionDamper = 300;
        angularXDrive.maximumForce = angularYZDrive.maximumForce = 10000;

        foreach (ConfigurableJoint joint in joints)
        {
            joint.xDrive = xDrive;
            joint.yDrive = yDrive;
            joint.zDrive = zDrive;

            joint.angularXDrive = angularXDrive;
            joint.angularYZDrive = angularYZDrive;
        }

        startOrientation = transform.localRotation;
        previousAngularVelocity = rb.angularVelocity;
        /*
        joint.angularXMotion = motion;
        joint.angularYMotion = motion;
        joint.angularZMotion = motion;
        */
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (ConfigurableJoint joint in joints)
        {
            SetTargetRotation(joint);
            SetTargetAngularVelocity(joint);
        }

        /*
        Matrix4x4 worldToJointSpaceMatrix = Matrix4x4.Rotate(worldToJointSpace);

        Vector3 directionToTarget = target.transform.localPosition - joint.connectedBody.transform.localPosition;

        Matrix4x4 targetToJointTranslation = Matrix4x4.Translate(directionToTarget);
        */
        //Vector3 coordinateOffsetWorldToJoint = worldToJointSpaceMatrix.inverse.MultiplyPoint3x4(transform.localPosition + joint.connectedAnchor);

        /*
         * rotation results in localPosition offset
         * --> set joint.targetPosition to a targetPositon that is relative to joint.connectedBody
         * --> convert target.transform.localPosition into joint space
        */
        /*
        Vector3 localToWorld = transform.TransformPoint(transform.localPosition + joint.connectedAnchor);

        Matrix4x4 transformationMatrix = Matrix4x4.TRS(localToWorld, Quaternion.Inverse(worldToJointSpace), new Vector3(1,1,1));
        */
        //Vector3 jointInWorldCoordinates = transformationMatrix * transform.

        //joint.targetPosition = directionToTarget;//localToWorld - target.transform.position; //worldToJointSpaceMatrix.inverse.MultiplyPoint3x4(target.transform.position);

    }

    void SetTargetRotation(ConfigurableJoint joint)
    {
        //Debug.Log(startOrientation);
        //description of the joint space
        //the x axis of the joint space
        Vector3 jointXAxis = joint.axis.normalized;
        // the y axis of the joint space
        Vector3 jointYAxis = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        //the z axis of the joint space
        Vector3 jointZAxis = Vector3.Cross(jointYAxis, jointXAxis).normalized;
        /*
         * Z axis will be aligned with forward
         * X axis aligned with cross product between forward and upwards
         * Y axis aligned with cross product between Z and X.
         * --> rotates world coordinates to align with joint coordinates
        */
        jointYAxis.x += 0.001f;
        Quaternion worldToJointSpace = Quaternion.LookRotation(jointYAxis, jointZAxis);
        /* 
         * turn joint space to align with world
         * perform rotation in world
         * turn joint back into joint space
        */
        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace) *
                                    Quaternion.Inverse(target.transform.localRotation) *
                                    startOrientation *
                                    worldToJointSpace;

        joint.targetRotation = resultRotation;
    }

    void SetTargetAngularVelocity(ConfigurableJoint joint)
    {
        /*
        Vector3 angularDistance = transform.localRotation.eulerAngles - previousRotation.eulerAngles;
        Vector3 angularVelocity = angularDistance / Time.fixedDeltaTime;
        previousRotation = transform.localRotation;

        joint.targetAngularVelocity = -angularVelocity;
        
        if(Mathf.Sign(rb.angularVelocity.x) != Mathf.Sign(previousAngularVelocity.x) || Mathf.Sign(rb.angularVelocity.y) != Mathf.Sign(previousAngularVelocity.y) || Mathf.Sign(rb.angularVelocity.z) != Mathf.Sign(previousAngularVelocity.z))
        {
            rb.AddTorque(-previousAngularVelocity);
        }
        */
    }
}
