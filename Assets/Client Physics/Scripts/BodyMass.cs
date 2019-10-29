using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMass {

    float totalMassKg;
    
    public BodyMass(float totalMassKg, AvatarManager avatarManager)
    {
        this.totalMassKg = totalMassKg;
        Dictionary<HumanBodyBones, GameObject> dict = avatarManager.GetGameObjectPerBoneAvatarDictionary();
        foreach(HumanBodyBones bone in dict.Keys)
        {
            if(dict[bone].GetComponent<Rigidbody>() != null)
            {
                dict[bone].GetComponent<Rigidbody>().mass = DetermineMassOfBodyPart(bone);
            }          
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
