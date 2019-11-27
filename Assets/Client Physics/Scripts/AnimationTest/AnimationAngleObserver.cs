using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAngleObserver : MonoBehaviour
{
    Vector3 localEuler;
    public float minAngleX, minAngleY, minAngleZ;
    public float currentAngleX, currentAngleY, currentAngleZ;
    public float maxAngleX, maxAngleY, maxAngleZ;
    void Start()
    {
        minAngleX = 0;
        minAngleY = 0;
        minAngleZ = 0;
    }

    void FixedUpdate()
    {
        localEuler = UnityEditor.TransformUtils.GetInspectorRotation(transform);

        currentAngleX = localEuler.x;
        currentAngleY = localEuler.y;
        currentAngleZ = localEuler.z;

        minAngleX = SetMin(minAngleX, currentAngleX);
        minAngleY = SetMin(minAngleY, currentAngleY);
        minAngleZ = SetMin(minAngleZ, currentAngleZ);

        maxAngleX = SetMax(maxAngleX, currentAngleX);
        maxAngleY = SetMax(maxAngleY, currentAngleY);
        maxAngleZ = SetMax(maxAngleZ, currentAngleZ);

    }

    float SetMin(float oldMin, float current)
    { 
        return oldMin > current ? current : oldMin;
    }

    float SetMax(float oldMax, float current)
    {
        return oldMax < current ? current : oldMax;
    }
}
