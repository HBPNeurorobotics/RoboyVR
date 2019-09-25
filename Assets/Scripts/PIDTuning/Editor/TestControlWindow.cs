using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PIDTuning.Editor
{
    public class TestControlWindow : EditorWindow
    {
        private const float GRAPH_HEIGHT = 200f;
        private const float CONTROL_GAP = 2f;
        private const float BUTTON_WIDTH = 175f;

        private EditorGraphRenderer _graphRenderer;

        private Rect _graphRect;

        private TestRunner _testRunner;

        private PoseErrorTracker _poseErrorTracker;

        private string[] _jointNames = { "No joints found" };

        private int _selectedJointIndex = 0;

        private bool _initialized = false;

        private void OnEnable()
        {
            _graphRenderer = new EditorGraphRenderer();
            _graphRect = new Rect(0f, 0f, 200f, GRAPH_HEIGHT);
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
                    _graphRect.y = GUILayoutUtility.GetLastRect().yMax + CONTROL_GAP;
                    _graphRect.width = position.width;

                    _graphRenderer.DrawPreviewRect(_graphRect);

                    GUILayout.Space(GRAPH_HEIGHT + CONTROL_GAP);

                    var newSelectedJointIndex = EditorGUILayout.Popup(_selectedJointIndex, _jointNames, GUILayout.Width(300));

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