using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationTest : MonoBehaviour {
    public List<AnimationClip> animations = new List<AnimationClip>();
    Dictionary<HumanBodyBones, GameObject> bones = new Dictionary<HumanBodyBones, GameObject>();
    Animator animator;
	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone) //&& bone == HumanBodyBones.LeftLowerArm) 
            {
                Transform boneTransformAvatar = animator.GetBoneTransform(bone);
                if (boneTransformAvatar != null)
                {
                    bones.Add(bone, boneTransformAvatar.gameObject);
                    AnimationAngleObserver observer = bones[bone].AddComponent<AnimationAngleObserver>();
                }
                
            }
        }
        foreach (AnimationClip clip in animations)
        {
            AnimationState state = new AnimationState();
            StartCoroutine(PlayAnimations(clip));
        }
        
	}
	
	// Update is called once per frame
	void Update () {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Done"))
        {
            OnTestFinished();
            Debug.Log("Test Done");
            Destroy(this);
        }
    }

    IEnumerator PlayAnimations(AnimationClip clip)
    {
        animator.Play(clip.name);
        yield return new WaitForSeconds(clip.length);
    }


    void OnTestFinished()
    {

        string path = "Assets/Client Physics/Scripts/AnimationTest/angles.txt";

        File.Delete(path);

        StreamWriter writer = new StreamWriter(path, true);

        writer.Write(GetAnglesPerBone());
        writer.Close();

        AssetDatabase.ImportAsset(path);
    }

    string GetAnglesPerBone()
    {
        string result = "";

        foreach(HumanBodyBones bone in bones.Keys)
        {
            AnimationAngleObserver observer = bones[bone].GetComponent<AnimationAngleObserver>();
            /*
             * result += bone + 
                              ":\n\t\tMinAngleX: "+ Math.Round(observer.minAngleX, 3) + "\t\t\tMaxAngleX: " + Math.Round(observer.maxAngleX, 3) +
                              "\n\t\tMinAngleY: " + Math.Round(observer.minAngleY, 3) + "\t\t\tMaxAngleY: " + Math.Round(observer.maxAngleY, 3) +
                              "\n\t\tMinAngleZ: " + Math.Round(observer.minAngleZ, 3) + "\t\t\tMaxAngleZ: " + Math.Round(observer.maxAngleZ, 3) + 
                              "\n";
                              */
            result += JsonUtility.ToJson(observer.GetJointAngleContainer(bone)) + "\n";
        }

        return result;
    }
    
}
