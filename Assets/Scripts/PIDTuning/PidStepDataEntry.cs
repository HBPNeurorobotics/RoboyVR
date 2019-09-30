using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
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
        /// aka SP (Set Point) in Degrees
        /// </summary>
        public readonly Vector3 Desired;

        /// <summary>
        /// aka PV (Process Variable) in Degrees
        /// </summary>
        public readonly Vector3 Measured;

        /// <summary>
        /// Can be used to hold additional data that was collected during the
        /// timestep.
        /// This is private because we want to do lazy initialization to save on
        /// resources in case that this dictionary is not used at all.
        /// If we ever update the version of .NET we are using, we can
        /// use the Lazy class for that purpose
        /// </summary>
        private Dictionary<string, string> _correlatedData;

        private PidStepDataEntry(Vector3 desired, Vector3 measured)
        {
            Desired = desired;
            Measured = measured;
            _correlatedData = null;
        }

        public static PidStepDataEntry FromRadians(Vector3 desired, Vector3 measured)
        {
            return new PidStepDataEntry(desired * Mathf.Rad2Deg, measured * Mathf.Rad2Deg);
        }

        public static PidStepDataEntry FromDegrees(Vector3 desired, Vector3 measured)
        {
            return new PidStepDataEntry(desired, measured);
        }

        public Vector3 SignedError
        {
            get {
                return new Vector3(
                    Mathf.DeltaAngle(Measured.x, Desired.x),
                    Mathf.DeltaAngle(Measured.y, Desired.y),
                    Mathf.DeltaAngle(Measured.z, Desired.z));
            }
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
        public JObject ToJson()
        {
            var json = new JObject();

            var desired = new JObject();
            desired["x"] = Desired.x;
            desired["y"] = Desired.y;
            desired["z"] = Desired.z;
            json["desired"] = desired;

            var measured = new JObject();
            measured["x"] = Measured.x;
            measured["y"] = Measured.y;
            measured["z"] = Measured.z;
            json["measured"] = measured;

            if (null != _correlatedData)
            {
                foreach (var cd in _correlatedData)
                {
                    json[cd.Key] = cd.Value;
                }
            }

            return json;
        }
    }
}