using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    /// <summary>
    /// A collection of discrete step-data entries for a
    /// control loop. Provides helper functions to calculate
    /// accumulate errors.
    /// </summary>
    public class PidStepData
    {
        public readonly DateTime CreatedTimestampUtc;

        public readonly SortedList<DateTime, PidStepDataEntry> Data;

        public readonly Dictionary<string, string> AdditionalKeys;

        public PidStepData(DateTime createdTimestampUtc)
        {
            // We take the timestamp as a parameter here instead of generating it
            // ourselves. This allows the caller to re-use their timestamp for other
            // data structures, making sure that they match exactly

            // But let's just make sure they are smart enough to use universal time.
            Assert.AreEqual(DateTimeKind.Utc, createdTimestampUtc.Kind);

            CreatedTimestampUtc = createdTimestampUtc;
            Data = new SortedList<DateTime, PidStepDataEntry>();

            AdditionalKeys = new Dictionary<string, string>();
        }

        public JObject ToJson()
        {
            var json = new JObject();

            json["createdTimestamp"] = CreatedTimestampUtc.ToFileTimeUtc().ToString();

            foreach (var kvPair in AdditionalKeys)
            {
                json[kvPair.Key] = kvPair.Value;
            }

            var dataArray = new JArray();

            int i = 0;
            foreach (var entry in Data)
            {
                var node = new JObject();
                node["timestamp"] = entry.Key.ToFileTimeUtc().ToString();
                node["entry"] = entry.Value.ToJson();

                dataArray.Add(node);
            }

            json["data"] = dataArray;

            return json;
        }
    }
}