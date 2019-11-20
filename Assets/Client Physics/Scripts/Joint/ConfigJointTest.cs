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

    ConfigurableJoint joint; 

    // Use this for initialization
    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        xDrive.positionSpring = yDrive.positionSpring = zDrive.positionSpring = 2500;
        xDrive.positionDamper = yDrive.positionDamper = zDrive.positionDamper = 300;
        xDrive.maximumForce = yDrive.maximumForce = zDrive.maximumForce = 10000;

        angularXDrive.positionSpring = angularYZDrive.positionSpring = 2500;
        angularXDrive.positionDamper = angularYZDrive.positionDamper = 500;
        angularXDrive.maximumForce = angularYZDrive.maximumForce = 10000;

        joint.xDrive = xDrive;
        joint.yDrive = yDrive;
        joint.zDrive = zDrive;

        joint.angularXDrive = angularXDrive;
        joint.angularYZDrive = angularYZDrive;

        startOrientation = target.transform.localRotation;//transform.localRotation;

        
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        
    }

    // Update is called once per frame
    void Update()
    {
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
        Quaternion worldToJointSpace = Quaternion.LookRotation(jointYAxis, jointZAxis);
        /* turn joint space to align with world
         * perform rotation in world
         * turn joint back into joint space
        */
        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace) *
                                    Quaternion.Inverse(target.transform.localRotation) *
                                    startOrientation *
                                    worldToJointSpace;

        joint.targetRotation = resultRotation;

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

}
