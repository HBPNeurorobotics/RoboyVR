using UnityEngine;
using System;
/// <summary>
/// Can be used to store key values of Unity ConfigurableJoint
/// </summary>

[Serializable]
public class JointSettings
{
    public HumanBodyBones bone;

    public bool showInEditor = false;
    public bool showAngularXDriveInEditor = false;
    public bool showAngularYZDriveInEditor = false;
    public bool showAngularLimitsInEditor = false;

    public float angularXDriveSpring = 2500;
    public float angularXDriveDamper = 600;
    public float maxForceX = 2500;

    public float angularYZDriveSpring = 2500;
    public float angularYZDriveDamper = 600; 
    public float maxForceYZ = 2500;

    public float angularLimitLowX;
    public float angularLimitHighX;
    public float angularLimitY;
    public float angularLimitZ;

    public Vector3 primaryAxis;
    public Vector3 secondaryAxis;

    //Rigidbody
    public bool gravity = true;
    public float mass = 0;
    public Vector3 centerOfMass = Vector3.zero;
    public Vector3 inertiaTensor = Vector3.one;

    [NonSerialized]
    public JointDrive angularXDrive;
    [NonSerialized]
    public JointDrive angularYZDrive;

    public JointSettings(HumanBodyBones bone, float angularXDriveSpring, float angularXDriveDamper, float maxForceX, float angularYZDriveSpring, float angularYZDriveDamper, float maxForceYZ)
    {
        this.bone = bone;
        this.angularXDriveDamper = angularXDriveDamper;
        this.angularXDriveSpring = angularXDriveSpring;
        this.maxForceX = maxForceX;
        this.angularYZDriveDamper = angularYZDriveDamper;
        this.angularYZDriveSpring = angularYZDriveSpring;
        this.maxForceYZ = maxForceYZ;
    }

    /// <summary>
    /// A container for the most important ConfigurableJoint and Rigidbody parameters.
    /// </summary>
    /// <param name="bone">The HumanBodyBone of the body part of the joint.</param>
    /// <param name="angularXDrive"></param>
    /// <param name="angularYZDrive"></param>
    public JointSettings(HumanBodyBones bone, ConfigurableJoint joint)//JointDrive angularXDrive, JointDrive angularYZDrive, float angularLimitLowX, float angularLimitHighX, float angularLimitY, float angularLimitZ)
    {
        this.bone = bone;

        angularXDrive = joint.angularXDrive;
        angularXDrive.positionDamper = joint.angularXDrive.positionDamper;
        angularXDrive.positionSpring = joint.angularXDrive.positionSpring;
        angularXDrive.maximumForce = joint.angularXDrive.maximumForce;

        angularYZDrive = joint.angularYZDrive;
        angularYZDrive.positionDamper = joint.angularYZDrive.positionDamper;
        angularYZDrive.positionSpring = joint.angularYZDrive.positionSpring;
        angularYZDrive.maximumForce = joint.angularYZDrive.maximumForce;

        angularLimitLowX = joint.lowAngularXLimit.limit;
        angularLimitHighX = joint.highAngularXLimit.limit;
        angularLimitY = joint.angularYLimit.limit;
        angularLimitZ = joint.angularZLimit.limit;

        primaryAxis = joint.axis;
        secondaryAxis = joint.secondaryAxis;

        Rigidbody rb = joint.gameObject.GetComponent<Rigidbody>();
        if(rb != null)
        {
            gravity = rb.useGravity;
            mass = rb.mass;
            centerOfMass = rb.centerOfMass;
            inertiaTensor = rb.inertiaTensor;
        }
       
    }
    public JointSettings(JointSettings copy)
    {
        bone = copy.bone;
        angularXDrive = copy.angularXDrive;
        angularYZDrive = copy.angularYZDrive;
    }

    public JointSettings()
    {

    }

    public JointSettings(float angularXDriveSpring, float angularXDriveDamper, float angularYZDriveSpring, float angularYZDriveDamper)
    {
        this.angularXDriveDamper = angularXDriveDamper;
        this.angularXDriveSpring = angularXDriveSpring;
        this.angularYZDriveDamper = angularYZDriveDamper;
        this.angularYZDriveSpring = angularYZDriveSpring;
    }

    public void SetAngularXDriveFromPD(float p, float d)
    {
        angularXDriveSpring = p;
        angularXDriveDamper = d;
    }

    public void SetAngularXDrive(JointDrive drive)
    {
        angularXDrive = drive;
        angularXDriveSpring = drive.positionSpring;
        angularXDriveDamper = drive.positionDamper;
        maxForceX = drive.maximumForce;
    }

    public void SetAngularYZDriveFromPD(float p, float d)
    {
        angularYZDriveSpring = p;
        angularYZDriveDamper = d;
    }

    public void SetAngularYZDrive(JointDrive drive)
    {
        angularYZDrive = drive;
        angularYZDriveSpring = drive.positionSpring;
        angularYZDriveDamper = drive.positionDamper;
        maxForceYZ = drive.maximumForce;
    }
}
