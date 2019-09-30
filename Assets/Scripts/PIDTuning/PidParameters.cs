using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using UnityEngine;

namespace PIDTuning
{
    /// <summary>
    /// Gain parameters for a PID controller
    /// Available in both standard and parallel form
    /// </summary>
    public struct PidParameters
    {
        public readonly float Kp, Ki, Kd;

        public float Ti
        {
            get { return Kp / Ki; }
        }

        public float Td
        {
            get { return Kd / Kp; }
        }

        private PidParameters(float kp, float ki, float kd)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
        }

        public static PidParameters FromParallelForm(float kp, float ki, float kd)
        {
            return new PidParameters(kp, ki, kd);
        }

        public static PidParameters FromStandardForm(float kp, float ti, float td)
        {
            return new PidParameters(
                kp,
                kp / ti,
                kp * td
            );
        }

        public JObject ToJson()
        {
            var json = new JObject();

            json["kp"] = Kp;
            json["ki"] = Ki;
            json["kd"] = Kd;

            return json;
        }
    }
}
