using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    /// <summary>
    /// A mapping from joint names to PID parameters
    /// Provides facilities for (de)serialization
    /// </summary>
    public class PidConfiguration
    {
        public readonly DateTime CreatedTimestampUtc;

        public readonly Dictionary<string, PidParameters> Mapping;

        public PidConfiguration(DateTime createdTimestampUtc)
        {
            // We take the timestamp as a parameter here instead of generating it
            // ourselves. This allows the caller to re-use their timestamp for other
            // data structures, making sure that they match exactly

            // But let's just make sure they are smart enough to use universal time.
            Assert.AreEqual(DateTimeKind.Utc, createdTimestampUtc.Kind);

            CreatedTimestampUtc = createdTimestampUtc;
            Mapping = new Dictionary<string, PidParameters>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public PidConfiguration(PidConfiguration copySource)
        {
            CreatedTimestampUtc = copySource.CreatedTimestampUtc;
            Mapping = new Dictionary<string, PidParameters>(copySource.Mapping);
        }

        public void InitializeMapping(IEnumerable<string> jointNames, PidParameters initializeValue)
        {
            Mapping.Clear();

            foreach (var name in jointNames)
            {
                Mapping[name] = new PidParameters(initializeValue);
            }
        }

        public JObject ToJson()
        {
            var json = new JObject();

            json["createdTimestamp"] = CreatedTimestampUtc.ToFileTimeUtc().ToString();
            
            var mappingJson = new JObject();

            foreach (var jointPidMapping in Mapping)
            {
                mappingJson[jointPidMapping.Key] = jointPidMapping.Value.ToJson();
            }

            json["mapping"] = mappingJson;

            return json;
        }

        public static PidConfiguration FromJson(string jsonText)
        {
            var json = JObject.Parse(jsonText);

            var config = new PidConfiguration(json["createdTimestamp"].Value<DateTime>());

            foreach (var jointToken in json["mapping"])
            {
                if (jointToken.Type == JTokenType.Property)
                {
                    config.Mapping.Add(jointToken.Value<string>(), PidParameters.FromParallelForm(
                        kp: jointToken["kp"].Value<float>(),
                        ki: jointToken["ki"].Value<float>(),
                        kd: jointToken["kd"].Value<float>()));
                }
            }

            return config;
        }
    }
}