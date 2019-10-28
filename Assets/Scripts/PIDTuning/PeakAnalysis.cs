using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PIDTuning
{
    public struct OscillationAnalysisResult
    {
        /// <summary>
        /// Average Same-Sign Peak interval in seconds
        /// </summary>
        public readonly float UltimatePeriod;

        /// <summary>
        /// Zero-to-Peak Amplitude average of all peaks (in degrees)
        /// </summary>
        public readonly float Amplitude;

        public OscillationAnalysisResult(float ultimatePeriod, float amplitude)
        {
            UltimatePeriod = ultimatePeriod;
            Amplitude = amplitude;
        }
    }

    public static class PeakAnalysis
    {
        private const float MIN_PEAK_ERROR_DISTANCE = 1f;
        private const int PEAK_DETECTION_SAMPLE_DIST = 10;

        private enum PeakType
        {
            Min,
            Max
        }

        public static List<int> FindPeakIndices(IList<KeyValuePair<DateTime, PidStepDataEntry>> data)
        {
            // I have found that the input data is very slightly noisy. I decided to go for a more thorough,
            // but slightly slower peak detection algorithm.

            var peakIndices = new List<int>();

            PeakType? lastPeakType = null;
            KeyValuePair<DateTime, PidStepDataEntry>? lastPeak = null;

            for (int potentialPeakIdx = 2 * PEAK_DETECTION_SAMPLE_DIST; potentialPeakIdx < data.Count - 2 * PEAK_DETECTION_SAMPLE_DIST; potentialPeakIdx++)
            {
                var xmm = data[potentialPeakIdx - 2 * PEAK_DETECTION_SAMPLE_DIST].Value.Measured;
                var xm = data[potentialPeakIdx - PEAK_DETECTION_SAMPLE_DIST].Value.Measured;
                var x = data[potentialPeakIdx].Value.Measured;
                var xp = data[potentialPeakIdx + PEAK_DETECTION_SAMPLE_DIST].Value.Measured;
                var xpp = data[potentialPeakIdx + 2 * PEAK_DETECTION_SAMPLE_DIST].Value.Measured;

                var isLocalMax = xmm <= xm && xm <= x && x >= xp && xp >= xpp;
                var isLocalMin = xmm >= xm && xm >= x && x <= xp && xp <= xpp;

                if (isLocalMin || isLocalMax)
                {
                    if (!peakIndices.Any() || !data[peakIndices.Last()].Value.IsSimilarTo(data[potentialPeakIdx].Value, MIN_PEAK_ERROR_DISTANCE))
                    {
                        peakIndices.Add(potentialPeakIdx);
                    }
                }
            }

            return peakIndices;
        }

        public static OscillationAnalysisResult? AnalyzeOscillation(PidStepData stepData)
        {
            const int MIN_PEAKS_FOR_OSCILLATION = 6;

            var stepDataAsList = stepData.Data.ToList();
            var peaks = FindPeakIndices(stepDataAsList).Select(peak => stepDataAsList[peak]).ToList();

            if (peaks.Count < MIN_PEAKS_FOR_OSCILLATION)
            {
                return null;
            }
            else
            {
                float avgAmplitude = peaks.Average(datedEntry => datedEntry.Value.AbsoluteError);

                var posPeaks = peaks.Where(datedEntry => datedEntry.Value.SignedError < 0f).ToList();

                var avgInterval = (float)posPeaks
                    .Skip(1)
                    .Select((datedEntry, i) => new KeyValuePair<DateTime, DateTime>(datedEntry.Key, posPeaks[i].Key))
                    .Average(nextLastPairs => (nextLastPairs.Key - nextLastPairs.Value).TotalSeconds);

                StringBuilder sb = new StringBuilder();
                var firstSampleTime = posPeaks.First().Key;
                foreach (var pk in posPeaks)
                {
                    sb.AppendFormat("{0}: {1}\n", (pk.Key - firstSampleTime).TotalSeconds, pk.Value.Measured);
                }

                Debug.Log("Peaks: " + sb);
                Debug.Log("Amp: " + avgAmplitude);
                Debug.Log("Tu: " + avgInterval);

                return new OscillationAnalysisResult(avgInterval, avgAmplitude);
            }
        }
    }
}       