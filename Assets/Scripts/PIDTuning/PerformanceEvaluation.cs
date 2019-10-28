using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// How fast does PV settle within 20% of a new set-point after a change?
        /// </summary>
        public readonly float? AvgSettlingTime10Percent;

        /// <summary>
        /// How fast does PV settle within 10% of a new set-point after a change?
        /// </summary>
        public readonly float? AvgSettlingTime5Percent;

        /// <summary>
        /// How fast does PV settle within 5% of a new set-point after a change?
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

            AvgAccumulator avgSettlingTime10Percent = new AvgAccumulator();
            AvgAccumulator avgSettlingTime5Percent = new AvgAccumulator();
            AvgAccumulator avgSettlingTime2Percent = new AvgAccumulator();

            AvgAccumulator avg10PercentResponseTime = new AvgAccumulator();
            AvgAccumulator avg50PercentResponseTime = new AvgAccumulator();
            AvgAccumulator avgCompleteResponseTime = new AvgAccumulator();

            CalculateResponseMetrics(
                stepData: stepData,
                maxOvershoot: out maxOvershoot,
                avgSettlingTime10Percent: avgSettlingTime10Percent,
                avgSettlingTime5Percent: avgSettlingTime5Percent,
                avgSettlingTime2Percent: avgSettlingTime2Percent,
                avg10PercentResponseTime: avg10PercentResponseTime,
                avg50PercentResponseTime: avg50PercentResponseTime,
                avgCompleteResponseTime: avgCompleteResponseTime);

            return new PerformanceEvaluation(timestamp,
                avgAbsoluteError: avgAbsoluteError, avgSignedError: avgSignedError, maxAbsoluteError: maxAbsoluteError,
                maxOvershoot: maxOvershoot,
                avgSettlingTime10Percent: avgSettlingTime10Percent.ToAverage(), 
                avgSettlingTime5Percent: avgSettlingTime5Percent.ToAverage(), 
                avgSettlingTime2Percent: avgSettlingTime2Percent.ToAverage(),
                avg10PercentResponseTime: avg10PercentResponseTime.ToAverage(), 
                avg50PercentResponseTime: avg50PercentResponseTime.ToAverage(), 
                avgCompleteResponseTime: avgCompleteResponseTime.ToAverage());
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
                avgAbsoluteError += entry.AbsoluteError;
                avgSignedError += entry.SignedError;
                maxAbsoluteError = Mathf.Max(entry.AbsoluteError, maxAbsoluteError);

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
            AvgAccumulator avgSettlingTime10Percent, AvgAccumulator avgSettlingTime5Percent, AvgAccumulator avgSettlingTime2Percent,
            AvgAccumulator avg10PercentResponseTime, AvgAccumulator avg50PercentResponseTime, AvgAccumulator avgCompleteResponseTime)
        {
            maxOvershoot = null;

            KeyValuePair<DateTime, PidStepDataEntry>? oldSetPoint = null;

            // Hold a streak of entries where the step-data is constant
            var currentStreak = new List<KeyValuePair<DateTime, PidStepDataEntry>>();

            // Hold the distance between the old set-point and the new set-point (at which the streak is)
            var currentSetPointDistance = 0f;

            foreach (var datedEntry in stepData.Data)
            {
                if (null == oldSetPoint)
                {
                    // We don't have any initial old set-point
                    oldSetPoint = datedEntry;

                    continue;
                }

                if (!Mathf.Approximately(datedEntry.Value.Desired, oldSetPoint.Value.Value.Desired)) // was (datedEntry.Value.Desired != oldSetPoint.Value.Value.Desired)
                {
                    // We have a new set-point that is different from the current set-point

                    if (currentStreak.Count >= 2)
                    {
                        // Evaluate the current streak, if it is actually a streak (meaning more than 1 element)
                        CalculateMaxOvershoot(currentStreak, ref maxOvershoot);

                        CalculateSettlingTime(currentStreak, currentSetPointDistance,
                            avgSettlingTime10Percent, avgSettlingTime5Percent, avgSettlingTime2Percent);

                        CalculateResponseTime(currentStreak, currentSetPointDistance,
                            avg10PercentResponseTime, avg50PercentResponseTime, avgCompleteResponseTime);
                    }

                    // Start a new streak, beginning with the first entry that has the new set-point
                    currentStreak.Clear();
                    currentStreak.Add(datedEntry);

                    // Set the set-point distance between the old streak (or non-streak) and the new streak
                    currentSetPointDistance = Mathf.Abs(datedEntry.Value.Desired - oldSetPoint.Value.Value.Desired);

                    oldSetPoint = datedEntry;

                    continue;
                }

                if (currentStreak.Count > 0)
                {
                    // If we have a streak going, add the entry
                    currentStreak.Add(datedEntry);
                }
            }

            // We might not have dealt with the final streak. This code should fix that:

            if (currentStreak.Count >= 2)
            {
                // Evaluate the current streak, if it is actually a streak (meaning more than 2 elements)
                CalculateMaxOvershoot(currentStreak, ref maxOvershoot);
                CalculateSettlingTime(currentStreak, currentSetPointDistance,
                    avgSettlingTime10Percent, avgSettlingTime5Percent, avgSettlingTime2Percent);
                CalculateResponseTime(currentStreak, currentSetPointDistance,
                    avg10PercentResponseTime, avg50PercentResponseTime, avgCompleteResponseTime);
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
            if (Mathf.Approximately(first.Measured, first.Desired)) // was (first.Measured == first.Desired)
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

        private static void CalculateSettlingTime(List<KeyValuePair<DateTime, PidStepDataEntry>> currentStreak, float setPointDistance,
            AvgAccumulator avgSettlingTime10Percent, AvgAccumulator avgSettlingTime5Percent, AvgAccumulator avgSettlingTime2Percent)
        {
            // The general notion here is: We identify all peaks and assert they the have decreasing magnitude.
            // Otherwise the process is unstable or disturbed somehow and finding a settling time doesn't make sense.

            // Then we pick the first peak that is under 10%/5%/2% respectively and backtrack until we find the
            // exact point that PV first entered the 10%/5%/2% band around SP.

            // 1. Find all the peak indices.

            var peakIndices = PeakAnalysis.FindPeakIndices(currentStreak);

            // If we have no peaks, we abort. While it is technically possible that we could
            // have 0 overshoot (and thus no peaks), this is only the case for PID controllers
            // with Ki=0, which are hopefully pretty rare.
            if (!peakIndices.Any())
            {
                return;
            }

            // 2. Assert the peaks are of decreasing magnitude within reasonable limits
            // Reasonable limits means we don't look at peaks below 5% of the set-point distance

            //// M: In retrospect, this condition seems a bit strict. 5% of the set-point distance
            //// can be a very small amount - so small, in fact, that floating point errors are
            //// sometimes dominant. I changed it to an absolute error of +/- 2 degrees for now.

            // M: Again, in retrospect, this didn't work either. I suspect that disturbances
            // caused by other joint can cause small peaks which violate this condition.
            // I completely disabled it for now.

            //float lastPeakAbsError = currentStreak[peakIndices[0]].Value.AbsoluteError;

            //foreach (var peak in peakIndices)
            //{
            //    var peakEntry = currentStreak[peak];

            //    if (peakEntry.Value.AbsoluteError > lastPeakAbsError &&
            //        peakEntry.Value.AbsoluteError > 2.0f) // was: peakEntry.Value.AbsoluteError > 0.05f * setPointDistance
            //    {
            //        // If the peak magnitude increased significantly and the peak was significant, we abort
            //        return;
            //    }

            //    lastPeakAbsError = peakEntry.Value.AbsoluteError;
            //}

            // 3. Find the first peaks that fall into the 10%/5%/2% bands and backtrack to find the settling times

            // Helper function: Takes a width around SP (bandWidth, in percent) and updates the settling time average if possible
            Action<AvgAccumulator, float> updateSettlingTime = (acc, bandWidth) =>
            {
                try
                {
                    var firstPeakBelowIndex =
                        peakIndices.First(i => currentStreak[i].Value.AbsoluteError <= bandWidth * setPointDistance);

                    // Find the index of the latest value that is outside of the settling band
                    int lastOutsiderIndex;
                    for (lastOutsiderIndex = firstPeakBelowIndex - 1; lastOutsiderIndex <= 0; lastOutsiderIndex--)
                    {
                        if (currentStreak[lastOutsiderIndex].Value.AbsoluteError > bandWidth * setPointDistance)
                        {
                            break;
                        }
                    }

                    acc.Update((float)(currentStreak[lastOutsiderIndex].Key - currentStreak.First().Key).TotalSeconds);
                }
                catch
                {
                    // No peak was small enough, so we can't update the metric
                }
            };

            updateSettlingTime(avgSettlingTime10Percent, 0.1f);
            updateSettlingTime(avgSettlingTime5Percent, 0.05f);
            updateSettlingTime(avgSettlingTime2Percent, 0.02f);
        }

        private static void CalculateResponseTime(List<KeyValuePair<DateTime, PidStepDataEntry>> currentStreak, float setPointDistance,
            AvgAccumulator avg10PercentResponseTime, AvgAccumulator avg50PercentResponseTime, AvgAccumulator avgCompleteResponseTime)
        {
            var threshold10Percent = .9f * setPointDistance;
            var threshold50Percent = .5f * setPointDistance;
            var initialErrorSign = Mathf.Sign(currentStreak[0].Value.SignedError);

            var crossed10Percent = false;
            var crossed50Percent = false;

            foreach (var datedEntry in currentStreak)
            {
                var respTime = (float)(datedEntry.Key - currentStreak[0].Key).TotalSeconds;

                if (!crossed10Percent && datedEntry.Value.AbsoluteError <= threshold10Percent)
                {
                    crossed10Percent = true;
                    avg10PercentResponseTime.Update(respTime);
                }

                if (!crossed50Percent && datedEntry.Value.AbsoluteError <= threshold50Percent)
                {
                    crossed50Percent = true;
                    avg50PercentResponseTime.Update(respTime);
                }

                if (Mathf.Sign(datedEntry.Value.SignedError) != initialErrorSign)
                {
                    // If the sign of the signed error flips, we have crossed the set-point,
                    // meaning that we have a complete response
                    avgCompleteResponseTime.Update(respTime);

                    // Since having a 100% response also means we MUST have had a 10% and 50%
                    // response, we can short-circuit here
                    return;
                }
            }
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

                avgSettlingTime10Percent.Update(eval.AvgSettlingTime10Percent);
                avgSettlingTime5Percent.Update(eval.AvgSettlingTime5Percent);
                avgSettlingTime2Percent.Update(eval.AvgSettlingTime2Percent);

                avg10PercentResponseTime.Update(eval.Avg10PercentResponseTime);
                avg50PercentResponseTime.Update(eval.Avg50PercentResponseTime);
                avgCompleteResponseTime.Update(eval.AvgCompleteResponseTime);

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

        /// <summary>
        /// Can store a float average of an arbitrary sample size.
        /// </summary>
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

            public void Update(float? nextSample)
            {
                if (nextSample.HasValue)
                {
                    if (this.Value.HasValue)
                    {
                        this.Value += nextSample.Value;
                    }
                    else
                    {
                        this.Value = nextSample.Value;
                    }

                    this.Count++;
                }
            }
        }
    }
}