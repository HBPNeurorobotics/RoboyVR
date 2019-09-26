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
        private PreviewRenderUtility _preview;

        private GraphLineRenderer _glrX, _glrY, _glrZ;

        private GraphLineRenderer _baseLine;

        public bool IsAtLimit
        {
            // Only need to care about x because all axis get the same amount of samples
            get { return _glrX.IsAtLimit; }
        }

        public float MaxSampleValue
        {
            get { return Mathf.Max(_glrX.MaxSampleValue, _glrY.MaxSampleValue, _glrZ.MaxSampleValue); }
        }

        public void DrawPreviewRect(Rect rect, bool drawX, bool drawY, bool drawZ)
        {
            if (null == _preview)
            {
                Initialize();
            }

            // Adjust camera rect for highest sample value
            float maxSampleVal = Mathf.Ceil(MaxSampleValue);
            _preview.camera.orthographicSize = maxSampleVal;

            // Move preview camera right to capture the latest sample
            var camWorldHalfWidth = (rect.size.x / rect.size.y) * _preview.camera.orthographicSize;
            var camX = Mathf.Max(_glrX.LastSampleX - camWorldHalfWidth, camWorldHalfWidth);
            _preview.camera.transform.position = new Vector3(camX, 0f, -1f);

            // Move the baseline along with the camera
            _baseLine.transform.position = new Vector3(_preview.camera.transform.position.x - camWorldHalfWidth, 0f, 1f);

            // Line visibility
            _glrX.IsVisible = drawX;
            _glrY.IsVisible = drawY;
            _glrZ.IsVisible = drawZ;

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

            _glrX.StartNewLine(firstSampleTimestamp);
            _glrY.StartNewLine(firstSampleTimestamp);
            _glrZ.StartNewLine(firstSampleTimestamp);
        }

        public void AddSample(DateTime timestamp, Vector3 sample)
        {
            if (null == _preview)
            {
                Initialize();
            }

            _glrX.AddSample(timestamp, sample.x);
            _glrY.AddSample(timestamp, sample.y);
            _glrZ.AddSample(timestamp, sample.z);
        }

        private void Initialize()
        {
            var lineRendererPrefab = Resources.Load<GameObject>("GraphLineRenderer");
            Assert.IsNotNull(lineRendererPrefab);

            _preview = new PreviewRenderUtility(true);

            _preview.camera.transform.position = new Vector3(0f, 0f, -1f);
            _preview.camera.orthographic = true;
            _preview.camera.orthographicSize = 1;
            _preview.camera.nearClipPlane = 0.1f;
            _preview.camera.farClipPlane = 10f;

            // Cheesy way to draw a baseline: We just use the GraphLineRenderer prefab
            _baseLine = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            _baseLine.Initialize(Color.gray, 0.09f);
            _baseLine.StartNewLine(DateTime.Now);
            _baseLine.AddSample(DateTime.Now, 0f);
            _baseLine.AddSample(DateTime.Now + TimeSpan.FromHours(1), 0f);

            _glrX = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            _glrX.transform.position = Vector3.zero;
            _glrX.Initialize(Color.red, 0.15f);

            _glrY = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            _glrY.transform.position = Vector3.forward * 0.1f;
            _glrY.Initialize(Color.green, 0.15f);

            _glrZ = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            _glrZ.transform.position = Vector3.forward * 0.2f;
            _glrZ.Initialize(Color.blue, 0.15f);
        }
    }
}