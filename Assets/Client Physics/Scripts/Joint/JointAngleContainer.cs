using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class JointAngleContainer
{
    public HumanBodyBones bone;
    public float minAngleX, minAngleY, minAngleZ;
    public float maxAngleX, maxAngleY, maxAngleZ;
}
