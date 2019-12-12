using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Adaption of PidConfiguration for ConfigurableJoint tuning
/// </summary>
public class ConfigurableJointConfiguration {
    public readonly DateTime CreatedTimestampUtc;

    public readonly Dictionary<HumanBodyBones, JointSettings> mapping;

    public ConfigurableJointConfiguration(DateTime createdTimestampUtc)
    {
        // We take the timestamp as a parameter here instead of generating it
        // ourselves. This allows the caller to re-use their timestamp for other
        // data structures, making sure that they match exactly

        // But let's just make sure they are smart enough to use universal time.
        Assert.AreEqual(DateTimeKind.Utc, createdTimestampUtc.Kind);

        CreatedTimestampUtc = createdTimestampUtc;
        mapping = new Dictionary<HumanBodyBones, JointSettings>();
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConfigurableJointConfiguration(ConfigurableJointConfiguration copySource)
    {
        CreatedTimestampUtc = copySource.CreatedTimestampUtc;
        mapping = new Dictionary<HumanBodyBones, JointSettings>(copySource.mapping);
    }

    public void InitializeMapping(IEnumerable<HumanBodyBones> bones, JointSettings initializeValue)
    {
        mapping.Clear();

        foreach (HumanBodyBones bone in bones)
        {
            initializeValue.bone = bone;
            mapping[bone] = initializeValue;
        }
    }
}
