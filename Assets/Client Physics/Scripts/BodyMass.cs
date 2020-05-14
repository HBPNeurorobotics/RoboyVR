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
    const float HEAD_F_MASS = 0.0668f;
    const float HEAD_M_MASS = 0.0694f;

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

    #region Center of Mass Percentages of Body Part Length
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

    /// <summary>
    /// The population group that the values should be based on.
    /// </summary>
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
        SetBodyMasses(false);
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
    /// <summary>
    /// Sets mass and center of mass to anatomically realistic values. 
    /// </summary>
    /// <param name="optimized">true: hand and finger segments will weight 1kg. Choose to prevent fingers from separating during collisions with relatively heavy or immovable objects.</param>
    public void SetBodyMasses(bool optimized)
    {
        foreach (HumanBodyBones bone in dict.Keys)
        {
            float mass = 0;
            if(optimized && (bone.ToString().Contains("Hand") || bone.ToString().Contains("Thumb") || bone.ToString().Contains("Index") || bone.ToString().Contains("Middle") || bone.ToString().Contains("Ring") || bone.ToString().Contains("Little")))
            {
                mass = 1;
            }
            else
            {
                mass = DetermineMassOfBodyPart(bone);
            }

            if (dict[bone].GetComponent<Rigidbody>() != null)
            {
                dict[bone].GetComponent<Rigidbody>().mass = mass;
                dict[bone].GetComponent<Rigidbody>().centerOfMass = DetermineCenterOfMass(bone);
            }
        }
    }

    Vector3 DetermineCenterOfMass(HumanBodyBones bone)
    {
        switch (bone)
        {
            #region Torso
            case HumanBodyBones.Head: 
                return GetLengthOfBodySegment(bone) * GetPercentage( HEAD_F_COM, HEAD_M_COM );
            case HumanBodyBones.UpperChest: 
                return GetLengthOfBodySegment(bone) * GetPercentage( UPPER_TRUNK_F_COM, UPPER_TRUNK_M_COM );
            case HumanBodyBones.Chest:
                return GetLengthOfBodySegment(bone) * GetPercentage( MID_TRUNK_F_COM, MID_TRUNK_M_COM );
            case HumanBodyBones.Spine:
            case HumanBodyBones.Hips:
                return GetLengthOfBodySegment(bone) * GetPercentage( LOWER_TRUNK_F_COM, LOWER_TRUNK_M_COM ) ;
            #endregion

            #region Arms
            case HumanBodyBones.LeftUpperArm:
            case HumanBodyBones.RightUpperArm:
                return GetLengthOfBodySegment(bone) * GetPercentage( UPPER_ARM_F_COM, UPPER_ARM_M_COM );
            case HumanBodyBones.LeftLowerArm:
            case HumanBodyBones.RightLowerArm:
                return GetLengthOfBodySegment(bone) * GetPercentage( FORE_ARM_F_COM, FORE_ARM_M_COM );
            case HumanBodyBones.LeftHand:
            case HumanBodyBones.RightHand:
                return GetLengthOfBodySegment(bone) * GetPercentage( HAND_F_COM, HAND_M_COM );
            #endregion

            #region Legs
            case HumanBodyBones.LeftUpperLeg:
            case HumanBodyBones.RightUpperLeg:
                return GetLengthOfBodySegment(bone) * GetPercentage( THIGH_F_COM, THIGH_M_COM );
            case HumanBodyBones.LeftLowerLeg:
            case HumanBodyBones.RightLowerLeg:
                return GetLengthOfBodySegment(bone) * GetPercentage( SHANK_F_COM, SHANK_M_COM );
            case HumanBodyBones.LeftFoot:
            case HumanBodyBones.RightFoot:
                return GetLengthOfBodySegment(bone) * GetPercentage( FOOT_F_COM, FOOT_M_COM );
            #endregion
            //no values found, CoM remains unchanged
            default: return Vector3.zero;
        }
    }

    /// <summary>
    /// Approximates the length of a body part by comparing its position with its child. May not be precise for the hips.
    /// </summary>
    /// <param name="bone"></param>
    /// <returns></returns>
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
            #region Torso
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
            #endregion

            #region Arms
            case HumanBodyBones.LeftUpperArm:
            case HumanBodyBones.RightUpperArm:
                return totalMassKg * GetPercentage( UPPER_ARM_F_MASS, UPPER_ARM_M_MASS );
            case HumanBodyBones.LeftLowerArm:
            case HumanBodyBones.RightLowerArm:
                return totalMassKg * GetPercentage( FORE_ARM_F_MASS, FORE_ARM_M_MASS );
            case HumanBodyBones.LeftHand:
            case HumanBodyBones.RightHand:
                return totalMassKg * GetPercentage( HAND_F_MASS, HAND_M_MASS ) * 0.4f; //while there is data on the mass of the hands, that value includes the fingers. Assumption: 40% palm 60% fingers
            case HumanBodyBones.LeftIndexDistal:
            case HumanBodyBones.LeftIndexIntermediate:
            case HumanBodyBones.LeftIndexProximal:
            case HumanBodyBones.LeftMiddleDistal:
            case HumanBodyBones.LeftMiddleIntermediate:
            case HumanBodyBones.LeftMiddleProximal:
            case HumanBodyBones.LeftRingDistal:
            case HumanBodyBones.LeftRingIntermediate:
            case HumanBodyBones.LeftRingProximal:
            case HumanBodyBones.LeftLittleDistal:
            case HumanBodyBones.LeftLittleIntermediate:
            case HumanBodyBones.LeftLittleProximal:
            case HumanBodyBones.LeftThumbDistal:
            case HumanBodyBones.LeftThumbIntermediate:
            case HumanBodyBones.LeftThumbProximal:
            case HumanBodyBones.RightIndexDistal:
            case HumanBodyBones.RightIndexIntermediate:
            case HumanBodyBones.RightIndexProximal:
            case HumanBodyBones.RightMiddleDistal:
            case HumanBodyBones.RightMiddleIntermediate:
            case HumanBodyBones.RightMiddleProximal:
            case HumanBodyBones.RightRingDistal:
            case HumanBodyBones.RightRingIntermediate:
            case HumanBodyBones.RightRingProximal:
            case HumanBodyBones.RightLittleDistal:
            case HumanBodyBones.RightLittleIntermediate:
            case HumanBodyBones.RightLittleProximal:
            case HumanBodyBones.RightThumbDistal:
            case HumanBodyBones.RightThumbIntermediate:
            case HumanBodyBones.RightThumbProximal:
                return totalMassKg * ((GetPercentage(HAND_F_MASS, HAND_M_MASS) * 0.6f) / 15f); // for lack of data and to simplify, each finger segment weights the same
            #endregion

            #region Legs
            case HumanBodyBones.LeftUpperLeg:
            case HumanBodyBones.RightUpperLeg:
                return totalMassKg * GetPercentage( THIGH_F_MASS, THIGH_M_MASS );
            case HumanBodyBones.LeftLowerLeg:
            case HumanBodyBones.RightLowerLeg:
                return totalMassKg * GetPercentage( SHANK_F_MASS, SHANK_M_MASS );
            case HumanBodyBones.LeftFoot:
            case HumanBodyBones.RightFoot:
                return totalMassKg * GetPercentage( FOOT_F_MASS, FOOT_M_MASS );
            #endregion

            default: return 0.02f;
        }
    }

    /// <summary>
    /// Returns the percentage for a given body part, depending on chosen population group (MODE).
    /// </summary>
    /// <param name="female">The percentage for an average female's body part.</param>
    /// <param name="male">The percentage for an average male's body part.</param>
    /// <returns></returns>
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