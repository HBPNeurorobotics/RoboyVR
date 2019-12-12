using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Adaption of PIDStepDataEntry for ConfigurableJoint Tuning
/// </summary>
public class QuaternionStepDataEntry {

    public readonly Quaternion desired;

    public readonly Quaternion measured;

    /// <summary>
    /// Can be used to hold additional data that was collected during the
    /// timestep.
    /// </summary>
    private Dictionary<string, string> _correlatedData;

    private QuaternionStepDataEntry(Quaternion desired, Quaternion measured)
    {
        this.desired = desired;
        this.measured = measured;
        _correlatedData = null;
    }

    public float SignedError
    {
        get { return Quaternion.Angle(measured, desired); }
    }

    public float AbsoluteError
    {
        get { return Mathf.Abs(SignedError); }
    }

    public void AddCorrelatedData(string key, string value)
    {
        if (null == _correlatedData)
        {
            // We initialize the Dictionary lazily to save resources
            // in case no correlated data exists
            _correlatedData = new Dictionary<string, string>();
        }

        _correlatedData[key] = value;
    }

    public string GetCorrelatedData(string key)
    {
        string entry;

        if (_correlatedData.TryGetValue(key, out entry))
        {
            return entry;
        }
        else
        {
            return null;
        }
    }

    public bool IsSimilarTo(QuaternionStepDataEntry other, float epsilon)
    {
        return Quaternion.Dot(desired, other.desired) < epsilon && Quaternion.Dot(measured, other.measured) < epsilon;
    }
}
