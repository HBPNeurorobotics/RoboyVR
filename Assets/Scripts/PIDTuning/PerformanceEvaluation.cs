using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning
{
    public class PerformanceEvaluation
    {
        public readonly DateTime TimeStamp;

        /// <summary>
        /// The absolute error value at each sample, divided by the number of samples.
        /// This is a very good indicator of tracking performance.
        /// </summary>
        public readonly float AvgAbsoluteError;

        /// <summary>
        /// The signed error value at each sample, divided by the number of samples.
        /// A non-zero value indicates that the PID controller is trying to control
        /// a non-symmetric process (e.g. working in the direction of gravity, or against it).
        /// If this value is high enough, think about using two PID controllers instead of one.
        /// </summary>
        public readonly float AvgSignedError;

        /// <summary>
        /// Keep in mind that a high maximum absolute error can also be caused
        /// by large set-point change, not only by poor tracking performance.
        /// </summary>
        public readonly float MaxAbsoluteError;

        // The metrics below can be null because they can only be reliably measured if the set-point stays
        // constant for at least 1 oscillation after a set-point change. This scenario is not always given.

        public readonly float? MaxOvershoot;

        // These metrics measure how fast the PV settles after a set-point change. The percentages describe
        // the leniency of the metric: The ..10Percent metric considers the PV settled if it oscillates no
        // more than 10 percent of the distance between the old set-point and the current set-point.

        /// <summary>
        /// How fast does PV settle within 10% of a new set-point after a change?
        /// </summary>
        public readonly float? AvgSettlingTime10Percent;

        /// <summary>
        /// How fast does PV settle within 5% of a new set-point after a change?
        /// </summary>
        public readonly float? AvgSettlingTime5Percent;

        /// <summary>
        /// How fast does PV settle within 2% of a new set-point after a change?
        /// </summary>
        public readonly float? AvgSettlingTime2Percent;

        /// <summary>
        /// How fast does the PV reach 10% of the way toward a new set-point after a change?
        /// </summary>
        public readonly float? Avg10PercentResponseTime;

        /// <summary>
        /// How fast does the PV reach 50% of the way toward a new set-point after a change?
        /// </summary>
        public readonly float? Avg50PercentResponseTime;

        /// <summary>
        /// How fast does the PV reach a new set-point after a change (ignoring any entailing overshoot)?
        /// </summary>
        public readonly float? AvgCompleteResponseTime;

        public PerformanceEvaluation(DateTime timeStamp, float avgAbsoluteError, float avgSignedError, float maxAbsoluteError, float? maxOvershoot, float? avgSettlingTime10Percent, float? avgSettlingTime5Percent, float? avgSettlingTime2Percent, float? avg10PercentResponseTime, float? avg50PercentResponseTime, float? avgCompleteResponseTime)
        {
            TimeStamp = timeStamp;
            AvgAbsoluteError = avgAbsoluteError;
            AvgSignedError = avgSignedError;
            MaxAbsoluteError = maxAbsoluteError;
            MaxOvershoot = maxOvershoot;
            AvgSettlingTime10Percent = avgSettlingTime10Percent;
            AvgSettlingTime5Percent = avgSettlingTime5Percent;
            AvgSettlingTime2Percent = avgSettlingTime2Percent;
            Avg10PercentResponseTime = avg10PercentResponseTime;
            Avg50PercentResponseTime = avg50PercentResponseTime;
            AvgCompleteResponseTime = avgCompleteResponseTime;
        }

        public static PerformanceEvaluation FromStepData(DateTime timestamp, PidStepData stepData)
        {
            // Can only create evaluation if we have at least one sample
            Assert.IsTrue(stepData.Data.Count > 0);

            float avgAbsoluteError;
            float avgSignedError;
            float maxAbsoluteError;

            CalculateSimpleMetrics(
                stepData: stepData,
                avgAbsoluteError: out avgAbsoluteError,
                avgSignedError: out avgSignedError,
                maxAbsoluteError: out maxAbsoluteError);

            float? maxOvershoot;

            float? avgSettlingTime10Percent;
            float? avgSettlingTime5Percent;
            float? avgSettlingTime2Percent;

            float? avg10PercentResponseTime;
            float? avg50PercentResponseTime;
            float? avgCompleteResponseTime;

            CalculateResponseMetrics(
                stepData: stepData,
                maxOvershoot: out maxOvershoot,
                avgSettlingTime10Percent: out avgSettlingTime10Percent,
                avgSettlingTime5Percent: out avgSettlingTime5Percent,
                avgSettlingTime2Percent: out avgSettlingTime2Percent,
                avg10PercentResponseTime: out avg10PercentResponseTime,
                avg50PercentResponseTime: out avg50PercentResponseTime,
                avgCompleteResponseTime: out avgCompleteResponseTime);

            return new PerformanceEvaluation(timestamp,
                avgAbsoluteError: avgAbsoluteError, avgSignedError: avgSignedError, maxAbsoluteError: maxAbsoluteError,
                maxOvershoot: maxOvershoot,
                avgSettlingTime10Percent: avgSettlingTime10Percent, avgSettlingTime5Percent: avgSettlingTime5Percent, avgSettlingTime2Percent: avgSettlingTime2Percent,
                avg10PercentResponseTime: avg10PercentResponseTime, avg50PercentResponseTime: avg50PercentResponseTime, avgCompleteResponseTime: avgCompleteResponseTime);
        }

        /// <summary>
        /// Calculates all metrics that don't have requirements w.r.t. set-point stability over time
        /// </summary>
        private static void CalculateSimpleMetrics(PidStepData stepData, out float avgAbsoluteError, out float avgSignedError, out float maxAbsoluteError)
        {
            avgAbsoluteError = 0f;
            avgSignedError = 0f;
            maxAbsoluteError = 0f;

            var count = 0;

            foreach (var entry in stepData.Data.Values)
            {
                float absError = Mathf.Abs(entry.SignedError);

                avgAbsoluteError += absError;
                avgSignedError += entry.SignedError;
                maxAbsoluteError = Mathf.Max(absError, maxAbsoluteError);

                count++;
            }

            avgAbsoluteError /= (float) count;
            avgSignedError /= (float) count;
        }

        /// <summary>
        /// Calculate metrics that require the set-point to hold constant for a while after a change
        /// </summary>
        private static void CalculateResponseMetrics(PidStepData stepData, 
            out float? maxOvershoot,
            out float? avgSettlingTime10Percent, out float? avgSettlingTime5Percent, out float? avgSettlingTime2Percent,
            out float? avg10PercentResponseTime, out float? avg50PercentResponseTime, out float? avgCompleteResponseTime)
        {
            maxOvershoot = null;
            avgSettlingTime10Percent = null;
            avgSettlingTime5Percent = null;
            avgSettlingTime2Percent = null;
            avg10PercentResponseTime = null;
            avg50PercentResponseTime = null;
            avgCompleteResponseTime = null;

            KeyValuePair<DateTime, PidStepDataEntry>? oldSetpoint = null;

            // Hold a streak of entries where the step-data is constant
            var currentStreak = new List<KeyValuePair<DateTime, PidStepDataEntry>>();

            foreach (var datedEntry in stepData.Data)
            {
                if (null == oldSetpoint)
                {
                    // We don't have any initial old set-point

                    oldSetpoint = datedEntry;

                    continue;
                }

                if (datedEntry.Value.Desired != oldSetpoint.Value.Value.Desired)
                {
                    // We have a new set-point that is different from the current set-point

                    oldSetpoint = datedEntry;

                    if (currentStreak.Count >= 2)
                    {
                        // Evaluate the current streak, if it is actually a streak (meaning more than 2 elements)
                        CalculateMaxOvershoot(currentStreak, ref maxOvershoot);
                        CalculateSettlingTime(currentStreak, ref avgSettlingTime10Percent, ref avgSettlingTime5Percent,
                            ref avgSettlingTime2Percent);
                        CalculateResponseTime(currentStreak, ref avg10PercentResponseTime, ref avg50PercentResponseTime,
                            ref avgCompleteResponseTime);
                    }

                    // Start a new streak, beginning with the first entry that has the new set-point
                    currentStreak.Clear();
                    currentStreak.Add(datedEntry);

                    continue;
                }

                if (currentStreak.Count > 0)
                {
                    // If we have a streak going, add the entry
                    currentStreak.Add(datedEntry);
                }
            }

            if (currentStreak.Count >= 2)
            {
                // Evaluate the current streak, if it is actually a streak (meaning more than 2 elements)
                CalculateMaxOvershoot(currentStreak, ref maxOvershoot);
                CalculateSettlingTime(currentStreak, ref avgSettlingTime10Percent, ref avgSettlingTime5Percent,
                    ref avgSettlingTime2Percent);
                CalculateResponseTime(currentStreak, ref avg10PercentResponseTime, ref avg50PercentResponseTime,
                    ref avgCompleteResponseTime);
            }
        }

        private static void CalculateMaxOvershoot(List<KeyValuePair<DateTime, PidStepDataEntry>> currentStreak, 
            ref float? maxOvershoot)
        {
            // Max Overshoot can be calculated iff PV crosses the line/value given by a constant SP at least once

            // We first determine if PV will be crossing from below or above SP:
            var first = currentStreak.First().Value;
            var pvToSpSign = Mathf.Sign(first.Desired - first.Measured);

            // We can't figure out overshoot if we start with PV == SP, since any possible
            // overshoot in that case can only come from prior SP changes and should thus be ignored
            if (first.Measured == first.Desired)
            {
                return;
            }

            // Now we iterate over the streak and see if we find any crossing.
            // If yes, we update maxOvershoot accordingly

            bool hasCrossed = false;

            foreach (var entry in currentStreak)
            {
                var diff = entry.Value.Desired - entry.Value.Measured;

                hasCrossed = hasCrossed || Mathf.Sign(diff) != pvToSpSign;

                if (hasCrossed)
                {
                    if (null == maxOvershoot)
                    {
                        maxOvershoot = Mathf.Abs(diff);
                    }
                    else
                    {
                        maxOvershoot = Mathf.Max(maxOvershoot.Value, Mathf.Abs(diff));
                    }
                }
            }
        }

        private static void CalculateSettlingTime(List<KeyValuePair<DateTime, PidStepDataEntry>> currentStreak, 
            ref float? avgSettlingTime10Percent, ref float? avgSettlingTime5Percent, ref float? avgSettlingTime2Percent)
        {
            //throw new NotImplementedException();
        }

        private static void CalculateResponseTime(List<KeyValuePair<DateTime, PidStepDataEntry>> currentStreak, 
            ref float? avg10PercentResponseTime, ref float? avg50PercentResponseTime, ref float? avgCompleteResponseTime)
        {
            //throw new NotImplementedException();
        }

        public JObject ToJson()
        {
            var json = new JObject();

            json["createdTimestamp"] = TimeStamp.ToFileTimeUtc().ToString();
            json["avgAbsoluteError"] = AvgAbsoluteError;
            json["avgSignedError"] = AvgSignedError;
            json["maxAbsoluteError"] = MaxAbsoluteError;

            if (MaxOvershoot.HasValue)
            {
                json["maxOvershoot"] = MaxOvershoot.Value;
            }

            if (AvgSettlingTime10Percent.HasValue)
            {
                json["avgSettlingTime10Percent"] = AvgSettlingTime10Percent.Value;
            }

            if (AvgSettlingTime5Percent.HasValue)
            {
                json["avgSettlingTime5Percent"] = AvgSettlingTime5Percent.Value;
            }

            if (AvgSettlingTime2Percent.HasValue)
            {
                json["avgSettlingTime2Percent"] = AvgSettlingTime2Percent.Value;
            }

            if (Avg10PercentResponseTime.HasValue)
            {
                json["avg10PercentResponseTime"] = Avg10PercentResponseTime.Value;
            }

            if (Avg50PercentResponseTime.HasValue)
            {
                json["avg50PercentResponseTime"] = Avg50PercentResponseTime.Value;
            }

            if (AvgCompleteResponseTime.HasValue)
            {
                json["avgCompleteResponseTime"] = AvgCompleteResponseTime.Value;
            }

            return json;
        }

        /// <summary>
        /// Creates a cumulative evaluation from any number of minor evaluations (equally weighted).
        /// The returned evaluation will take its timestamp from the first of the passed evaluations.
        /// </summary>
        public static PerformanceEvaluation FromCumulative(IEnumerable<PerformanceEvaluation> evaluations)
        {
            // Make sure we have at least 1 evaluation
            Assert.IsTrue(evaluations.Any());

            // Setup accumulator values
            int count = 0;

            float avgAbsoluteError = 0f;
            float avgSignedError = 0f;
            float maxAbsoluteError = 0f;

            float? maxOvershoot = null;

            AvgAccumulator avgSettlingTime10Percent = new AvgAccumulator();
            AvgAccumulator avgSettlingTime5Percent = new AvgAccumulator();
            AvgAccumulator avgSettlingTime2Percent = new AvgAccumulator();

            AvgAccumulator avg10PercentResponseTime = new AvgAccumulator();
            AvgAccumulator avg50PercentResponseTime = new AvgAccumulator();
            AvgAccumulator avgCompleteResponseTime = new AvgAccumulator();

            // Fill accumulator values
            foreach (var eval in evaluations)
            {
                avgAbsoluteError += eval.AvgAbsoluteError;
                avgSignedError += eval.AvgSignedError;
                maxAbsoluteError = Mathf.Max(maxAbsoluteError, eval.MaxAbsoluteError);

                if (eval.MaxOvershoot.HasValue)
                {
                    if (maxOvershoot.HasValue)
                    {
                        maxOvershoot = Mathf.Max(maxOvershoot.Value, eval.MaxOvershoot.Value);
                    }
                    else
                    {
                        maxOvershoot = eval.MaxOvershoot;
                    }
                }

                UpdateAvgAccumulator(avgSettlingTime10Percent, eval.AvgSettlingTime10Percent);
                UpdateAvgAccumulator(avgSettlingTime5Percent, eval.AvgSettlingTime5Percent);
                UpdateAvgAccumulator(avgSettlingTime2Percent, eval.AvgSettlingTime2Percent);

                UpdateAvgAccumulator(avg10PercentResponseTime, eval.Avg10PercentResponseTime);
                UpdateAvgAccumulator(avg50PercentResponseTime, eval.Avg50PercentResponseTime);
                UpdateAvgAccumulator(avgCompleteResponseTime, eval.AvgCompleteResponseTime);

                count++;
            }

            // Reduce averages
            avgAbsoluteError /= (float) count;
            avgSignedError /= (float) count;

            return new PerformanceEvaluation(evaluations.First().TimeStamp,
                avgAbsoluteError: avgAbsoluteError,
                avgSignedError: avgSignedError,
                maxAbsoluteError: maxAbsoluteError,
                maxOvershoot: maxOvershoot,
                avgSettlingTime10Percent: avgSettlingTime10Percent.ToAverage(),
                avgSettlingTime5Percent: avgSettlingTime5Percent.ToAverage(),
                avgSettlingTime2Percent: avgSettlingTime2Percent.ToAverage(),
                avg10PercentResponseTime: avg10PercentResponseTime.ToAverage(),
                avg50PercentResponseTime: avg50PercentResponseTime.ToAverage(),
                avgCompleteResponseTime: avgCompleteResponseTime.ToAverage());
        }

        private static void UpdateAvgAccumulator(AvgAccumulator accumulator, float? nextSample)
        {
            if (nextSample.HasValue)
            {
                if (accumulator.Value.HasValue)
                {
                    accumulator.Value += nextSample.Value;
                }
                else
                {
                    accumulator.Value = nextSample.Value;
                }

                accumulator.Count++;
            }
        }

        private class AvgAccumulator
        {
            public float? Value;
            public int Count;

            public AvgAccumulator()
            {
                Value = null;
                Count = 0;
            }

            public float? ToAverage()
            {
                if (Value.HasValue)
                {
                    return Value.Value / (float)Count;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}