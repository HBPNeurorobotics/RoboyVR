using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointMotionHandler : MonoBehaviour
{

    public GameObject target;
    public float angularVelocityScale = 3f;
    bool useRotationFromJointSpace = false;

    public JointDrive xDrive = new JointDrive();
    public JointDrive yDrive = new JointDrive();
    public JointDrive zDrive = new JointDrive();

    public JointDrive angularXDrive = new JointDrive();
    public JointDrive angularYZDrive = new JointDrive();

    ConfigurableJointMotion motion = ConfigurableJointMotion.Limited;

    Quaternion startOrientation;
    Quaternion jointSpaceRotation;

    Quaternion previousOrientation;

    ConfigurableJoint[] joints;
    Rigidbody rb;

    bool useMultipleJointTemplate;
    ConfigJointManager configJointManager;

    // Use this for initialization
    void Start()
    {
        joints = GetComponents<ConfigurableJoint>();
        rb = GetComponent<Rigidbody>();

        configJointManager = GameObject.FindGameObjectWithTag("Avatar").GetComponent<ConfigJointManager>();
        useMultipleJointTemplate = configJointManager.useJointsMultipleTemplate;
        /*
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
        */
        startOrientation = transform.localRotation;
        previousOrientation = startOrientation;
        /*
        joint.angularXMotion = motion;
        joint.angularYMotion = motion;
        joint.angularZMotion = motion;
        */
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        joints = GetComponents<ConfigurableJoint>();
        foreach (ConfigurableJoint joint in joints)
        {
            SetTargetRotation(joint);
            SetTargetAngularVelocity(joint);
        }
    }

    void SetTargetRotation(ConfigurableJoint joint)
    {
        if (useRotationFromJointSpace)
        {
            joint.targetRotation = jointSpaceRotation;
        }
        else
        {
            Quaternion worldToJointSpace = ConfigJointUtility.GetWorldToJointRotation(joint);
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

    /// <summary>
    /// Set whether the joint target rotation should be set to a rotation in joint space instead of imitating a target object.
    /// </summary>
    /// <param name="rotation">The input target rotation in joint space.</param>
    /// <param name="useJointRotation">True: rotation will be directly set. False: uses target.localRotation</param>
    public void UseJointRotation(Quaternion rotation, bool useJointRotation)
    {
        useRotationFromJointSpace = useJointRotation;
        jointSpaceRotation = rotation;
    }

    public Quaternion GetOriginalOrientation()
    {
        return startOrientation;
    }
    public void SetOriginalOrientation(Quaternion originalOrientation)
    {
         this.startOrientation = originalOrientation;
    }

}
