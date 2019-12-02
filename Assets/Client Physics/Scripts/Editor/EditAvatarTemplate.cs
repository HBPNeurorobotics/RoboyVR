using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class EditAvatarTemplate : EditorWindow {
    float bodyWeight = 72f;
    string test = "test";
    bool isInitialized = false;
    bool mirrorAngles;

    Object template;
    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBone = new Dictionary<HumanBodyBones, GameObject>();

    List<JointSettings> jointSettings = new List<JointSettings>();

    [MenuItem("Window/Edit Joint Avatar Template")]
    public static void GetWindow()
    {
        EditAvatarTemplate editor = (EditAvatarTemplate)GetWindow(typeof(EditAvatarTemplate));
        editor.Show();
    }
    void OnGUI()
    {
        EditorGUILayout.HelpBox("The rig of AvatarTemplate", MessageType.Info);
        template = EditorGUILayout.ObjectField(template, typeof(GameObject), true);
        bodyWeight = EditorGUILayout.FloatField("Total Body Weight", bodyWeight);

        GetDictionary();

        EditorGUILayout.BeginVertical();

        GUILayout.Label("Joint Settings", EditorStyles.boldLabel);

        Debug.Log("bones: " + gameObjectsPerBone.Keys.Count);
        Debug.Log(jointSettings.Count);

        for (int i = 0; i < jointSettings.Count; i++)
        {
            //jointSettings[i] = (JointSettings)EditorGUILayout.ObjectField(jointSettings[i], typeof(JointSettings), true);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label(jointSettings[i].bone);
            jointSettings[i].angularXDriveDamper = EditorGUILayout.FloatField("Angular X Damper", jointSettings[i].angularXDriveDamper);
            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.EndVertical();

        //EditorGUILayout.PropertyField(SerializedObject, );

        if(GUILayout.Button("Update Template"))
        {
            BodyMass bodyMass = new BodyMass(bodyWeight, gameObjectsPerBone);
            Debug.Log("Body Weights assigned");
        }

        if(GUILayout.Button("Restore Default Mass"))
        {
            BodyMass bodyMass = new BodyMass(bodyWeight, gameObjectsPerBone); 
            bodyMass.RestoreOneValues();
        }

        if (GUILayout.Button("Restore All"))
        {
            foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
            {
                PrefabUtility.ResetToPrefabState(gameObjectsPerBone[bone]);
            }
            //template = (GameObject)Instantiate(Resources.Load("Assets/Client Physics/Prefabs/AvatarTemplate.prefab"));
        }
            
    }

    void GetDictionary()
    {
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone && !gameObjectsPerBone.ContainsKey(bone))
            {
                Transform boneTransformAvatar = ((GameObject)template).GetComponent<Animator>().GetBoneTransform(bone);
                if (boneTransformAvatar != null)
                {
                    gameObjectsPerBone.Add(bone, boneTransformAvatar.gameObject);
                    GetJointSettings(bone);
                }

            }
        }
    }

    void GetJointSettings(HumanBodyBones bone)
    {
        ConfigurableJoint joint = gameObjectsPerBone[bone].GetComponent<ConfigurableJoint>();
        if (joint != null)
        {
            JointSettings setting = new JointSettings(bone.ToString(), joint.angularXDrive.positionSpring, joint.angularXDrive.positionDamper, joint.angularYZDrive.positionSpring, joint.angularYZDrive.positionDamper);
            jointSettings.Add(setting);
        }
    }

    
}
