﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EditorStorage {

    public float bodyWeight;
    public BodyMass.MODE mode;

    public bool useCollisions;
    public bool noPreprocessing;
    public bool setGlobalJointSetting;
    public bool mirror;
    public bool showJointSettings;
    public bool showGlobalJointSettings;
    public bool useGravity;

    public float angularXDriveSpringGlobal;
    public float angularXDriveDamperGlobal;
    public float maxForceXGlobal;

    public float angularYZDriveSpringGlobal;
    public float angularYZDriveDamperGlobal;
    public float maxForceYZGlobal;

    public BodyGroups.BODYGROUP bodyGroup;

    public JointSettings globalSettings;

    public Dictionary<HumanBodyBones, JointSettings> jointSettings = new Dictionary<HumanBodyBones, JointSettings>();
    public Dictionary<HumanBodyBones, JointSettings> jointSettingsNoLeft = new Dictionary<HumanBodyBones, JointSettings>();

    public BodyMass bodyMass;
}
