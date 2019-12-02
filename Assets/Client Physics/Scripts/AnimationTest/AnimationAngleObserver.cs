using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAngleObserver : MonoBehaviour
{
    Vector3 localEuler;
    JointAngleContainer jointAngleContainer = new JointAngleContainer();
    public float currentAngleX, currentAngleY, currentAngleZ;

    void Start()
    {
        jointAngleContainer.minAngleX = 0;
        jointAngleContainer.minAngleY = 0;
        jointAngleContainer.minAngleZ = 0;
    }

    void FixedUpdate()
    {
        localEuler = UnityEditor.TransformUtils.GetInspectorRotation(transform);

        currentAngleX = localEuler.x;
        currentAngleY = localEuler.y;
        currentAngleZ = localEuler.z;

        jointAngleContainer.minAngleX = SetMin(jointAngleContainer.minAngleX, currentAngleX);        
        //Debug.Log(jointAngleContainer.minAngleX);
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
        return jointAngleContainer;
    }
}
