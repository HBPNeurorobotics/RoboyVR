using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class EditAvatarTemplate : EditorWindow
{
    float bodyWeight = 72f;
    float indent = 15f;
    string test = "test";
    bool useCollisions;
    bool noPreprocessing = true;
    bool hasColliders = false;
    bool mirror;
    bool showJointSettings = true;

    static EditAvatarTemplate editor;

    Vector2 scroll;

    Object template;
    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBone = new Dictionary<HumanBodyBones, GameObject>();

    Dictionary<HumanBodyBones, JointSettings> jointSettings = new Dictionary<HumanBodyBones, JointSettings>();
    Dictionary<HumanBodyBones, JointSettings> jointSettingsNoLeft = new Dictionary<HumanBodyBones, JointSettings>();

    BodyMass bodyMass;

    [MenuItem("Window/Edit Joint Avatar Template")]
    public static void GetWindow()
    {
        editor = (EditAvatarTemplate)GetWindow(typeof(EditAvatarTemplate));
        editor.Show();
    }
    void OnGUI()
    {
        EditorGUILayout.HelpBox("Assign the rig of AvatarTemplate", MessageType.Info);


        template = EditorGUILayout.ObjectField(template, typeof(GameObject), true);

        if (template != null)
        {
            EditorGUILayout.BeginVertical();
            bodyWeight = EditorGUILayout.FloatField("Total Body Weight", bodyWeight);

            GetDictionary();

            hasColliders = EditorGUILayout.Toggle("Add Colliders", hasColliders);

            if (hasColliders)
            {
                AddColliders();
            }
            else
            {
                RemoveColliders();
            }

            useCollisions = EditorGUILayout.Toggle("Enable Collisions Between Joints", useCollisions);
            noPreprocessing = EditorGUILayout.Toggle("Disable Preprocessing", noPreprocessing);
            mirror = EditorGUILayout.Toggle("Mirror Changes", mirror);


            showJointSettings = EditorGUILayout.Foldout(showJointSettings, "Joint Settings", true);

            if (showJointSettings)
            {
                DisplayJointSettings();
            }

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Enable Colliders"))
            {

            }

            if (GUILayout.Button("Restore Default Mass"))
            {
                bodyMass = new BodyMass(bodyWeight, gameObjectsPerBone);
                bodyMass.RestoreOneValues();
            }

            //TODO
            if (GUILayout.Button("Restore All"))
            {
                foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
                {
                    PrefabUtility.ResetToPrefabState(gameObjectsPerBone[bone]);
                }
                //template = (GameObject)Instantiate(Resources.Load("Assets/Client Physics/Prefabs/AvatarTemplate.prefab"));
            }

            if (GUILayout.Button("Update Template"))
            {
                UpdateTemplate();
            }
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
        if (!jointSettings.ContainsKey(bone))
        {
            ConfigurableJoint joint = gameObjectsPerBone[bone].GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                JointSettings setting = new JointSettings(bone.ToString(), joint.angularXDrive.positionSpring, joint.angularXDrive.positionDamper, joint.angularYZDrive.positionSpring, joint.angularYZDrive.positionDamper);
                jointSettings.Add(bone, setting);
                if (!setting.bone.ToString().StartsWith("Left"))
                {
                    jointSettingsNoLeft.Add(bone, setting);
                }
            }
        }
    }

    void DisplayJointSettings()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        Dictionary<HumanBodyBones, JointSettings> dict;

        dict = mirror ? jointSettingsNoLeft : jointSettings;

        //Display Joint Settings
        foreach (HumanBodyBones bone in dict.Keys)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            EditorGUILayout.BeginVertical();

            DisplayJointSettingsOfBone(bone);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }
        EditorGUILayout.EndScrollView();
    }

    void DisplayJointSettingsOfBone(HumanBodyBones bone)
    {
        jointSettings[bone].showInEditor = EditorGUILayout.Foldout(jointSettings[bone].showInEditor, jointSettings[bone].bone, true);

        if (jointSettings[bone].showInEditor)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            EditorGUILayout.BeginVertical();

            DisplayAngularDrives(bone);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }

    void DisplayAngularDrives(HumanBodyBones bone)
    {
        //Angular X Drive
        jointSettings[bone].showAngularXDriveInEditor = EditorGUILayout.Foldout(jointSettings[bone].showAngularXDriveInEditor, "Angular X Drive", true);
        if (jointSettings[bone].showAngularXDriveInEditor)
        {
            DisplayAngularDrivesHelper(bone, 0);
        }

        //Angular YZ Drive
        jointSettings[bone].showAngularYZDriveInEditor = EditorGUILayout.Foldout(jointSettings[bone].showAngularYZDriveInEditor, "Angular YZ Drive", true);
        if (jointSettings[bone].showAngularYZDriveInEditor)
        {
            DisplayAngularDrivesHelper(bone, 1);
        }
    }

    void DisplayAngularDrivesHelper(HumanBodyBones bone, int index)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        EditorGUILayout.BeginVertical();

        //Angular X Drive
        if (index == 0)
        {
            jointSettings[bone].angularXDriveSpring = EditorGUILayout.FloatField("Spring", jointSettings[bone].angularXDriveSpring);
            jointSettings[bone].angularXDriveDamper = EditorGUILayout.FloatField("Damper", jointSettings[bone].angularXDriveDamper);
        }
        else
        //Angular YZ Drive
        {
            jointSettings[bone].angularYZDriveSpring = EditorGUILayout.FloatField("Spring", jointSettings[bone].angularYZDriveSpring);
            jointSettings[bone].angularYZDriveDamper = EditorGUILayout.FloatField("Damper", jointSettings[bone].angularYZDriveDamper);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    void UpdateTemplate()
    {
        //Mass
        bodyMass = new BodyMass(bodyWeight, gameObjectsPerBone);
        bodyMass.SetBodyMasses();

        //Joint Settings
        UpdateJoints();

        //Refresh Values
        editor.Repaint();
    }

    void UpdateJoints()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
        {
            ConfigurableJoint joint = gameObjectsPerBone[bone].GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                joint.enableCollision = useCollisions;
                joint.enablePreprocessing = !noPreprocessing;
                JointSettings setting;
                if (jointSettings.TryGetValue(bone, out setting))
                {
                    if (mirror)
                    {
                        if (setting.bone.ToString().StartsWith("Left"))
                        {
                            setting = jointSettings[LeftToRightMapping(bone)];
                        }
                    }

                    ApplyJointSetting(joint, setting);
                }
            }
        }
    }

    HumanBodyBones LeftToRightMapping(HumanBodyBones bone)
    {
        switch (bone)
        {
            case HumanBodyBones.LeftFoot: return HumanBodyBones.RightFoot;
            case HumanBodyBones.LeftHand: return HumanBodyBones.RightHand;
            case HumanBodyBones.LeftIndexDistal: return HumanBodyBones.RightIndexDistal;
            case HumanBodyBones.LeftIndexIntermediate: return HumanBodyBones.RightIndexIntermediate;
            case HumanBodyBones.LeftIndexProximal: return HumanBodyBones.RightIndexProximal;
            case HumanBodyBones.LeftLittleDistal: return HumanBodyBones.RightLittleDistal;
            case HumanBodyBones.LeftLittleIntermediate: return HumanBodyBones.RightLittleIntermediate;
            case HumanBodyBones.LeftLittleProximal: return HumanBodyBones.RightLittleProximal;
            case HumanBodyBones.LeftLowerArm: return HumanBodyBones.RightLowerArm;
            case HumanBodyBones.LeftLowerLeg: return HumanBodyBones.RightLowerLeg;
            case HumanBodyBones.LeftMiddleDistal: return HumanBodyBones.RightMiddleDistal;
            case HumanBodyBones.LeftMiddleIntermediate: return HumanBodyBones.RightMiddleIntermediate;
            case HumanBodyBones.LeftMiddleProximal: return HumanBodyBones.RightMiddleProximal;
            case HumanBodyBones.LeftRingDistal: return HumanBodyBones.RightRingDistal;
            case HumanBodyBones.LeftRingIntermediate: return HumanBodyBones.RightRingIntermediate;
            case HumanBodyBones.LeftRingProximal: return HumanBodyBones.RightRingProximal;
            case HumanBodyBones.LeftShoulder: return HumanBodyBones.RightShoulder;
            case HumanBodyBones.LeftThumbDistal: return HumanBodyBones.RightThumbDistal;
            case HumanBodyBones.LeftThumbIntermediate: return HumanBodyBones.RightThumbIntermediate;
            case HumanBodyBones.LeftThumbProximal: return HumanBodyBones.RightThumbProximal;
            case HumanBodyBones.LeftToes: return HumanBodyBones.RightToes;
            case HumanBodyBones.LeftUpperArm: return HumanBodyBones.RightUpperArm;
            case HumanBodyBones.LeftUpperLeg: return HumanBodyBones.RightUpperLeg;
            default: return bone;
        }
    }

    void ApplyJointSetting(ConfigurableJoint joint, JointSettings setting)
    {
        JointDrive tmp;

        tmp = joint.angularXDrive;
        tmp.positionSpring = setting.angularXDriveSpring;
        tmp.positionDamper = setting.angularXDriveDamper;
        joint.angularXDrive = tmp;

        tmp = joint.angularYZDrive;
        tmp.positionSpring = setting.angularYZDriveSpring;
        tmp.positionDamper = setting.angularYZDriveDamper;
        joint.angularYZDrive = tmp;
    }

    void AddColliders()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
        {
            if (gameObjectsPerBone[bone].GetComponent<Collider>() == null)
            {
                switch (bone)
                {
                    //SphereColliders
                    case HumanBodyBones.Head:
                    case HumanBodyBones.LeftShoulder:
                    case HumanBodyBones.RightShoulder: gameObjectsPerBone[bone].AddComponent<SphereCollider>(); return;
                    //Box Colliders
                    case HumanBodyBones.Hips:
                    case HumanBodyBones.Spine:
                    case HumanBodyBones.Chest:
                    case HumanBodyBones.UpperChest: gameObjectsPerBone[bone].AddComponent<BoxCollider>(); return;
                    //Capsule Colliders
                    default: gameObjectsPerBone[bone].AddComponent<CapsuleCollider>(); return;
                }
            }
        }
    }

    void RemoveColliders()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
        {
            Collider collider = gameObjectsPerBone[bone].GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
        }
    }
}
