﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

namespace PIDTuning
{
    [RequireComponent(typeof(LineRenderer))]
    public class GraphLineRenderer : MonoBehaviour
    {
        private const int MAX_SAMPLES = 1000;

        /// <summary>
        /// Determines how far apart two samples are displayed. Play around with it.
        /// </summary>
        public const float SecondsPerGraphUnit = 0.2f;

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

        public float LastSampleX { private set; get; }

        public float LineWidthMultiplier
        {
            get { return _lineRenderer.widthMultiplier; }
            set { _lineRenderer.widthMultiplier = value; }
        }

        public bool IsVisible
        {
            get { return _lineRenderer.enabled; }
            set { _lineRenderer.enabled = value; }
        }

        public void Initialize(Color color)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 0;
            _lineRenderer.material.color = color;
            _firstSampleTimestamp = DateTime.UtcNow;

            LastSampleX = 0f;
            MaxSampleValue = 1f;
            IsAtLimit = false;
        }

        public void StartNewLine(DateTime firstSampleTimestamp)
        {
            _firstSampleTimestamp = firstSampleTimestamp;
            _lineRenderer.positionCount = 0;

            LastSampleX = 0f;
            MaxSampleValue = 1f;
            IsAtLimit = false;
        }

        public void AddSample(DateTime timestamp, float sample)
        {
            if (_lineRenderer.positionCount >= MAX_SAMPLES)
            {
                IsAtLimit = true;
                return;
            }

            MaxSampleValue = Mathf.Max(MaxSampleValue, Mathf.Abs(sample));

            var x = (float)(timestamp - _firstSampleTimestamp).TotalSeconds / SecondsPerGraphUnit;

            _lineRenderer.positionCount += 1;
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, new Vector3(x, sample));

            LastSampleX = x;
        }
    }
}