using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BodyMass
{

    float totalMassKg;
    Dictionary<HumanBodyBones, GameObject> dict;
    MODE mode;

    #region Mass Percentages of Body Weight
    const float HEAD_F_MASS = 0.1247f;
    const float HEAD_M_MASS = 0.1117f;

    const float UPPER_TRUNK_F_MASS = 0.1545f;
    const float UPPER_TRUNK_M_MASS = 0.1596f;

    const float MID_TRUNK_F_MASS = 0.1465f;
    const float MID_TRUNK_M_MASS = 0.1633f;

    const float LOWER_TRUNK_F_MASS = 0.1247f;
    const float LOWER_TRUNK_M_MASS = 0.1117f;

    const float FORE_ARM_F_MASS = 0.0138f;
    const float FORE_ARM_M_MASS = 0.0162f;

    const float UPPER_ARM_F_MASS = 0.0255f;
    const float UPPER_ARM_M_MASS = 0.0271f;

    const float HAND_F_MASS = 0.0056f;
    const float HAND_M_MASS = 0.0061f;

    const float THIGH_F_MASS = 0.1478f;
    const float THIGH_M_MASS = 0.1416f;

    const float SHANK_F_MASS = 0.0481f;
    const float SHANK_M_MASS = 0.0433f;

    const float FOOT_F_MASS = 0.0129f;
    const float FOOT_M_MASS = 0.0137f;

    #endregion

    #region Center of Mass Percentages of Body Weight
    const float HEAD_F_COM = 0.5894f;
    const float HEAD_M_COM = 0.5976f;

    const float UPPER_TRUNK_F_COM = 0.2077f;
    const float UPPER_TRUNK_M_COM = 0.2999f;

    const float MID_TRUNK_F_COM = 0.4512f;
    const float MID_TRUNK_M_COM = 0.4502f;

    const float LOWER_TRUNK_F_COM = 0.4920f;
    const float LOWER_TRUNK_M_COM = 0.6115f;

    const float UPPER_ARM_F_COM = 0.5754f;
    const float UPPER_ARM_M_COM = 0.5772f;

    const float FORE_ARM_F_COM = 0.4559f;
    const float FORE_ARM_M_COM = 0.4574f;

    const float HAND_F_COM = 0.7474f;
    const float HAND_M_COM = 0.79f;

    const float THIGH_F_COM = 0.3612f;
    const float THIGH_M_COM = 0.4095f;

    const float SHANK_F_COM = 0.4416f;
    const float SHANK_M_COM = 0.4459f;

    const float FOOT_F_COM = 0.4014f;
    const float FOOT_M_COM = 0.4415f;

    #endregion

    public enum MODE
    {
        FEMALE,
        MALE,
        AVERAGE
    }

    public BodyMass(float totalMassKg, Dictionary<HumanBodyBones, GameObject> gameObjectFromBone, MODE mode)
    {
        this.totalMassKg = totalMassKg;
        dict = gameObjectFromBone;
        this.mode = mode;
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

    /// <summary>
    /// Use to update the mass of all body parts according to new body weight
    /// </summary>
    /// <param name="totalMassKg">The new body weight. Each body part will weight a percentage of it.</param>
    public void SetTotalMass(float totalMassKg)
    {
        this.totalMassKg = totalMassKg;
        SetBodyMasses();
    }
    /// <summary>
    /// Sets the mass of all body parts to be equal to 1.
    /// </summary>
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
                dict[bone].GetComponent<Rigidbody>().centerOfMass = DetermineCenterOfMass(bone);
            }
        }
    }

    Vector3 DetermineCenterOfMass(HumanBodyBones bone)
    {
        switch (bone)
        {
            case HumanBodyBones.Head: 
                return GetLengthOfBodySegment(bone) * GetPercentage( HEAD_F_COM, HEAD_M_COM );
            case HumanBodyBones.UpperChest: 
                return GetLengthOfBodySegment(bone) * GetPercentage( UPPER_TRUNK_F_COM, UPPER_TRUNK_M_COM );
            case HumanBodyBones.Chest:
                return GetLengthOfBodySegment(bone) * GetPercentage( MID_TRUNK_F_COM, MID_TRUNK_M_COM );
            case HumanBodyBones.Spine:
            case HumanBodyBones.Hips:
                return GetLengthOfBodySegment(bone) * GetPercentage( LOWER_TRUNK_F_COM, LOWER_TRUNK_M_COM );
            case HumanBodyBones.LeftUpperArm:
            case HumanBodyBones.RightUpperArm:
                return GetLengthOfBodySegment(bone) * GetPercentage( UPPER_ARM_F_COM, UPPER_ARM_M_COM );
            case HumanBodyBones.LeftLowerArm:
            case HumanBodyBones.RightLowerArm:
                return GetLengthOfBodySegment(bone) * GetPercentage( FORE_ARM_F_COM, FORE_ARM_M_COM );
            case HumanBodyBones.LeftHand:
            case HumanBodyBones.RightHand:
                return GetLengthOfBodySegment(bone) * GetPercentage( HAND_F_COM, HAND_M_COM );
            case HumanBodyBones.LeftUpperLeg:
            case HumanBodyBones.RightUpperLeg:
                return GetLengthOfBodySegment(bone) * GetPercentage( THIGH_F_COM, THIGH_M_COM );
            case HumanBodyBones.LeftLowerLeg:
            case HumanBodyBones.RightLowerLeg:
                return GetLengthOfBodySegment(bone) * GetPercentage( SHANK_F_COM, SHANK_M_COM );
            case HumanBodyBones.LeftFoot:
            case HumanBodyBones.RightFoot:
                return GetLengthOfBodySegment(bone) * GetPercentage( FOOT_F_COM, FOOT_M_COM );
            default: return Vector3.zero;
        }
    }

    Vector3 GetLengthOfBodySegment(HumanBodyBones bone)
    {
        return (dict[bone].transform.GetChild(0).transform.position - dict[bone].transform.position);
    }

    /// <summary>
    /// Calculates the average weight of a human body part. Default is used for body parts for which no value has been found.
    /// </summary>
    /// <param name="bone">The bone of the body part.</param>
    /// <returns></returns>
    float DetermineMassOfBodyPart(HumanBodyBones bone)
    {
        switch (bone)
        {
            case HumanBodyBones.Head: return totalMassKg * GetPercentage( HEAD_F_MASS, HEAD_M_MASS );
            case HumanBodyBones.UpperChest:
            case HumanBodyBones.LeftShoulder:
            case HumanBodyBones.RightShoulder:
            case HumanBodyBones.Neck:
                return totalMassKg * GetPercentage( UPPER_TRUNK_F_MASS, UPPER_TRUNK_M_MASS ) / 4f; //Upper Trunk -> assumption: mass is roughly equally distributed
            case HumanBodyBones.Chest: return totalMassKg * GetPercentage( MID_TRUNK_F_MASS, MID_TRUNK_M_MASS ); //Mid Trunk
            case HumanBodyBones.Spine:
            case HumanBodyBones.Hips:
                return totalMassKg * GetPercentage( LOWER_TRUNK_F_MASS, LOWER_TRUNK_M_MASS ) / 2; //Lower Trunk -> assumption: mass is roughly equally distributed
            case HumanBodyBones.LeftUpperArm:
            case HumanBodyBones.RightUpperArm:
                return totalMassKg * GetPercentage( UPPER_ARM_F_MASS, UPPER_ARM_M_MASS );
            case HumanBodyBones.LeftLowerArm:
            case HumanBodyBones.RightLowerArm:
                return totalMassKg * GetPercentage( FORE_ARM_F_MASS, FORE_ARM_M_MASS );
            case HumanBodyBones.LeftHand:
            case HumanBodyBones.RightHand:
                return totalMassKg * GetPercentage( HAND_F_MASS, HAND_M_MASS );
            case HumanBodyBones.LeftUpperLeg:
            case HumanBodyBones.RightUpperLeg:
                return totalMassKg * GetPercentage( THIGH_F_MASS, THIGH_M_MASS );
            case HumanBodyBones.LeftLowerLeg:
            case HumanBodyBones.RightLowerLeg:
                return totalMassKg * GetPercentage( SHANK_F_MASS, SHANK_M_MASS );
            case HumanBodyBones.LeftFoot:
            case HumanBodyBones.RightFoot:
                return totalMassKg * GetPercentage( FOOT_F_MASS, FOOT_M_MASS );
            default: return 0.02f;
        }
    }

    float GetPercentage(float female, float male)
    {
        switch (mode)
        {
            case MODE.FEMALE: return female;
            case MODE.MALE: return male;
            default: return Average(new float[] { female, male });
        }
    }

    /// <summary>
    /// Computes the avarage value of a given array.
    /// </summary>
    /// <param name="values">The values to compute the avarage of.</param>
    /// <returns></returns>
    float Average(float[] values)
    {
        float sum = 0;
        foreach (float x in values)
        {
            sum += x;
        }
        return sum / values.Length;
    }
}