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

        private GraphLineRenderer _glrX;

        public bool IsAtLimit
        {
            get { return _glrX.IsAtLimit; }
        }

        public void DrawPreviewRect(Rect rect)
        {
            if (null == _preview)
            {
                Initialize();
            }

            // Move preview camera right to capture the latest sample
            var camWorldHalfWidth = (rect.size.x / rect.size.y) * _preview.camera.orthographicSize;
            var camX = Mathf.Max(_glrX.LastSampleX - camWorldHalfWidth, camWorldHalfWidth);
            _preview.camera.transform.position = new Vector3(camX, 0f, -1f);

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
        }

        public void AddSample(DateTime timestamp, Vector3 sample)
        {
            if (null == _preview)
            {
                Initialize();
            }

            // TODO: Other axis
            _glrX.AddSample(timestamp, sample.x);
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

            // Cheesy way to draw a baseline: We just use the GraphLineRenderer prefab and abuse it
            var baseLine = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            baseLine.Initialize(Color.gray, 0.5f);
            baseLine.StartNewLine(DateTime.Now);
            baseLine.AddSample(DateTime.Now, 0f);
            baseLine.AddSample(DateTime.Now + TimeSpan.FromHours(1), 0f);
            baseLine.transform.position = Vector3.forward;

            _glrX = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            _glrX.transform.position = Vector3.zero;
            _glrX.Initialize(Color.red);
        }
    }
}