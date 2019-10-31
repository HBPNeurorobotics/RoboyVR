using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMass {

    float totalMassKg;
    AvatarManager avatarManager;
    Dictionary<HumanBodyBones, GameObject> dict;

    public BodyMass(float totalMassKg, AvatarManager avatarManager)
    {
        this.totalMassKg = totalMassKg;
        this.avatarManager = avatarManager;

        SetBodyMasses();
    }

    public void SetTotalMass(float totalMassKg)
    {
        this.totalMassKg = totalMassKg;
        SetBodyMasses();
    }

    void SetBodyMasses()
    {
        dict = avatarManager.GetGameObjectPerBoneAvatarDictionary();
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

            case HumanBodyBones.Head: return totalMassKg * 0.0823f;
            case HumanBodyBones.UpperChest: return totalMassKg * 0.1856f;
            case HumanBodyBones.Chest: return totalMassKg * 0.0826f;
            case HumanBodyBones.Spine: return totalMassKg * 0.1265f;
            default: return 1;
        }
    }
}
