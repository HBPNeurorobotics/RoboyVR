using UnityEngine;
/// <summary>
/// Can be used to store key values of Unity ConfigurableJoint
/// </summary>
public class JointSettings : MonoBehaviour
{
    public bool showInEditor = false;
    public bool showAngularXDriveInEditor = false;
    public bool showAngularYZDriveInEditor = false;
    public HumanBodyBones bone;

    public float angularXDriveSpring = 2500;
    public float angularXDriveDamper = 600;
    public float maxForceX = 2500;

    public float angularYZDriveSpring = 2500;
    public float angularYZDriveDamper = 600; 
    public float maxForceYZ = 2500;

    public JointDrive angularXDrive;
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
    /// A container for all important ConfigurableJoint parameters for tuning purposes.
    /// </summary>
    /// <param name="bone">The HumanBodyBone of the body part of the joint.</param>
    /// <param name="angularXDrive"></param>
    /// <param name="angularYZDrive"></param>
    public JointSettings(HumanBodyBones bone, JointDrive angularXDrive, JointDrive angularYZDrive)
    {
        this.bone = bone;
        this.angularXDrive = angularXDrive;
        this.angularYZDrive = angularYZDrive;
    }
    public JointSettings(JointSettings copy)
    {
        this.bone = copy.bone;
        this.angularXDrive = copy.angularXDrive;
        this.angularYZDrive = copy.angularYZDrive;
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
