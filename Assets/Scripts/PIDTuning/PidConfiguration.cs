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

        public readonly DateTime CreatedTimestamp;

        public readonly Dictionary<string, PidParameters> Parameters;

        public PidConfiguration(string name)
        {
            Name = name;
            CreatedTimestamp = DateTime.Now.ToUniversalTime();
            Parameters = new Dictionary<string, PidParameters>();
        }

        public void InitializePidParameters(IEnumerable<string> jointNames, PidParameters initializeValue)
        {
            // We need to make sure that noone changes PidParameters from a struct to a class
            // because the implicit copy-on-assign below will not work with classes
            Assert.IsTrue(typeof(PidParameters).IsValueType);

            foreach (var name in jointNames)
            {
                Parameters[name] = initializeValue;
            }
        }
    }
}