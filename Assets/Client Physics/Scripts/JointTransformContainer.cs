using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointTransformContainer : MonoBehaviour {

    HumanBodyBones bone;
    Transform start;

    public JointTransformContainer(HumanBodyBones bone, Transform start)
    {
        this.bone = bone;
        this.start = start;
    }

    public HumanBodyBones GetBone()
    {
        return bone;
    }

    public Transform GetStart()
    {
        return start;
    }
}
