using UnityEngine;

public class JointSettings : MonoBehaviour
{
    public bool showInEditor = false;
    public bool showAngularXDriveInEditor = false;
    public bool showAngularYZDriveInEditor = false;
    public string bone;
    [Header("Angular Drive X")]
    public float angularXDriveSpring;
    public float angularXDriveDamper;
    [Header("Angular Drive YZ")]
    public float angularYZDriveSpring;
    public float angularYZDriveDamper;

    public JointSettings(string bone, float angularXDriveSpring, float angularXDriveDamper, float angularYZDriveSpring, float angularYZDriveDamper)
    {
        this.bone = bone;
        this.angularXDriveDamper = angularXDriveDamper;
        this.angularXDriveSpring = angularXDriveSpring;
        this.angularYZDriveDamper = angularYZDriveDamper;
        this.angularYZDriveSpring = angularYZDriveSpring;
    }

    public JointSettings()
    {

    }
}
