using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointTest : MonoBehaviour
{

    public GameObject target;

    public JointDrive xDrive = new JointDrive();
    public JointDrive yDrive = new JointDrive();
    public JointDrive zDrive = new JointDrive();

    public JointDrive angularXDrive = new JointDrive();
    public JointDrive angularYZDrive = new JointDrive();

    Quaternion startOrientation;
    Vector3 startPosition;
    // Use this for initialization

    void Start()
    {
        xDrive.positionSpring = yDrive.positionSpring = zDrive.positionSpring = 2500;
        xDrive.positionDamper = yDrive.positionDamper = zDrive.positionDamper = 300;
        xDrive.maximumForce = yDrive.maximumForce = zDrive.maximumForce = 10000;

        angularXDrive.positionSpring = angularYZDrive.positionSpring = 20;
        angularXDrive.positionDamper = angularYZDrive.positionDamper = 10;
        angularXDrive.maximumForce = angularYZDrive.maximumForce = 10000;

        GetComponent<ConfigurableJoint>().xDrive = xDrive;
        GetComponent<ConfigurableJoint>().yDrive = yDrive;
        GetComponent<ConfigurableJoint>().zDrive = zDrive;

        GetComponent<ConfigurableJoint>().angularXDrive = angularXDrive;
        GetComponent<ConfigurableJoint>().angularYZDrive = angularYZDrive;

        startOrientation = transform.rotation;
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 right = GetComponent<ConfigurableJoint>().axis.normalized;
        Vector3 forward = Vector3.Cross(GetComponent<ConfigurableJoint>().axis, GetComponent<ConfigurableJoint>().secondaryAxis).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;
        Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

        // Transform into world space

        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

        resultRotation *= startOrientation * Quaternion.Inverse(target.transform.rotation);

        resultRotation *= worldToJointSpace;


        //Debug.Log((Quaternion.Inverse(target.transform.rotation) * startOrientation).eulerAngles);
        GetComponent<ConfigurableJoint>().targetRotation = resultRotation;

    }

}
