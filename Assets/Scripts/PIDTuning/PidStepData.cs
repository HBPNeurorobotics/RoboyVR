using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public readonly string Name;

        public readonly DateTime CreatedTimestampUtc;

        public readonly SortedList<DateTime, PidStepDataEntry> Data;

        public readonly Dictionary<string, string> AdditionalKeys;

        public PidStepData(string name, DateTime createdTimestampUtc)
        {
            // We take the timestamp as a parameter here instead of generating it
            // ourselves. This allows the caller to re-use their timestamp for other
            // data structures, making sure that they match exactly

            // But let's just make sure they are smart enough to use universal time.
            Assert.AreEqual(DateTimeKind.Utc, createdTimestampUtc.Kind);

            Name = name;
            CreatedTimestampUtc = createdTimestampUtc;
            Data = new SortedList<DateTime, PidStepDataEntry>();

            AdditionalKeys = new Dictionary<string, string>();
        }

        public float SignedError(ErrorScaling errorScaling, DateTime? start = null, DateTime? end = null)
        {
            ConstrainToLegalTimeRange(ref start, ref end);

            var aggregate = GetEntriesInRange(start.Value, end.Value)
                .Aggregate(new ErrorAggregationResult(), (acc, data) =>
                {
                    acc.Count++;
                    acc.Error += data.Value.SignedError;
                    return acc;
                });

            return ScaledError(errorScaling, start, end, aggregate);
        }

        public float AbsoluteError(ErrorScaling errorScaling, DateTime? start = null, DateTime? end = null)
        {
            ConstrainToLegalTimeRange(ref start, ref end);

            var aggregate = GetEntriesInRange(start.Value, end.Value)
                .Aggregate(new ErrorAggregationResult(), (acc, data) =>
                {
                    acc.Count++;
                    acc.Error += data.Value.AbsoluteError;
                    return acc;
                });

            return ScaledError(errorScaling, start, end, aggregate);
        }

        public JSONNode ToJson()
        {
            var json = new JSONNode();
            json["name"] = Name;
            json["createdTimestamp"] = CreatedTimestampUtc.ToFileTimeUtc().ToString();

            foreach (var kvPair in AdditionalKeys)
            {
                json[kvPair.Key] = kvPair.Value;
            }

            int i = 0;
            foreach (var entry in Data)
            {
                json["data"][i++]["timestamp"] = entry.Key.ToFileTimeUtc().ToString();
                json["data"][i++]["entry"] = entry.Value.ToJson();
            }

            return json;
        }

        /// <summary>
        /// Does NOT do constraint checking on the parameters. Do that yourself via "ConstrainToLegalTimeRange"
        /// </summary>
        private IEnumerable<KeyValuePair<DateTime, PidStepDataEntry>> GetEntriesInRange(DateTime start, DateTime end)
        {
            return Data
                .SkipWhile(data => data.Key < start) // Skip entries with smaller timestamps than "start"
                .TakeWhile(data => data.Key <= end); // Take all entries with smaller/equal timestamps than "end"
        }

        /// <summary>
        /// Constrains given timestamps to a range of time that is represented in "Data".
        /// If "start" is null, it is set to the first entry's timestamp. "end" behaves analogously.
        /// </summary>
        private void ConstrainToLegalTimeRange(ref DateTime? start, ref DateTime? end)
        {
            Assert.IsTrue(Data.Count > 0);

            if (null == start)
            {
                start = Data.First().Key;
            }
            else
            {
                Assert.AreEqual(DateTimeKind.Utc, start.Value.Kind);
                Assert.IsTrue(start.Value >= Data.First().Key && start.Value <= Data.Last().Key);
            }

            if (null == end)
            {
                end = Data.Last().Key;
            }
            else
            {
                Assert.AreEqual(DateTimeKind.Utc, end.Value.Kind);
                Assert.IsTrue(end.Value >= Data.First().Key && end.Value <= Data.Last().Key);
            }

            Assert.IsTrue(end.Value >= start.Value);
        }

        /// <summary>
        /// Scales an accumulated error value via the provided scaling method.
        /// Unscaled errors are generally useless because longer measurements will often
        /// generate larger accumulate errors.
        /// </summary>
        private static float ScaledError(ErrorScaling errorScaling, DateTime? start, DateTime? end, ErrorAggregationResult aggregate)
        {
            switch (errorScaling)
            {
                case ErrorScaling.Unscaled:
                    return aggregate.Error;

                case ErrorScaling.PerSecond:
                    return aggregate.Error / (float)(end.Value - start.Value).TotalSeconds;

                case ErrorScaling.PerSample:
                    return aggregate.Error / aggregate.Count;

                default:
                    throw new InvalidOperationException();
            }
        }

        public enum ErrorScaling
        {
            Unscaled,
            PerSecond,
            PerSample
        }

        /// <summary>
        /// This is kind of a hacky thing to improve performance:
        /// We use LINQ to calculate aggregate error values.
        /// To scale them by the number of samples, we would need to
        /// evaluate a sample range (IEnumerable) twice:
        /// - once for .Aggregate(...)
        /// - once for .Count()
        /// This can be expensive, so we move the .Count() into
        /// the .Aggregate(...) by storing a running count as part
        /// of the accumulator value.
        /// </summary>
        private class ErrorAggregationResult
        {
            /// <summary>
            /// The running count
            /// </summary>
            public int Count;

            /// <summary>
            /// The aggregated/accumulated error
            /// </summary>
            public float Error;

            public ErrorAggregationResult()
            {
                Count = 0;
                Error = 0;
            }
        }
    }
}