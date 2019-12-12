using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
/// <summary>
/// Adaption of PidStepData for ConfigurableJoint Tuning
/// </summary>
public class QuaternionStepData {
    public readonly DateTime createdTimestampUtc;

    public readonly SortedList<DateTime, QuaternionStepDataEntry> data;

    public readonly Dictionary<string, string> AdditionalKeys;

    public QuaternionStepData(DateTime createdTimestampUtc)
    {
        Assert.AreEqual(DateTimeKind.Utc, createdTimestampUtc.Kind);

        this.createdTimestampUtc = createdTimestampUtc;
        data = new SortedList<DateTime, QuaternionStepDataEntry>();

        AdditionalKeys = new Dictionary<string, string>();
    }
}
