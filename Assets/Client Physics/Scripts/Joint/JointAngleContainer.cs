using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class JointAngleContainer
{
    public HumanBodyBones bone;
    public string boneName;
    public float minAngleX, maxAngleX, minAngleY, maxAngleY, minAngleZ, maxAngleZ;
}
