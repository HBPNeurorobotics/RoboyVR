using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BodyMass
{

    float totalMassKg;
    Dictionary<HumanBodyBones, GameObject> dict;

    public BodyMass(float totalMassKg, Dictionary<HumanBodyBones, GameObject> gameObjectFromBone)
    {
        this.totalMassKg = totalMassKg;
        dict = gameObjectFromBone;

        //SetBodyMasses();
    }

    public void AdjustWeightOfBodyPart(HumanBodyBones bone, float mass)
    {
        Rigidbody rb = dict[bone].GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = mass;
        }
    }

    public void SetTotalMass(float totalMassKg)
    {
        this.totalMassKg = totalMassKg;
        SetBodyMasses();
    }

    public void RestoreOneValues()
    {
        Rigidbody rb;
        foreach (HumanBodyBones bone in dict.Keys)
        {
            rb = dict[bone].GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 1;
            }
        }
    }

    public void SetBodyMasses()
    {
        foreach (HumanBodyBones bone in dict.Keys)
        {
            if (dict[bone].GetComponent<Rigidbody>() != null)
            {
                dict[bone].GetComponent<Rigidbody>().mass = DetermineMassOfBodyPart(bone);
            }
        }
    }

    void ChangeMassOfBodyPart(HumanBodyBones bone, float newMass)
    {
        if (dict[bone].GetComponent<Rigidbody>() != null)
        {
            dict[bone].GetComponent<Rigidbody>().mass = newMass;
        }
    }

    float DetermineMassOfBodyPart(HumanBodyBones bone)
    {
        switch (bone)
        {
            case HumanBodyBones.Head: return totalMassKg * Average(new float[] { 0.0668f, 0.0694f });
            case HumanBodyBones.UpperChest:
            case HumanBodyBones.LeftShoulder:
            case HumanBodyBones.RightShoulder:
                return totalMassKg * Average(new float[] { 0.1545f, 0.1596f }) / 3; //Upper Trunk
            case HumanBodyBones.Chest: return totalMassKg * Average(new float[] { 0.1465f, 0.1633f }); //Mid Trunk
            case HumanBodyBones.Spine:
            case HumanBodyBones.Hips:
                return totalMassKg * Average(new float[] { 0.1247f, 0.1117f }) / 2; //Lower Trunk
            case HumanBodyBones.LeftUpperArm:
            case HumanBodyBones.RightUpperArm:
                return totalMassKg * Average(new float[] { 0.0255f, 0.0271f });
            case HumanBodyBones.LeftLowerArm:
            case HumanBodyBones.RightLowerArm:
                return totalMassKg * Average(new float[] { 0.0138f, 0.0162f });
            case HumanBodyBones.LeftHand:
            case HumanBodyBones.RightHand:
                return totalMassKg * Average(new float[] { 0.0056f, 0.0061f });
            case HumanBodyBones.LeftUpperLeg:
            case HumanBodyBones.RightUpperLeg:
                return totalMassKg * Average(new float[] { 0.1478f, 0.1416f });
            case HumanBodyBones.LeftLowerLeg:
            case HumanBodyBones.RightLowerLeg:
                return totalMassKg * Average(new float[] { 0.0481f, 0.0433f });
            case HumanBodyBones.LeftFoot:
            case HumanBodyBones.RightFoot:
                return totalMassKg * Average(new float[] { 0.0129f, 0.0137f });
            default: return 0.02f;
        }
    }

    public float Average(float[] values)
    {
        float sum = 0;
        foreach (float x in values)
        {
            sum += x;
        }
        return sum / values.Length;
    }
}