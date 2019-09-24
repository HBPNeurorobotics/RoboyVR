using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

namespace PIDTuning
{
    [RequireComponent(typeof(LineRenderer))]
    public class GraphLineRenderer : MonoBehaviour
    {
        private const int MAX_SAMPLES = 5000;

        /// <summary>
        /// Determines how far apart two samples are displayed. Play around with it.
        /// </summary>
        private readonly float _secondsPerGraphUnit = 1f;

        private LineRenderer _lineRenderer;

        private DateTime _firstSampleTimestamp;

        /// <summary>
        /// Maximum ABSOLUTE value among all samples in the graph.
        /// This should be used to properly render the whole graph height.
        /// That makes displaying the thing correctly slightly harder,
        /// but gives a massive performance boost.
        /// </summary>
        public float MaxSampleValue { private set; get; }

        /// <summary>
        /// To avoid lag, this renderer will only display samples
        /// up to MAX_SAMPLES. After that, new samples are ignored
        /// and this propeprty is set to true
        /// </summary>
        public bool IsAtLimit { private set; get; }

        public void Initialize()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 0;
            _lineRenderer.startColor = _lineRenderer.endColor = Color.red;

            _firstSampleTimestamp = DateTime.UtcNow;

            MaxSampleValue = 0f;
            IsAtLimit = false;
        }

        public void StartNewLine(DateTime firstSampleTimestamp)
        {
            _firstSampleTimestamp = firstSampleTimestamp;
        }

        public void AddSample(DateTime timestamp, float sampleVal)
        {
            if (_lineRenderer.positionCount == MAX_SAMPLES)
            {
                IsAtLimit = true;
                return;
            }

            MaxSampleValue = Mathf.Max(MaxSampleValue, Mathf.Abs(sampleVal));

            var x = (float)(timestamp - _firstSampleTimestamp).TotalSeconds / _secondsPerGraphUnit;

            _lineRenderer.positionCount += 1;
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, new Vector3(x, sampleVal));
        }
    }
}