using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarManager : MonoBehaviour {

    Animator animatorAvatar;
    Animator animatorTarget;

    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneAvatar = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneTarget = new Dictionary<HumanBodyBones, GameObject>();

    // Use this for initialization
    void Start () {
        animatorAvatar = GetComponentInChildren<Animator>();
        animatorTarget = GameObject.FindGameObjectWithTag("Target").GetComponent<Animator>();
        InitializeBodyStructures();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        UpdatePDControllers();
	}
    /// <summary>
    ///     Maps all HumanBodyBones (assigned in the Avatar) to their GameObjects in the scene in order to get access to all components.
    ///     Adds Rigidbody to both bodies, adds PDController to the avatar.
    /// </summary>
    void InitializeBodyStructures()
    {
        foreach(HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone)
            {
                Transform boneTransformAvatar = animatorAvatar.GetBoneTransform(bone);
                Transform boneTransformTarget = animatorTarget.GetBoneTransform(bone);
                //We have to skip unassigned bodyparts.
                if (boneTransformAvatar != null && boneTransformTarget != null)
                {
                    //build Dictionaries
                    gameObjectPerBoneAvatar.Add(bone, boneTransformAvatar.gameObject);
                    gameObjectPerBoneTarget.Add(bone, boneTransformTarget.gameObject);

                    AssignRigidbodys(bone);
                    AssignPDControllers(bone);
                }
            }
        }
    }
    /// <summary>
    ///     A method to return the Rigidbody of the GameObject that corresponds to a certain bodypart. 
    ///     Use this to gain access to the velocity of the bodypart.
    /// </summary>
    Rigidbody GetRigidbodyFromBone(bool fromAvatar, HumanBodyBones boneID)
    {
        GameObject obj;
        if((fromAvatar ? gameObjectPerBoneAvatar : gameObjectPerBoneTarget).TryGetValue(boneID, out obj))
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if(rb != null)
            {
                return rb;
            } 
            else
            {
                Debug.Log("No rigidbody is assigned to the bone " + boneID + "\nMake sure to run AvatarManager.Initialize first.");
                return null;
            }
        }
        else
        {
            Debug.Log("No object is assigned to the bone " + boneID);
            return null;
        }
    }

    void AssignRigidbodys(HumanBodyBones bone)
    {
        gameObjectPerBoneAvatar[bone].AddComponent<Rigidbody>();
        gameObjectPerBoneAvatar[bone].GetComponent<Rigidbody>().useGravity = false;

        gameObjectPerBoneTarget[bone].AddComponent<Rigidbody>();
        gameObjectPerBoneTarget[bone].GetComponent<Rigidbody>().useGravity = false;
    }       

    void AssignPDControllers(HumanBodyBones bone)
    {
        gameObjectPerBoneAvatar[bone].AddComponent<PDController>();
        gameObjectPerBoneAvatar[bone].GetComponent<PDController>().rigidbody = gameObjectPerBoneAvatar[bone].GetComponent<Rigidbody>();
    }
   

    void AssignJoints() 
    {
        //Assumption: all joints can be modeled by configurable joints (ball joints, except for knees and elbows)
    }

    void UpdatePDControllers()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneAvatar.Keys)
        {
            Rigidbody targetRb = GetRigidbodyFromBone(false, bone);
            if(targetRb != null)
            {
                gameObjectPerBoneAvatar[bone].GetComponent<PDController>().SetDestination(gameObjectPerBoneTarget[bone].transform, targetRb.velocity);
            }
        }
    }

    public Dictionary<HumanBodyBones, GameObject> GetGameObjectPerBoneAvatarDictionary()
    {
        return gameObjectPerBoneAvatar;
    }    
    public Dictionary<HumanBodyBones, GameObject> GetGameObjectPerBoneTargetDictionary()
    {
        return gameObjectPerBoneTarget;
    }
}
