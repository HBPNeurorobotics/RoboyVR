using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning.Editor
{
    public class EditorGraphRenderer
    {
        private const float BASELINE_WIDTH = 0.05f;
        private const float SAMPLE_LINE_WIDTH = 0.1f;

        private PreviewRenderUtility _preview;

        private GraphLineRenderer _errorLine;

        private GraphLineRenderer _baseLine;

        public bool IsAtLimit
        {
            // Only need to care about x because all axis get the same amount of samples
            get { return _errorLine.IsAtLimit; }
        }

        /// <summary>
        /// Will never return less than 5, even if the highest sample of all axis is below that!
        /// </summary>
        public float MaxSampleValueForDisplay
        {
            get { return Mathf.Max(5f, _errorLine.MaxSampleValue); }
        }

        public void DrawPreviewRect(Rect rect, bool drawX, bool drawY, bool drawZ)
        {
            if (null == _preview)
            {
                Initialize();
            }

            // Adjust camera rect for highest sample value, with a minumum of 5
            var maxSampleVal = Mathf.Ceil(MaxSampleValueForDisplay);
            _preview.camera.orthographicSize = maxSampleVal;

            var rectAspect = (rect.size.x / rect.size.y);
            // Adjust camera so that 1 second of samples is always represented as the same width
            // NOTE: While this sounds smart, i really screws with the line width due to the sometimes
            // very extreme aspect ratio. For now, just learn to live with per-joint time scaling.
            //_preview.camera.aspect = rectAspect / maxSampleVal;
            //var camWorldHalfWidth = _preview.camera.aspect * _preview.camera.orthographicSize;

            // Move preview camera right to capture the latest sample
            var camWorldHalfWidth = rectAspect * _preview.camera.orthographicSize;
            var camX = Mathf.Max(_errorLine.LastSampleX - camWorldHalfWidth, camWorldHalfWidth);
            _preview.camera.transform.position = new Vector3(camX, 0f, -1f);

            // Move the baseline along with the camera
            _baseLine.transform.position = new Vector3(_preview.camera.transform.position.x - camWorldHalfWidth, 0f, 1f);

            // Adjust line width to account for viewport changes
            _baseLine.LineWidthMultiplier = maxSampleVal * BASELINE_WIDTH;
            _errorLine.LineWidthMultiplier = maxSampleVal * SAMPLE_LINE_WIDTH;

            // Line visibility
            _errorLine.IsVisible = drawX;

            _preview.BeginPreview(rect, GUIStyle.none);
            _preview.camera.Render();
            _preview.EndAndDrawPreview(rect);
        }

        public void Dispose()
        {
            if (null != _preview)
            {
                _preview.Cleanup();
            }
        }

        public void StartNewLine(DateTime firstSampleTimestamp)
        {
            if (null == _preview)
            {
                Initialize();
            }

            _errorLine.StartNewLine(firstSampleTimestamp);
        }

        public void AddSample(DateTime timestamp, float sample)
        {
            if (null == _preview)
            {
                Initialize();
            }

            _errorLine.AddSample(timestamp, sample);
        }

        private void Initialize()
        {
            var lineRendererPrefab = Resources.Load<GameObject>("GraphLineRenderer");
            Assert.IsNotNull(lineRendererPrefab);

            _preview = new PreviewRenderUtility(true);

            _preview.camera.transform.position = new Vector3(0f, 0f, -1f);
            _preview.camera.orthographic = true;
            _preview.camera.nearClipPlane = 0.1f;
            _preview.camera.farClipPlane = 10f;

            // Cheesy way to draw a baseline: We just use the GraphLineRenderer prefab
            _baseLine = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            _baseLine.Initialize(Color.gray);
            _baseLine.StartNewLine(DateTime.Now);
            _baseLine.AddSample(DateTime.Now, 0f);
            _baseLine.AddSample(DateTime.Now + TimeSpan.FromHours(1), 0f);

            _errorLine = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            _errorLine.transform.position = Vector3.zero;
            _errorLine.Initialize(Color.red);
        }
    }
}