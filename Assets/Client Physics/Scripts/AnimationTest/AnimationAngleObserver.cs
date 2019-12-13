using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Assign to a body whose orientation should be observed. Stores minimum and maximum angles.
/// </summary>
public class AnimationAngleObserver : MonoBehaviour
{
    Vector3 localEuler;
    JointAngleContainer jointAngleContainer = new JointAngleContainer();
    public float currentAngleX, currentAngleY, currentAngleZ;
    Quaternion currentOrientation;
    Quaternion startOrientation;
    Quaternion yxzOrientation;

    void Start()
    {
        jointAngleContainer.minAngleX = 0;
        jointAngleContainer.minAngleY = 0;
        jointAngleContainer.minAngleZ = 0;

        startOrientation = transform.localRotation;
    }

    void FixedUpdate()
    {
        //TODO: THIS RETURNS GIMBAL LOCK ERROR -> QUATERNION? -> store min rotation and max rotation in quats
        //euler Rotation order in Unity: Z -> X -> Y
        //quaternion Rotation order in Unity: Y -> X -> Z
        localEuler = UnityEditor.TransformUtils.GetInspectorRotation(transform);

        Vector3 forward = transform.localRotation * Vector3.forward;
        
        currentAngleX = Mathf.Atan2(forward.z, forward.y) * Mathf.Rad2Deg;
        currentAngleY = Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
        currentAngleZ = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

        //Debug.DrawLine(transform.position, Vector3.up);
        /*
        currentAngleX = localEuler.x;
        currentAngleY = localEuler.y;
        currentAngleZ = localEuler.z;
        */
        jointAngleContainer.minAngleX = SetMin(jointAngleContainer.minAngleX, currentAngleX);        
        jointAngleContainer.minAngleY = SetMin(jointAngleContainer.minAngleY, currentAngleY);
        jointAngleContainer.minAngleZ = SetMin(jointAngleContainer.minAngleZ, currentAngleZ);

        jointAngleContainer.maxAngleX = SetMax(jointAngleContainer.maxAngleX, currentAngleX);
        jointAngleContainer.maxAngleY = SetMax(jointAngleContainer.maxAngleY, currentAngleY);
        jointAngleContainer.maxAngleZ = SetMax(jointAngleContainer.maxAngleZ, currentAngleZ);
    }

    float SetMin(float oldMin, float current)
    {
        return oldMin > current ? current: oldMin;
    }

    float SetMax(float oldMax, float current)
    {
        return oldMax < current ? current : oldMax;
    }
    public JointAngleContainer GetJointAngleContainer(HumanBodyBones bone)
    {
        jointAngleContainer.bone = bone;
        jointAngleContainer.boneName = Enum.GetName(typeof(HumanBodyBones), bone);
        jointAngleContainer.minAngleX = (float)Math.Round(jointAngleContainer.minAngleX, 2, MidpointRounding.AwayFromZero);
        jointAngleContainer.minAngleY = (float)Math.Round(jointAngleContainer.minAngleY, 2, MidpointRounding.AwayFromZero);
        jointAngleContainer.minAngleZ = (float)Math.Round(jointAngleContainer.minAngleZ, 2, MidpointRounding.AwayFromZero);

        jointAngleContainer.maxAngleX = (float)Math.Round(jointAngleContainer.maxAngleX, 2, MidpointRounding.AwayFromZero);
        jointAngleContainer.maxAngleY = (float)Math.Round(jointAngleContainer.maxAngleY, 2, MidpointRounding.AwayFromZero);
        jointAngleContainer.maxAngleZ = (float)Math.Round(jointAngleContainer.maxAngleZ, 2, MidpointRounding.AwayFromZero);
        return jointAngleContainer;
    }
}
