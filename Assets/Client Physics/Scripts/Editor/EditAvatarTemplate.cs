using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

[ExecuteInEditMode]
public class EditAvatarTemplate : EditorWindow
{
    float bodyWeight = 72f;
    BodyMass.MODE mode = BodyMass.MODE.AVERAGE;
    float indent = 15f;

    bool useCollisions;
    bool noPreprocessing = true;
    bool setGlobalJointSetting = false;
    bool selectGroupConfigJoints = false;
    bool mirror;
    bool showJointSettings = true;
    bool useGravity = true;

    public float angularXDriveSpringGlobal;
    public float angularXDriveDamperGlobal;
    public float maxForceXGlobal;

    public float angularYZDriveSpringGlobal;
    public float angularYZDriveDamperGlobal;
    public float maxForceYZGlobal;
    JointSettings globalSettings;

    JointSettings groupSettings;

    BodyGroups.BODYGROUP bodyGroup;

    static EditAvatarTemplate editor;

    Vector2 scroll;

    Object template;
    Object templateMultiple;

    TextAsset savedEditor;
    string fileName = "";
    bool loadFromFile;
    bool gatheredTemplateSettings;

    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBoneTemplate = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBoneTemplateMultiple = new Dictionary<HumanBodyBones, GameObject>();

    Dictionary<HumanBodyBones, JointSettings> jointSettings = new Dictionary<HumanBodyBones, JointSettings>();

    HashSet<HumanBodyBones> jointsLeft = new HashSet<HumanBodyBones>();
    HashSet<HumanBodyBones> jointsRight = new HashSet<HumanBodyBones>();
    HashSet<HumanBodyBones> toDisplay = new HashSet<HumanBodyBones>();

    BodyMass bodyMass;

    [MenuItem("Window/Edit Joint Avatar Template")]
    public static void GetWindow()
    {
        editor = (EditAvatarTemplate)GetWindow(typeof(EditAvatarTemplate));
        editor.Show();
    }
    void OnGUI()
    {
        //editor = (EditAvatarTemplate)GetWindow(typeof(EditAvatarTemplate));
        EditorGUILayout.HelpBox("Assign the rig of AvatarTemplate and AvatarTemplateMultipleJoints", MessageType.Info);
        template = EditorGUILayout.ObjectField("Avatar Template Rig", template, typeof(GameObject), true);
        templateMultiple = EditorGUILayout.ObjectField("Avatar Template Multiple Joints Rig", templateMultiple, typeof(GameObject), true);

        EditorGUILayout.HelpBox("Assign a previously saved joint settings file to continue working on it. Reset to \"None\" to use the values in the assigned AvatarTemplate.", MessageType.Info);

        #region Load & Save
        savedEditor = (TextAsset)EditorGUILayout.ObjectField("Avatar Template Multiple Joints Rig", savedEditor, typeof(TextAsset), true);
        fileName = EditorGUILayout.TextField("New Name / Overwrite", fileName);

        EditorGUILayout.BeginHorizontal();
        if (savedEditor == null)
        {
            GUI.enabled = false;
            loadFromFile = false;
            //if not done so already, empty previously loaded settings (we do not want to execute in every editor update)
            if (!gatheredTemplateSettings)
            {
                //set clean dictionaries
                ClearConstructs();
                gatheredTemplateSettings = true;
            }
            GUILayout.Button("Load from Json");
        }
        else
        {
            GUI.enabled = true;
            if (GUILayout.Button("Load from Json"))
            {
                loadFromFile = true;
                jointSettings = RecoverJointSettingsFromJson(savedEditor.text);
                gatheredTemplateSettings = false;
            }
        }
        if (template != null && templateMultiple != null)
        {
            GUI.enabled = true;
        }
        if (GUILayout.Button("Save to new Json"))
        {
            SaveJointSettingsAsJson();
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        if (template != null && templateMultiple != null)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            #region Editor Parameters

            EditorGUILayout.BeginVertical();
            bodyWeight = EditorGUILayout.FloatField("Total Body Weight", bodyWeight);
            mode = (BodyMass.MODE)EditorGUILayout.EnumPopup("Population Group of Avatar", mode);

            bodyGroup = (BodyGroups.BODYGROUP)EditorGUILayout.EnumPopup("Body Group", bodyGroup);

            GetDictionary();
            
            useGravity = EditorGUILayout.Toggle("Enable Gravity", useGravity);
            useCollisions = EditorGUILayout.Toggle("Enable Collisions Between Joints", useCollisions);
            noPreprocessing = EditorGUILayout.Toggle("Disable Preprocessing", noPreprocessing);

            //Global Joints Settings will apply to all joints
            setGlobalJointSetting = EditorGUILayout.Toggle("Set Global Joint Settings", setGlobalJointSetting);
            globalSettings = new JointSettings(HumanBodyBones.LastBone, angularXDriveSpringGlobal, angularXDriveDamperGlobal, maxForceXGlobal, angularYZDriveSpringGlobal, angularYZDriveDamperGlobal, maxForceYZGlobal);


            GUI.enabled = false;
            if (!setGlobalJointSetting)
            {
                GUI.enabled = true;
            }

            selectGroupConfigJoints = EditorGUILayout.Toggle("Select Joints in Scene", selectGroupConfigJoints);
            mirror = EditorGUILayout.Toggle("Mirror Settings", mirror);
            if (selectGroupConfigJoints)
            {
                SelectJointsInTemplate();
            }

            GUI.enabled = true;
            if (!setGlobalJointSetting && !selectGroupConfigJoints)
            {
                showJointSettings = EditorGUILayout.Foldout(showJointSettings, "Joint Settings", true);
                if (showJointSettings)
                {
                    DisplayJointSettings();
                }
            }
            else if (setGlobalJointSetting)
            {
                DisplayGlobalJoint();
            }
            EditorGUILayout.EndVertical();
            #endregion

            #region Buttons at bottom
            GUILayout.BeginHorizontal();
            GUI.enabled = true;
            if (GUILayout.Button("Set BodyMass"))
            {
                bodyMass = new BodyMass(bodyWeight, gameObjectsPerBoneTemplate, mode);
                bodyMass.SetBodyMasses();

                foreach (HumanBodyBones bone in jointSettings.Keys)
                {
                    if (gameObjectsPerBoneTemplate.ContainsKey(bone))
                    {
                        jointSettings[bone].mass = gameObjectsPerBoneTemplate[bone].GetComponent<Rigidbody>().mass;
                    }
                }
            }

            if (GUILayout.Button("Restore 1 Mass"))
            {
                bodyMass = new BodyMass(bodyWeight, gameObjectsPerBoneTemplate, mode);
                bodyMass.RestoreOneValues();

                foreach (HumanBodyBones bone in jointSettings.Keys)
                {
                    if (gameObjectsPerBoneTemplate.ContainsKey(bone))
                    {
                        jointSettings[bone].mass = gameObjectsPerBoneTemplate[bone].GetComponent<Rigidbody>().mass;
                    }
                }
            }
            GUILayout.EndHorizontal();
            //TODO
            if (GUILayout.Button("Restore All"))
            {
                foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
                {
                    PrefabUtility.ResetToPrefabState(gameObjectsPerBoneTemplate[bone]);
                }
            }

            if (GUILayout.Button("Update Templates"))
            {
                UpdateTemplate();
            }
            #endregion
            EditorGUILayout.EndScrollView();
        }
    }

    void ClearConstructs()
    {
        List<HumanBodyBones> jointSettingsKeys = jointSettings.Keys.ToList();
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (jointSettings.ContainsKey(bone))
            {
                jointSettings.Remove(bone);
            }
            if (jointsLeft.Contains(bone))
            {
                jointsLeft.Remove(bone);
            }
            if (gameObjectsPerBoneTemplate.ContainsKey(bone))
            {
                gameObjectsPerBoneTemplate.Remove(bone);
            }
            if (gameObjectsPerBoneTemplateMultiple.ContainsKey(bone))
            {
                gameObjectsPerBoneTemplateMultiple.Remove(bone);
            }
        }
    }

    void GetDictionary()
    {
        Dictionary<HumanBodyBones, GameObject> chosenBones = new Dictionary<HumanBodyBones, GameObject>();
        Animator animator = ((GameObject)template).GetComponent<Animator>();
        Animator animatorMultiple = ((GameObject)templateMultiple).GetComponent<Animator>();
        BodyGroups bodyGroups = new BodyGroups(animator);
        switch (bodyGroup)
        {
            case BodyGroups.BODYGROUP.ALL_COMBINED: chosenBones = bodyGroups.AllCombined(); break;
            case BodyGroups.BODYGROUP.HEAD: chosenBones = bodyGroups.Head(); break;
            case BodyGroups.BODYGROUP.LEFT_ARM: chosenBones = bodyGroups.LeftArm(); break;
            case BodyGroups.BODYGROUP.LEFT_FOOT: chosenBones = bodyGroups.LeftFoot(); break;
            case BodyGroups.BODYGROUP.LEFT_FINGERS: chosenBones = bodyGroups.LeftFingers(); break;
            case BodyGroups.BODYGROUP.LEFT_LEG: chosenBones = bodyGroups.LeftLeg(); break;
            case BodyGroups.BODYGROUP.RIGHT_ARM: chosenBones = bodyGroups.RightArm(); break;
            case BodyGroups.BODYGROUP.RIGHT_FOOT: chosenBones = bodyGroups.RightFoot(); break;
            case BodyGroups.BODYGROUP.RIGHT_FINGERS: chosenBones = bodyGroups.RightFingers(); break;
            case BodyGroups.BODYGROUP.RIGHT_LEG: chosenBones = bodyGroups.RightLeg(); break;
            case BodyGroups.BODYGROUP.TRUNK: chosenBones = bodyGroups.Trunk(); break;
            case BodyGroups.BODYGROUP.TRUNK_HEAD: chosenBones = bodyGroups.TrunkHead(); break;
            default: break;
        }

        if (!loadFromFile)
        {
            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (!bone.Equals(HumanBodyBones.LastBone))
                {
                    AddJointSettings(bone, animator, animatorMultiple, chosenBones);
                }
            }
        }
    }

    void AddJointSettings(HumanBodyBones bone, Animator animator, Animator animatorMultiple, Dictionary<HumanBodyBones, GameObject> chosenBones)
    {
        Transform boneTransformAvatar = animator.GetBoneTransform(bone);
        Transform boneTransformAvatarMultiple = animatorMultiple.GetBoneTransform(bone);

        //we have to avoid duplicates in our dictionaries
        if (!gameObjectsPerBoneTemplate.ContainsKey(bone) && !gameObjectsPerBoneTemplate.ContainsKey(bone))
        {
            if (boneTransformAvatar != null && boneTransformAvatarMultiple != null)
            {
                gameObjectsPerBoneTemplate.Add(bone, boneTransformAvatar.gameObject);
                gameObjectsPerBoneTemplateMultiple.Add(bone, boneTransformAvatarMultiple.gameObject);
                GetJointSettings(bone);
            }
        }

        //chosenBones are the bones that will be displayed in the editor
        if (chosenBones.ContainsKey(bone))
        {
            toDisplay.Add(bone);
        }
        else
        {
            if (toDisplay.Contains(bone))
            {
                toDisplay.Remove(bone);
            }
        }
    }

    /// <summary>
    /// Returns the JointSettings of a specified bone. Use only if a single ConfigurableJoint is attached to the bone.
    /// </summary>
    /// <param name="bone"></param>
    void GetJointSettings(HumanBodyBones bone)
    {
        if (!jointSettings.ContainsKey(bone))
        {
            ConfigurableJoint joint = gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                JointSettings setting = new JointSettings(bone, joint);
                jointSettings.Add(bone, setting);

                if (setting.bone.ToString().StartsWith("Left"))
                {
                    jointsLeft.Add(bone);
                }
                if (setting.bone.ToString().StartsWith("Right"))
                {
                    jointsRight.Add(bone);
                }
            }

        }
    }
    /// <summary>
    /// Shows all JointSettings in Editor.
    /// </summary>
    void DisplayJointSettings()
    {
        //Dictionary<HumanBodyBones, JointSettings> dict;

        //dict = mirror ? jointSettingsNoLeft : jointSettings;
        HashSet<HumanBodyBones> toDisplayHelper = toDisplay;

        if (mirror)
        {
            bool hasOnlyLeft = true;
            bool hasOnlyRight = true;
            foreach (HumanBodyBones bone in toDisplay)
            {
                if (bone.ToString().StartsWith("Right")) hasOnlyLeft = false;
                if (bone.ToString().StartsWith("Left")) hasOnlyRight = false;
            }

            if (hasOnlyRight)
            {
                //only show joints on the right
                toDisplayHelper.IntersectWith(jointsRight);
            }
            else if (hasOnlyLeft)
            {
                //only show joints on the left
                toDisplayHelper.IntersectWith(jointsLeft);
            }
            else
            {
                //if mixed, show joints on the right
                toDisplayHelper.ExceptWith(jointsLeft);
            }
        }

        List<HumanBodyBones> sortedJoints = new List<HumanBodyBones>(toDisplayHelper);
        sortedJoints.Sort();

        //Display Joint Settings
        foreach (HumanBodyBones bone in sortedJoints)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            EditorGUILayout.BeginVertical();

            DisplayJointSettingsOfBone(jointSettings[bone]);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }


    }
    /// <summary>
    /// Shows single JointSettings in Editor.
    /// </summary>
    /// <param name="boneSettings">The JointSettings to show.</param>
    void DisplayJointSettingsOfBone(JointSettings boneSettings)
    {
        boneSettings.showInEditor = EditorGUILayout.Foldout(boneSettings.showInEditor, setGlobalJointSetting ? "Global Joint" : boneSettings.bone.ToString(), true);

        if (boneSettings.showInEditor || setGlobalJointSetting)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            EditorGUILayout.BeginVertical();

            DisplayJointValues(boneSettings);
            if (mirror) MirrorJointValues(boneSettings);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }

    void MirrorJointValues(JointSettings settings)
    {
        if (settings.bone.ToString().StartsWith("Left")) CopySettings(settings, jointSettings[LeftToRightMapping(settings.bone)]);
        if (settings.bone.ToString().StartsWith("Right")) CopySettings(settings, jointSettings[RightToLeftMapping(settings.bone)]);
    }

    void CopySettings(JointSettings settings, JointSettings mappedSettings)
    {
        //changing the primary/secondary axis depends on the exact bone. It is not supported right now, if needed it could be done similar to the split joint method in JointSetup
        //we cannot just copy the settings as a whole. This would result in wrong axis orientations and wrong bone identity 
        mappedSettings.angularLimitHighX = settings.angularLimitHighX;
        mappedSettings.angularLimitLowX = settings.angularLimitLowX;
        mappedSettings.angularLimitY = settings.angularLimitY;
        mappedSettings.angularLimitZ = settings.angularLimitZ;
        mappedSettings.angularXDriveDamper = settings.angularXDriveDamper;
        mappedSettings.angularXDriveSpring = settings.angularXDriveSpring;
        mappedSettings.angularYZDriveDamper = settings.angularYZDriveDamper;
        mappedSettings.angularYZDriveSpring = settings.angularYZDriveSpring;
        mappedSettings.maxForceX = settings.maxForceX;
        mappedSettings.maxForceYZ = settings.maxForceYZ;
    }

    /// <summary>
    /// Shows the values of a JointSettings.
    /// </summary>
    /// <param name="settings"></param>
    void DisplayJointValues(JointSettings settings)
    {
        //More functions could be added here, if other joint parameters are to be modified in the future
        DisplayAngularLimits(settings);
        DisplayAngularDrives(settings);
    }

    void DisplayAngularLimits(JointSettings settings)
    {
        settings.showAngularLimitsInEditor = EditorGUILayout.Foldout(settings.showAngularLimitsInEditor, "Angular Joint Limits", true);
        if (settings.showAngularLimitsInEditor || setGlobalJointSetting)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            EditorGUILayout.BeginVertical();

            settings.angularLimitLowX = EditorGUILayout.FloatField("Low X Limit", settings.angularLimitLowX);
            settings.angularLimitHighX = EditorGUILayout.FloatField("High X Limit", settings.angularLimitHighX);
            settings.angularLimitY = EditorGUILayout.FloatField("Y Limit", settings.angularLimitY);
            settings.angularLimitZ = EditorGUILayout.FloatField("Z Limit", settings.angularLimitZ);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }

    void DisplayAngularDrives(JointSettings settings)
    {
        string globalPrefix = "";
        if (setGlobalJointSetting) globalPrefix = "Global ";

        //Angular X Drive
        settings.showAngularXDriveInEditor = EditorGUILayout.Foldout(settings.showAngularXDriveInEditor, globalPrefix + "Angular X Drive", true);
        if (settings.showAngularXDriveInEditor || setGlobalJointSetting)
        {
            DisplayAngularDrivesHelper(settings, 0);
        }

        //Angular YZ Drive
        settings.showAngularYZDriveInEditor = EditorGUILayout.Foldout(settings.showAngularYZDriveInEditor, globalPrefix + "Angular YZ Drive", true);
        if (settings.showAngularYZDriveInEditor || setGlobalJointSetting)
        {
            DisplayAngularDrivesHelper(settings, 1);
        }
    }

    void DisplayAngularDrivesHelper(JointSettings settings, int index)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        EditorGUILayout.BeginVertical();

        //Angular X Drive
        if (index == 0)
        {
            settings.angularXDriveSpring = EditorGUILayout.FloatField("Spring", settings.angularXDriveSpring);
            settings.angularXDriveDamper = EditorGUILayout.FloatField("Damper", settings.angularXDriveDamper);
            settings.maxForceX = EditorGUILayout.FloatField("Maximum Force", settings.maxForceX);
        }
        else
        //Angular YZ Drive
        {
            settings.angularYZDriveSpring = EditorGUILayout.FloatField("Spring", settings.angularYZDriveSpring);
            settings.angularYZDriveDamper = EditorGUILayout.FloatField("Damper", settings.angularYZDriveDamper);
            settings.maxForceYZ = EditorGUILayout.FloatField("Maximum Force", settings.maxForceYZ);
        }

        if (setGlobalJointSetting)
        {
            angularXDriveSpringGlobal = settings.angularXDriveSpring;
            angularXDriveDamperGlobal = settings.angularXDriveDamper;
            maxForceXGlobal = settings.maxForceX;

            angularYZDriveSpringGlobal = settings.angularYZDriveSpring;
            angularYZDriveDamperGlobal = settings.angularYZDriveDamper;
            maxForceYZGlobal = settings.maxForceYZ;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// Shows values of the global JointSettings in the editor. These settings apply to all joints if setGlobalJointSettings is chosen.
    /// </summary>
    void DisplayGlobalJoint()
    {

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        DisplayJointSettingsOfBone(globalSettings);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

    }

    void UpdateTemplate()
    {
        //Joint Settings
        UpdateJoints();

        //Apply template changes to multiple joint template 
        CopyToMultipleJointsTemplate();

        //Refresh Values
        RefreshJointSettings();

        //editor.Repaint();
    }

    void RefreshJointSettings()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
        {
            ConfigurableJoint joint = gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>();
            if (joint != null) 
            {
                jointSettings[bone] = new JointSettings(bone, joint);
            }
        }
    }

    void CopyToMultipleJointsTemplate()
    {
        //we have to tell the JointSetup that we are using it in the context of the editor (we have no configjointmanager)
        JointSetup setup = new JointSetup(gameObjectsPerBoneTemplateMultiple, gameObjectsPerBoneTemplate, null, true);

        foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
        {
            //we destroy previous components of the multiple joints template to assure a clean copy of the component
            foreach (ConfigurableJoint joint in gameObjectsPerBoneTemplateMultiple[bone].GetComponents<ConfigurableJoint>())
            {
                DestroyImmediate(joint);
            }
            foreach (Collider collider in gameObjectsPerBoneTemplateMultiple[bone].GetComponents<Collider>())
            {
                DestroyImmediate(collider);
            }
            //setup.CopyPasteTemplateJoint(bone);
            setup.AddJointFromTemplate(bone);
        }
    }

    void UpdateJoints()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
        {
            ConfigurableJoint joint = gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                joint.enableCollision = useCollisions;
                joint.enablePreprocessing = !noPreprocessing;
                JointSettings settings = globalSettings;
                if (!setGlobalJointSetting)
                {
                    JointSettings specificSettings;
                    if (jointSettings.TryGetValue(bone, out specificSettings))
                    {
                        if (mirror)
                        {
                            if (specificSettings.bone.ToString().StartsWith("Left"))
                            {
                                specificSettings = jointSettings[LeftToRightMapping(bone)];
                            }
                            else
                            {
                                if (specificSettings.bone.ToString().StartsWith("Right"))
                                {
                                    specificSettings = jointSettings[RightToLeftMapping(bone)];
                                }
                            }
                        }
                        settings = specificSettings;
                    }
                }
                AddGravity(bone);
                ApplyJointSetting(joint, settings);
            }
        }
    }
    /// <summary>
    /// Returns the body part on the right side for a body part on the left side.
    /// </summary>
    /// <param name="bone">The body part on the left side.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns the body part on the right side for a body part on the left side.
    /// </summary>
    /// <param name="bone">The body part on the left side.</param>
    /// <returns></returns>
    HumanBodyBones RightToLeftMapping(HumanBodyBones bone)
    {
        switch (bone)
        {
            case HumanBodyBones.RightFoot: return HumanBodyBones.LeftFoot;
            case HumanBodyBones.RightHand: return HumanBodyBones.LeftHand;
            case HumanBodyBones.RightIndexDistal: return HumanBodyBones.LeftIndexDistal;
            case HumanBodyBones.RightIndexIntermediate: return HumanBodyBones.LeftIndexIntermediate;
            case HumanBodyBones.RightIndexProximal: return HumanBodyBones.LeftIndexProximal;
            case HumanBodyBones.RightLittleDistal: return HumanBodyBones.LeftLittleDistal;
            case HumanBodyBones.RightLittleIntermediate: return HumanBodyBones.LeftLittleIntermediate;
            case HumanBodyBones.RightLittleProximal: return HumanBodyBones.LeftLittleProximal;
            case HumanBodyBones.RightLowerArm: return HumanBodyBones.LeftLowerArm;
            case HumanBodyBones.RightLowerLeg: return HumanBodyBones.LeftLowerLeg;
            case HumanBodyBones.RightMiddleDistal: return HumanBodyBones.LeftMiddleDistal;
            case HumanBodyBones.RightMiddleIntermediate: return HumanBodyBones.LeftMiddleIntermediate;
            case HumanBodyBones.RightMiddleProximal: return HumanBodyBones.LeftMiddleProximal;
            case HumanBodyBones.RightRingDistal: return HumanBodyBones.LeftRingDistal;
            case HumanBodyBones.RightRingIntermediate: return HumanBodyBones.LeftRingIntermediate;
            case HumanBodyBones.RightRingProximal: return HumanBodyBones.LeftRingProximal;
            case HumanBodyBones.RightShoulder: return HumanBodyBones.LeftShoulder;
            case HumanBodyBones.RightThumbDistal: return HumanBodyBones.LeftThumbDistal;
            case HumanBodyBones.RightThumbIntermediate: return HumanBodyBones.LeftThumbIntermediate;
            case HumanBodyBones.RightThumbProximal: return HumanBodyBones.LeftThumbProximal;
            case HumanBodyBones.RightToes: return HumanBodyBones.LeftToes;
            case HumanBodyBones.RightUpperArm: return HumanBodyBones.LeftUpperArm;
            case HumanBodyBones.RightUpperLeg: return HumanBodyBones.LeftUpperLeg;
            default: return bone;
        }
    }

    void ApplyJointSetting(ConfigurableJoint joint, JointSettings setting)
    {
        Rigidbody rb = joint.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = setting.mass;
            rb.useGravity = setting.gravity;
            rb.centerOfMass = setting.centerOfMass;
            rb.inertiaTensor = setting.inertiaTensor;
        }
        //angular limits
        SoftJointLimit tmpLimit;

        tmpLimit = joint.lowAngularXLimit;
        tmpLimit.limit = setting.angularLimitLowX;
        joint.lowAngularXLimit = tmpLimit;

        tmpLimit = joint.highAngularXLimit;
        tmpLimit.limit = setting.angularLimitHighX;
        joint.highAngularXLimit = tmpLimit;

        tmpLimit = joint.angularYLimit;
        tmpLimit.limit = setting.angularLimitY;
        joint.angularYLimit = tmpLimit;

        tmpLimit = joint.angularZLimit;
        tmpLimit.limit = setting.angularLimitZ;
        joint.angularZLimit = tmpLimit;

        //joint drives
        JointDrive tmpDrive;

        tmpDrive = joint.angularXDrive;
        tmpDrive.positionSpring = setting.angularXDriveSpring;
        tmpDrive.positionDamper = setting.angularXDriveDamper;
        tmpDrive.maximumForce = setting.maxForceX;
        joint.angularXDrive = tmpDrive;

        tmpDrive = joint.angularYZDrive;
        tmpDrive.positionSpring = setting.angularYZDriveSpring;
        tmpDrive.positionDamper = setting.angularYZDriveDamper;
        tmpDrive.maximumForce = setting.maxForceYZ;
        joint.angularYZDrive = tmpDrive;

        tmpDrive.positionSpring = 0;
        tmpDrive.positionDamper = 0;
        tmpDrive.maximumForce = 0;

        joint.xDrive = joint.yDrive = joint.zDrive = tmpDrive;
    }

    void AddGravity(HumanBodyBones bone)
    {
        Rigidbody rb = gameObjectsPerBoneTemplate[bone].GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = useGravity;
        }
    }

    void SaveJointSettingsAsJson()
    {
        //save joint settings
        Leguar.TotalJSON.JSON json = new Leguar.TotalJSON.JSON(ConfigJointUtility.ConvertHumanBodyBonesKeyToStringJson(jointSettings));

        string values = json.CreatePrettyString();
        string path = "Assets/Client Physics/Scripts/Editor/Saved Settings/";

        path += (fileName.Length == 0 ? ("settings_" + System.DateTime.Now.ToString()) : fileName).Replace('/', '_').Replace(' ', '_').Replace(':', '_') + ".txt";

        File.WriteAllText(path, values);
        AssetDatabase.ImportAsset(path);
    }

    Dictionary<HumanBodyBones, JointSettings> RecoverJointSettingsFromJson(string savedInfo)
    {
        Leguar.TotalJSON.JSON json = Leguar.TotalJSON.JSON.ParseString(savedInfo);
        Dictionary<string, string> recoverdStringDictionary = json.Deserialize<Dictionary<string, string>>();
        Dictionary<HumanBodyBones, JointSettings> jointSettingsFromJson = new Dictionary<HumanBodyBones, JointSettings>();

        foreach (string key in recoverdStringDictionary.Keys)
        {
            jointSettingsFromJson.Add((HumanBodyBones)int.Parse(key), JsonUtility.FromJson<JointSettings>(recoverdStringDictionary[key]));
        }
        return jointSettingsFromJson;
    }

    void SelectJointsInTemplate()
    {
        HashSet<HumanBodyBones> selection = new HashSet<HumanBodyBones>();

        foreach (HumanBodyBones bone in toDisplay)
        {
            selection.Add(bone);
            if (mirror)
            {
                //since elements hashset are unique, no further check needed
                selection.Add(LeftToRightMapping(bone));
                selection.Add(RightToLeftMapping(bone));
            }
        }

        List<Object> objectsInTemplate = new List<Object>();

        foreach (HumanBodyBones bone in selection)
        {
            if (gameObjectsPerBoneTemplate.Keys.Contains(bone))
            {
                ConfigurableJoint joint = gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>();
                if (jointSettings.ContainsKey(bone) && joint != null)
                {
                    objectsInTemplate.Add(gameObjectsPerBoneTemplate[bone]);
                    jointSettings[bone] = new JointSettings(bone, joint);
                }
            }
        }
        Selection.objects = objectsInTemplate.ToArray();
    }
}
