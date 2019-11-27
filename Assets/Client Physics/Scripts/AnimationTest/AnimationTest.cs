using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationTest : MonoBehaviour {
    public TextAsset textFile;
    Dictionary<HumanBodyBones, GameObject> bones = new Dictionary<HumanBodyBones, GameObject>();
    Animator animator;
	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone)
            {
                Transform boneTransformAvatar = animator.GetBoneTransform(bone);
                if (boneTransformAvatar != null)
                {
                    bones.Add(bone, boneTransformAvatar.gameObject);
                    AnimationAngleObserver observer = bones[bone].AddComponent<AnimationAngleObserver>();
                }
                
            }
        }
	}
	
	// Update is called once per frame
	void Update () {

    }

    void OnApplicationQuit()
    {
        string path = "Assets/Client Physics/Scripts/AnimationTest/angles.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.Write(GetAnglesPerBone());
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
    }

    string GetAnglesPerBone()
    {
        string result = "";

        foreach(HumanBodyBones bone in bones.Keys)
        {
            AnimationAngleObserver observer = bones[bone].GetComponent<AnimationAngleObserver>();
            result += bone + 
                              ":\n\t\tMinAngleX: "+ observer.minAngleX + "\t\t\tMaxAngleX: " + observer.maxAngleX +
                              "\n\t\tMinAngleY: " + observer.minAngleY + "\t\t\tMaxAngleY: " + observer.maxAngleY +
                              "\n\t\tMinAngleZ: " + observer.minAngleZ + "\t\t\tMaxAngleZ: " + observer.maxAngleZ + 
                              "\n";
        }

        return result;
    }
    
}
