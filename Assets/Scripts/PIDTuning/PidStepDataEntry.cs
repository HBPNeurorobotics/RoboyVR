using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace PIDTuning
{
    /// <summary>
    /// A single entry of step data. Allows the user to
    /// add custom correlated additional data (like RTT etc.)
    /// </summary>
    public struct PidStepDataEntry
    {
        /// <summary>
        /// aka SP (Set Point)
        /// </summary>
        public readonly float Desired;

        /// <summary>
        /// aka PV (Process Variable)
        /// </summary>
        public readonly float Measured;

        /// <summary>
        /// Can be used to hold additional data that was collected during the
        /// timestep.
        /// </summary>
        private Dictionary<string, string> _correlatedData;

        public PidStepDataEntry(float desired, float measured)
        {
            Desired = desired;
            Measured = measured;
            _correlatedData = null;
        }

        public float SignedError
        {
            get { return Desired - Measured; }
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

        /// <summary>
        /// JSON representation has contains correlated data "flattened out"
        /// </summary>
        public JSONNode ToJson()
        {
            var json = new JSONNode();
            json["desired"].AsFloat = Desired;
            json["measured"].AsFloat = Desired;

            foreach (var cd in _correlatedData)
            {
                json[cd.Key] = cd.Value;
            }

            return json;
        }
    }
}