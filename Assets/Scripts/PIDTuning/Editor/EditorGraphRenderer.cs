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

        private GraphLineRenderer _glr;

        public void DrawPreviewRect(Rect rect)
        {
            if (null == _preview)
            {
                Initialize();
            }

            _preview.BeginPreview(rect, GUIStyle.none);
            _glr.transform.position = Vector3.left * rect.size.x / rect.size.y * _preview.camera.orthographicSize;
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
            _glr.StartNewLine(firstSampleTimestamp);
        }

        public void AddSample(DateTime timestamp, float sampleVal)
        {
            _glr.AddSample(timestamp, sampleVal);
        }

        private void Initialize()
        {
            var lineRendererPrefab = Resources.Load<GameObject>("GraphLineRenderer");
            Assert.IsNotNull(lineRendererPrefab);

            _preview = new PreviewRenderUtility(true);

            _preview.camera.transform.position = new Vector3(0f, 0f, -10f);
            _preview.camera.orthographic = true;
            _preview.camera.orthographicSize = 1;

           _glr = _preview.InstantiatePrefabInScene(lineRendererPrefab).GetComponent<GraphLineRenderer>();
            _glr.Initialize();
            Assert.IsNotNull(_glr);
        }
    }
}