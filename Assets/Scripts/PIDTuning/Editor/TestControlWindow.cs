using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning.Editor
{
    public class TestControlWindow : EditorWindow
    {
        private const float GRAPH_MARGIN_LEFT = 40f;
        private const float GRAPH_HEIGHT = 200f;
        private const float CONTROL_GAP = 2f;
        private const float BUTTON_WIDTH = 175f;
        private const float LABEL_HEIGHT = 15f;

        private EditorGraphRenderer _graphRenderer;

        private Rect _graphRect;
        private Rect _graphLabelTopRect;
        private Rect _graphLabelBottomRect;

        private TestRunner _testRunner;

        private PoseErrorTracker _poseErrorTracker;

        private string[] _jointNames = { "No joints found" };

        private int _selectedJointIndex = 0;

        private bool _drawX = true, _drawY = false, _drawZ = false;

        private bool _initialized = false;

        private void OnEnable()
        {
            _graphRenderer = new EditorGraphRenderer();
            _graphRect = new Rect(GRAPH_MARGIN_LEFT, 0f, 200f, GRAPH_HEIGHT);
            _graphLabelTopRect = new Rect(10f, 0f, GRAPH_MARGIN_LEFT, LABEL_HEIGHT);
            _graphLabelBottomRect = new Rect(10f, 80f, GRAPH_MARGIN_LEFT, LABEL_HEIGHT);

            titleContent = new GUIContent("PID Test Ctrl");
            minSize = new Vector2(200f, 200f); // We define a non-zero minSize so the window cannot disappear
        }

        private void OnDisable()
        {
            _graphRenderer.Dispose();
        }

        [MenuItem("Window/PID Test Control")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TestControlWindow));
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            _testRunner = EditorGUILayout.ObjectField("PidTestRunner Scene Obj", _testRunner, typeof(TestRunner), true) as TestRunner;
            EditorGUI.EndDisabledGroup();

            if (null == _testRunner)
            {
                GUILayout.Label("Please enter PlayMode and drag PIDTuningService in here. Make sure it is enabled!");

                EditorGUILayout.EndHorizontal();

                _initialized = false;
            }
            else
            {
                GUILayout.Space(10f);

                EditorGUIUtility.labelWidth = 65f;
                _testRunner.CurrentTestLabel = EditorGUILayout.DelayedTextField("Test Label", _testRunner.CurrentTestLabel);
                EditorGUIUtility.labelWidth = 0f; // Reset label width

                DrawMultiPurposeButton();

                EditorGUILayout.EndHorizontal();

                if (_initialized)
                {
                    // Draw graph
                    _graphRect.y = GUILayoutUtility.GetLastRect().yMax + CONTROL_GAP;
                    _graphRect.width = position.width - GRAPH_MARGIN_LEFT;

                    _graphRenderer.DrawPreviewRect(_graphRect, _drawX, _drawY, _drawZ);

                    // Draw graph axis labels
                    var maxSampleLabel = Mathf.CeilToInt(_graphRenderer.MaxSampleValueForDisplay);

                    _graphLabelTopRect.y = _graphRect.y;
                    _graphLabelBottomRect.y = _graphRect.y + GRAPH_HEIGHT - LABEL_HEIGHT;

                    GUI.Label(_graphLabelTopRect, maxSampleLabel.ToString());
                    GUI.Label(_graphLabelBottomRect, "-" + maxSampleLabel);

                    GUILayout.Space(GRAPH_HEIGHT + CONTROL_GAP);

                    // Draw 30 sec indicator
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(GRAPH_MARGIN_LEFT - 3f + (GRAPH_HEIGHT / (2f * _graphRenderer.MaxSampleValueForDisplay)) * 30f);
                    GUILayout.Label("| 30 sec");
                    EditorGUILayout.EndHorizontal();

                    // Draw graph controls
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(GRAPH_MARGIN_LEFT);
                    var newSelectedJointIndex = EditorGUILayout.Popup(_selectedJointIndex, _jointNames);
                    _drawX = GUILayout.Toggle(_drawX, "X Axis");
                    _drawY = GUILayout.Toggle(_drawY, "Y Axis");
                    _drawZ = GUILayout.Toggle(_drawZ, "Z Axis");
                    EditorGUILayout.EndHorizontal();

                    if (newSelectedJointIndex != _selectedJointIndex)
                    {
                        _graphRenderer.StartNewLine(DateTime.Now);
                        _selectedJointIndex = newSelectedJointIndex;
                    }
                }
            }
        }

        private void OnInspectorUpdate()
        {
            if (null != _testRunner && _testRunner.State != TestRunner.TestRunnerState.NotReady)
            {
                if (!_initialized)
                {
                    InitializeAfterTestRunnerReady();

                    _graphRenderer.StartNewLine(DateTime.Now);

                    _initialized = true;
                }

                if (_graphRenderer.IsAtLimit)
                {
                    _graphRenderer.StartNewLine(DateTime.Now);
                }

                _graphRenderer.AddSample(DateTime.Now, _poseErrorTracker.GetCurrentStepDataForJoint(_jointNames[_selectedJointIndex]).SignedError);

                Repaint();
            }
        }

        private void InitializeAfterTestRunnerReady()
        {
            _poseErrorTracker = _testRunner.GetComponent<PoseErrorTracker>();

            Assert.IsNotNull(_poseErrorTracker);

            _jointNames = _poseErrorTracker.GetJointNames().ToArray();
            _selectedJointIndex = 0;
        }

        private void DrawMultiPurposeButton()
        {
            switch (_testRunner.State)
            {
                case TestRunner.TestRunnerState.NotReady:
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GUILayout.Button("Waiting for Avatar...", GUILayout.Width(BUTTON_WIDTH));
                    }
                    break;

                case TestRunner.TestRunnerState.Ready:

                    if (GUILayout.Button("Start Test", GUILayout.Width(BUTTON_WIDTH)))
                    {
                        _testRunner.StartCoroutine(_testRunner.RunTest());
                    }

                    break;

                case TestRunner.TestRunnerState.RunningTest:
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GUILayout.Button("Test Running...", GUILayout.Width(BUTTON_WIDTH));
                    }
                    break;

                case TestRunner.TestRunnerState.FinishedTest:

                    if (GUILayout.Button("Reset", GUILayout.Width(BUTTON_WIDTH)))
                    {
                        _testRunner.ResetTestRunner();
                    }

                    break;
            }
        }
    }
}