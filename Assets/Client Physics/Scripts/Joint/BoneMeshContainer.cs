using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneMeshContainer : MonoBehaviour {

    public List<Mesh> Hips;
    public List<Mesh> Spine;
    public List<Mesh> Ribcage;
    public List<Mesh> Head;

    public List<Mesh> LeftShoulder;
    public List<Mesh> LeftArm;
    public List<Mesh> LeftForearm;
    public List<Mesh> LeftHand;
    public List<Mesh> LeftThumb1;
    public List<Mesh> LeftThumb2;
    public List<Mesh> LeftThumb3;    
    public List<Mesh> LeftIndex1;
    public List<Mesh> LeftIndex2;
    public List<Mesh> LeftIndex3;    
    public List<Mesh> LeftMiddle1;
    public List<Mesh> LeftMiddle2;
    public List<Mesh> LeftMiddle3;   
    public List<Mesh> LeftRing1;
    public List<Mesh> LeftRing2;
    public List<Mesh> LeftRing3;
    public List<Mesh> LeftLittle1;
    public List<Mesh> LeftLittle2;
    public List<Mesh> LeftLittle3;
    
    public List<Mesh> RightShoulder;
    public List<Mesh> RightArm;
    public List<Mesh> RightForearm;
    public List<Mesh> RightHand;
    public List<Mesh> RightThumb1;
    public List<Mesh> RightThumb2;
    public List<Mesh> RightThumb3;    
    public List<Mesh> RightIndex1;
    public List<Mesh> RightIndex2;
    public List<Mesh> RightIndex3;    
    public List<Mesh> RightMiddle1;
    public List<Mesh> RightMiddle2;
    public List<Mesh> RightMiddle3;   
    public List<Mesh> RightRing1;
    public List<Mesh> RightRing2;
    public List<Mesh> RightRing3;
    public List<Mesh> RightLittle1;
    public List<Mesh> RightLittle2;
    public List<Mesh> RightLittle3;
    
    public List<Mesh> LeftUpperLeg;
    public List<Mesh> LeftLowerLeg;
    public List<Mesh> LeftFoot;
    public List<Mesh> LeftToes;
    
    public List<Mesh> RightUpperLeg;
    public List<Mesh> RightLowerLeg;
    public List<Mesh> RightFoot;
    public List<Mesh> RightToes;

    public List<Mesh> GetMeshesFromBone(HumanBodyBones bone)
    {
        switch (bone)
        {
            case HumanBodyBones.Hips: return Hips;
            case HumanBodyBones.Spine: return Spine;
            case HumanBodyBones.UpperChest: return Ribcage;
            case HumanBodyBones.Head: return Head;

            case HumanBodyBones.LeftShoulder: return LeftShoulder;
            case HumanBodyBones.LeftUpperArm: return LeftArm;
            case HumanBodyBones.LeftLowerArm: return LeftForearm;
            case HumanBodyBones.LeftHand: return LeftHand;
            case HumanBodyBones.LeftIndexProximal: return LeftIndex1;
            case HumanBodyBones.LeftIndexIntermediate: return LeftIndex2;
            case HumanBodyBones.LeftIndexDistal: return LeftIndex3;
            case HumanBodyBones.LeftMiddleProximal: return LeftMiddle1;
            case HumanBodyBones.LeftMiddleIntermediate: return LeftMiddle2;
            case HumanBodyBones.LeftMiddleDistal: return LeftMiddle3;
            case HumanBodyBones.LeftRingProximal: return LeftRing1;
            case HumanBodyBones.LeftRingIntermediate: return LeftRing2;
            case HumanBodyBones.LeftRingDistal: return LeftRing3;
            case HumanBodyBones.LeftLittleProximal: return LeftLittle1;
            case HumanBodyBones.LeftLittleIntermediate: return LeftLittle2;
            case HumanBodyBones.LeftLittleDistal: return LeftLittle3;
            case HumanBodyBones.LeftThumbProximal: return LeftThumb1;
            case HumanBodyBones.LeftThumbIntermediate: return LeftThumb2;
            case HumanBodyBones.LeftThumbDistal: return LeftThumb3;

            case HumanBodyBones.RightShoulder: return RightShoulder;
            case HumanBodyBones.RightUpperArm: return RightArm;
            case HumanBodyBones.RightLowerArm: return RightForearm;
            case HumanBodyBones.RightHand: return RightHand;
            case HumanBodyBones.RightIndexProximal: return RightIndex1;
            case HumanBodyBones.RightIndexIntermediate: return RightIndex2;
            case HumanBodyBones.RightIndexDistal: return RightIndex3;
            case HumanBodyBones.RightMiddleProximal: return RightMiddle1;
            case HumanBodyBones.RightMiddleIntermediate: return RightMiddle2;
            case HumanBodyBones.RightMiddleDistal: return RightMiddle3;
            case HumanBodyBones.RightRingProximal: return RightRing1;
            case HumanBodyBones.RightRingIntermediate: return RightRing2;
            case HumanBodyBones.RightRingDistal: return RightRing3;
            case HumanBodyBones.RightLittleProximal: return RightLittle1;
            case HumanBodyBones.RightLittleIntermediate: return RightLittle2;
            case HumanBodyBones.RightLittleDistal: return RightLittle3;
            case HumanBodyBones.RightThumbProximal: return RightThumb1;
            case HumanBodyBones.RightThumbIntermediate: return RightThumb2;
            case HumanBodyBones.RightThumbDistal: return RightThumb3;

            case HumanBodyBones.LeftUpperLeg: return LeftUpperLeg;
            case HumanBodyBones.LeftLowerLeg: return LeftLowerLeg;
            case HumanBodyBones.LeftFoot: return LeftFoot;
            case HumanBodyBones.LeftToes: return LeftToes;

            case HumanBodyBones.RightUpperLeg: return RightUpperLeg;
            case HumanBodyBones.RightLowerLeg: return RightLowerLeg;
            case HumanBodyBones.RightFoot: return RightFoot;
            case HumanBodyBones.RightToes: return RightToes;
            //no mesh provided
            default: return null;
        }
    }
}
