using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Wrapper class to store various settings of a ConfigurableJoint.
/// </summary>
[Serializable]
public class JointAngleContainer
{
    public HumanBodyBones bone;
    public string boneName;
    public float minAngleX, maxAngleX, minAngleY, maxAngleY, minAngleZ, maxAngleZ;
}
