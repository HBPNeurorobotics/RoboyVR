using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

[ExecuteInEditMode]
public class EditAvatarTemplate : EditorWindow
{
    #region Configurable in Editor
    Object template;
    Object templateMultiple;

    TextAsset savedEditor;
    TextAsset tunedSettings;

    float bodyWeight = 72f;
    BodyMass.MODE mode = BodyMass.MODE.AVERAGE;
    float indent = 15f;

    float maxForceTuning = 2500;

    BodyGroups.BODYGROUP bodyGroup;

    bool useCollisions;
    bool noPreprocessing = true;
    bool setGlobalJointSetting = false;
    bool selectGroupConfigJoints = false;
    bool mirror;
    bool showJointSettings = true;
    bool useGravity = true;
    bool loadFromTuning = false;
    #endregion

    #region Helper variables
    public float angularXDriveSpringGlobal;
    public float angularXDriveDamperGlobal;
    public float maxForceXGlobal;

    public float angularYZDriveSpringGlobal;
    public float angularYZDriveDamperGlobal;
    public float maxForceYZGlobal;
    JointSettings globalSettings;

    static EditAvatarTemplate editor;

    Vector2 scroll;

    BodyMass bodyMass;

    string fileName = "";
    bool loadFromFile;
    bool gatheredTemplateSettings;
    bool gatheredTuningSettings;
    bool restoredAvatarSettings;
    #endregion

    #region Dictionaries and HashSets
    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBoneTemplate = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBoneTemplateMultiple = new Dictionary<HumanBodyBones, GameObject>();

    Dictionary<HumanBodyBones, Dictionary<string,JointSettings>> jointSettings = new Dictionary<HumanBodyBones, Dictionary<string, JointSettings>>();

    HashSet<HumanBodyBones> jointsLeft = new HashSet<HumanBodyBones>();
    HashSet<HumanBodyBones> jointsRight = new HashSet<HumanBodyBones>();
    HashSet<HumanBodyBones> toDisplay = new HashSet<HumanBodyBones>();

    #endregion

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
        savedEditor = (TextAsset)EditorGUILayout.ObjectField("Avatar Template Settings", savedEditor, typeof(TextAsset), true);
        fileName = EditorGUILayout.TextField("New Name / Overwrite", fileName);

        EditorGUILayout.BeginHorizontal();
        if (savedEditor == null)
        {
            GUI.enabled = false;
            loadFromFile = false;
            //if not done so already, empty previously loaded settings (we do not want to execute in every editor update)
            if (!gatheredTemplateSettings)
            {
                //set clean editor
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
        GUI.enabled = true;
        loadFromTuning = EditorGUILayout.Toggle("Load from Tuning", loadFromTuning);
        
        if (loadFromTuning)
        {
            GUI.enabled = true;
        }
        else
        {
            GUI.enabled = false;
        }
        

        tunedSettings = (TextAsset)EditorGUILayout.ObjectField("Tuned Settings", tunedSettings, typeof(TextAsset), true);
        maxForceTuning = EditorGUILayout.FloatField("Maximum Force", maxForceTuning);
        GUI.enabled = true;
        #endregion


        if (template != null && templateMultiple != null)
        {

            scroll = EditorGUILayout.BeginScrollView(scroll);
            #region Editor Parameters

            EditorGUILayout.BeginVertical();
            bodyWeight = EditorGUILayout.FloatField("Total Body Weight", bodyWeight);
            mode = (BodyMass.MODE)EditorGUILayout.EnumPopup("Population Group of Avatar", mode);

            bodyGroup = (BodyGroups.BODYGROUP)EditorGUILayout.EnumPopup("Body Group", bodyGroup);

            Initialize();

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
            if (selectGroupConfigJoints && !loadFromTuning)
            {
                SelectJointsInTemplate();
            }

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

            if (GUILayout.Button("Set BodyMass"))
            {
                bodyMass = new BodyMass(bodyWeight, gameObjectsPerBoneTemplate, mode);
                bodyMass.SetBodyMasses();

                foreach (HumanBodyBones bone in jointSettings.Keys)
                {
                    if (gameObjectsPerBoneTemplate.ContainsKey(bone))
                    {
                        //mass is the same for all joints of one body part
                        foreach (JointSettings settings in jointSettings[bone].Values) {
                        settings.mass = gameObjectsPerBoneTemplate[bone].GetComponent<Rigidbody>().mass;
        }
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
                        foreach (JointSettings settings in jointSettings[bone].Values)
                        {
                            settings.mass = gameObjectsPerBoneTemplate[bone].GetComponent<Rigidbody>().mass;
                        }
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
            GUI.enabled = true;
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
    /// <summary>
    /// Initializes dictionaries and hashsets.
    /// </summary>
    void Initialize()
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

        if (loadFromTuning && tunedSettings != null)
        {
            if (!gatheredTuningSettings)
            {
                jointSettings = RecoverJointSettingsFromJson(tunedSettings.text);
                gatheredTuningSettings = true;
                restoredAvatarSettings = false;
            }
        }
        else
        {
            gatheredTuningSettings = false;
            if (!restoredAvatarSettings)
            {
                jointSettings.Clear();
                foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
                {
                    GetJointSettingsFromAvatarTemplate(bone);
                }
                restoredAvatarSettings = true;
            }
        }

    }

    /// <summary>
    /// Initializes the JointSettings of bone according to values found in AvatarTemplate.
    /// </summary>
    /// <param name="bone"></param>
    /// <param name="animator"></param>
    /// <param name="animatorMultiple"></param>
    /// <param name="chosenBones">The bones that will be displayed in the editor.</param>
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
                GetJointSettingsFromAvatarTemplate(bone);
            }
        }

        //handle current display
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
    void GetJointSettingsFromAvatarTemplate(HumanBodyBones bone)
    {
        Debug.Log("FromAvatar");
        //JointSettings will only be set to the values in AvatarTemplate once (when both AvatarTemplate and AvatarTemplateMultipleJoint rigs have been assigned).
        //prevents constantly overwriting the JointSettings with every editor update.
        if (!jointSettings.ContainsKey(bone))
        {
            ConfigurableJoint joint = gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>();

            Dictionary<string, JointSettings> dict = new Dictionary<string, JointSettings>();
            if(joint != null)
            {
                dict.Add(bone.ToString(), new JointSettings(bone, joint));

                if (bone.ToString().StartsWith("Left"))
                {
                    jointsLeft.Add(bone);
                }
                if (bone.ToString().StartsWith("Right"))
                {
                    jointsRight.Add(bone);
                }
            }
            jointSettings.Add(bone, dict);
        }
    }
    /// <summary>
    /// Shows all editable JointSettings in Editor.
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

            foreach (JointSettings settings in jointSettings[bone].Values)
            {
                DisplayJointSettingsOfBone(settings);
            }

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
        boneSettings.showInEditor = EditorGUILayout.Foldout(boneSettings.showInEditor, setGlobalJointSetting ? "Global Joint" : boneSettings.jointName, true);

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
        if (settings.bone.ToString().StartsWith("Left")) CopySettings(settings, jointSettings[LeftToRightMapping(settings.bone)][settings.jointName.Replace("Left", "Right")]);
        if (settings.bone.ToString().StartsWith("Right")) CopySettings(settings, jointSettings[RightToLeftMapping(settings.bone)][settings.jointName.Replace("Right", "Left")]);
    }
    /// <summary>
    /// Mapps angular limits and angular drives of one JointSettings instance to another one.
    /// </summary>
    /// <param name="settings">The settings to copy.</param>
    /// <param name="mappedSettings">The settings to overwrite.</param>
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
        Debug.Log("DisplayJointValues " + settings.jointName);
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
            Debug.Log("Drive");
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
            //RefreshJointSettings();

        //editor.Repaint();
    }
    /*
    /// <summary>
    /// Makes sure that the correct applied values are assigned.
    /// </summary>
    void RefreshJointSettings()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
        {
            ConfigurableJoint[] joints = gameObjectsPerBoneTemplate[bone].GetComponents<ConfigurableJoint>();
            Dictionary<string, JointSettings> settings = new Dictionary<string, JointSettings>();
           foreach (ConfigurableJoint joint in joints) 
            {
                settings.Add( new JointSettings(bone, joint));
            }
            jointSettings.Add(bone, settings);
        }
    }
    */
    /// <summary>
    /// Called on update press to copy changes made to the TemplateAvatar to the TemplateAvatarMutlipleJoints. Utilizes JointSetup functions.
    /// </summary>
    void CopyToMultipleJointsTemplate()
    {
        //we have to tell the JointSetup that we are using it in the context of the editor (we have no configjointmanager)
        JointSetup setup = new JointSetup(gameObjectsPerBoneTemplateMultiple, gameObjectsPerBoneTemplate, null, true);
        Dictionary<HumanBodyBones, Dictionary<string, JointSettings>> fromTuningRaw = null;

        if (loadFromTuning && tunedSettings != null)
        {
             fromTuningRaw = RecoverJointSettingsFromJson(tunedSettings.text);
        }

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

            /*
            if (fromTuningRaw != null)
            {  
                List<ConfigurableJoint> untunedJoints = new List<ConfigurableJoint>();
                foreach (ConfigurableJoint addedJoint in gameObjectsPerBoneTemplateMultiple[bone].GetComponents<ConfigurableJoint>())
                {
                    untunedJoints.Add(addedJoint);
                }

                foreach(ConfigurableJoint toTune in untunedJoints)
                {
                    foreach(JointSettings copyFrom in fromTuningRaw[bone].Keys)
                    {
                        if (copyFrom.primaryAxis == toTune.axis)
                        {
                            JointDrive drive = toTune.angularXDrive;
                            float scale = GetScaleOfSpringForce(copyFrom.angularXDriveSpring);
                            drive.positionSpring = copyFrom.angularXDriveSpring;
                            drive.positionDamper = copyFrom.angularXDriveDamper;
                            drive.maximumForce = maxForceTuning;
                            toTune.angularXDrive = drive;
                        }
                    }
                }
            }
            */
        }
    }

    float GetScaleOfSpringForce(float spring)
    {
        return 2 * spring / maxForceTuning;
    }

    /// <summary>
    /// Applys changes made to the ConfigurableJoints to the TemplateAvatar.
    /// </summary>
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
                    //get the correct JointSettings for the current bone
                    Dictionary<string, JointSettings> specificSettings;
                    if (jointSettings.TryGetValue(bone, out specificSettings))
                    {
                        foreach (string jointName in specificSettings.Keys)
                        {
                            string settingsKey = jointName;
                            //return bone on the right if left side
                            if (mirror)
                            {
                                if (jointName.StartsWith("Left"))
                                {
                                    settingsKey = jointName.Replace("Left", "Right");
                                    specificSettings = jointSettings[LeftToRightMapping(bone)];
                                }
                                //return bone on the left if right side
                                else
                                {
                                    if (jointName.StartsWith("Right"))
                                    {
                                        settingsKey = jointName.Replace("Right", "Left");
                                        specificSettings = jointSettings[RightToLeftMapping(bone)];
                                    }
                                }
                            }
                            settings = specificSettings[settingsKey];
                            ApplyJointSetting(joint, settings);
                        }
                    }
                }
                else
                {
                    ApplyJointSetting(joint, settings);
                }
                AddGravity(bone);
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
    /// <summary>
    /// Applys a JointSettings configuration to a specified joint.
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="setting"></param>
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
    /// <summary>
    /// Saves the current JointSettings for all bones.
    /// </summary>
    void SaveJointSettingsAsJson()
    {
        //save joint settings
        string values = "";
        foreach (HumanBodyBones bone in jointSettings.Keys)
        {
            values += ConfigJointUtility.ConvertDictionaryToJson(jointSettings[bone]) + "\n";
        }
        values = values.Substring(0, values.Length - 1);

        string path = "Assets/Client Physics/Scripts/Editor/Saved Settings/";

        path += (fileName.Length == 0 ? ("settings_" + System.DateTime.Now.ToString()) : fileName).Replace('/', '_').Replace(' ', '_').Replace(':', '_') + ".txt";

        File.WriteAllText(path, values);
        AssetDatabase.ImportAsset(path);
    }

    /// <summary>
    /// Load JointSettings from Json.
    /// </summary>
    /// <param name="savedInfo">The text inside of the assigned previously saved file.</param>
    /// <returns></returns>
    Dictionary<HumanBodyBones, Dictionary<string, JointSettings>> RecoverJointSettingsFromJson(string savedInfo)
    {
        Dictionary<HumanBodyBones, Dictionary<string, JointSettings>> jointSettingsFromJson = new Dictionary<HumanBodyBones, Dictionary<string, JointSettings>>();

        //prepare Dictionary
        foreach (HumanBodyBones bone in gameObjectsPerBoneTemplateMultiple.Keys)
        {
            jointSettingsFromJson.Add(bone, new Dictionary<string, JointSettings>());
        }

        string[] settingsEntries = savedInfo.Split('\n');

        foreach (string entry in settingsEntries)
        {
            //recover JointSettings
            JointSettings joint = JsonUtility.FromJson<JointSettings>(entry);

            Dictionary<string, JointSettings> jointsWithSameBone;
            if (jointSettingsFromJson.TryGetValue(joint.bone, out jointsWithSameBone))
            {
                jointSettingsFromJson[joint.bone].Add(joint.jointName, joint);
            }
        }

        return jointSettingsFromJson;
    }
    /*
    Dictionary<HumanBodyBones, List<JointSettings>> RecoverJointSettingsFromTuning(string savedInfo)
    {
        Dictionary<HumanBodyBones, List<JointSettings>> jointSettingsFromJson = new Dictionary<HumanBodyBones, List<JointSettings>>();

        //prepare Dictionary
        foreach(HumanBodyBones bone in gameObjectsPerBoneTemplateMultiple.Keys)
        {
            jointSettingsFromJson.Add(bone, new List<JointSettings>());
        }

        string[] settingsEntries = savedInfo.Split('\n');
        foreach (string entry in settingsEntries)
        {
            //recover JointSettings
            JointSettings joint = JsonUtility.FromJson<JointSettings>(entry);

            List<JointSettings> jointsWithSameBone;
            if(jointSettingsFromJson.TryGetValue(joint.bone, out jointsWithSameBone))
            {
                jointSettingsFromJson[joint.bone].Add(joint);
            }
        }

        return jointSettingsFromJson;
    }
    */

    /// <summary>
    /// Selects the Objects of the respective joints in the TemplateAvatar to make the same changes to all of them.
    /// </summary>
    void SelectJointsInTemplate()
    {
        HashSet<HumanBodyBones> selection = new HashSet<HumanBodyBones>();

        foreach (HumanBodyBones bone in toDisplay)
        {
            selection.Add(bone);
            if (mirror)
            {
                //since elements in hashsets are unique, no further check needed
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
                    //update the JointSettings to store changes in file if needed.
                    jointSettings[bone][bone.ToString()] = new JointSettings(bone, joint);
                }
            }
        }
        Selection.objects = objectsInTemplate.ToArray();
    }
}
