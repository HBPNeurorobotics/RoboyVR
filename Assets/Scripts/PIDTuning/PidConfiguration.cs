using System;
using System.Collections;
using System.Collections.Generic;
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
        public readonly string Name;

        public readonly DateTime CreatedTimestampUtc;

        public readonly Dictionary<string, PidParameters> Parameters;

        public PidConfiguration(string name, DateTime createdTimestampUtc)
        {
            // We take the timestamp as a parameter here instead of generating it
            // ourselves. This allows the caller to re-use their timestamp for other
            // data structures, making sure that they match exactly

            // But let's just make sure they are smart enough to use universal time.
            Assert.AreEqual(DateTimeKind.Utc, createdTimestampUtc.Kind);

            Name = name;
            CreatedTimestampUtc = createdTimestampUtc;
            Parameters = new Dictionary<string, PidParameters>();
        }

        public void InitializePidParameters(IEnumerable<string> jointNames, PidParameters initializeValue)
        {
            // We need to make sure that no one changes PidParameters from a struct to a class
            // because the implicit copy-on-assign below will not work with classes
            Assert.IsTrue(typeof(PidParameters).IsValueType);

            foreach (var name in jointNames)
            {
                Parameters[name] = initializeValue;
            }
        }
    }
}