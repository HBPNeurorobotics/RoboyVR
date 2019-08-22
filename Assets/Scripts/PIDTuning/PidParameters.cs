using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PIDTuning
{
    /// <summary>
    /// Gain parameters for a PID controller
    /// Available in both standard and parallel form
    /// </summary>
    public struct PidParameters
    {
        public readonly double Kp, Ki, Kd;

        public double Ti
        {
            get { return Kp / Ki; }
        }

        public double Td
        {
            get { return Kd / Kp; }
        }

        private PidParameters(double kp, double ki, double kd)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
        }

        public static PidParameters FromParallelForm(double kp, double ki, double kd)
        {
            return new PidParameters(kp, ki, kd);
        }

        public static PidParameters FromStandardForm(double kp, double ti, double td)
        {
            return new PidParameters(
                kp,
                kp / ti,
                kp * td
            );
        }
    }
}
