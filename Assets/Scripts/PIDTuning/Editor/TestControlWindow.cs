using System;
using System.Linq;
using System.Text;
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

        private AnimatorControl _animatorControl;

        private string[] _jointNames = { "No joints found" };

        private int _selectedJointIndex = 0;

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
                _testRunner.CurrentTestLabel = EditorGUILayout.TextField("Test Label", _testRunner.CurrentTestLabel);
                EditorGUIUtility.labelWidth = 0f; // Reset label width

                DrawTestControlButton();
                DrawRecordingControlButton();

                if (_testRunner.State == TestRunner.TestRunnerState.FinishedTest)
                {
                    if (GUILayout.Button("Save Results"))
                    {
                        _testRunner.SaveTestData();
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (_initialized)
                {
                    DrawGraph();
                    DrawJointControls();

                    if (_testRunner.State == TestRunner.TestRunnerState.FinishedTest)
                    {
                        DrawAllEvaluations();
                    }
                }
            }
        }

        // Lower-level drawing functions
        // #########################################################################################

        private void DrawJointControls()
        {
            EditorGUILayout.BeginHorizontal();

            var newSelectedJointIndex = EditorGUILayout.Popup(_selectedJointIndex, _jointNames);

            if (newSelectedJointIndex != _selectedJointIndex)
            {
                _graphRenderer.StartNewLine(DateTime.Now);
                _selectedJointIndex = newSelectedJointIndex;
            }

            if (GUILayout.Button("Invert Joint Angle (Step-Test)", GUILayout.Width(350f)))
            {
                if (_animatorControl.Animator.enabled)
                {
                    _animatorControl.Animator.enabled = false;
                    Debug.LogWarning("Had to disable animator component to trigger step test!");
                }

                var oldAngle = _poseErrorTracker.LocalRig.GetJointToRadianMapping()[_jointNames[_selectedJointIndex]];
                _poseErrorTracker.LocalRig.SetJointRadians(_jointNames[_selectedJointIndex], -oldAngle);

            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGraph()
        {
            _graphRect.y = GUILayoutUtility.GetLastRect().yMax + CONTROL_GAP;
            _graphRect.width = position.width - GRAPH_MARGIN_LEFT;

            _graphRenderer.DrawPreviewRect(_graphRect);

            // Draw graph axis labels
            var maxSampleLabel = Mathf.CeilToInt(_graphRenderer.MaxSampleValueForDisplay);

            _graphLabelTopRect.y = _graphRect.y;
            _graphLabelBottomRect.y = _graphRect.y + GRAPH_HEIGHT - LABEL_HEIGHT;

            GUI.Label(_graphLabelTopRect, maxSampleLabel.ToString());
            GUI.Label(_graphLabelBottomRect, "-" + maxSampleLabel);

            GUILayout.Space(GRAPH_HEIGHT + CONTROL_GAP);

            // Draw x axis scale indicators
            const float SECONDS_INDICATOR_AT = 1f;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(GRAPH_MARGIN_LEFT - 3f + // Position at left border of graph display
                            _animatorControl.TimeStretchFactor * (GRAPH_HEIGHT / (2f * _graphRenderer.MaxSampleValueForDisplay)) * SECONDS_INDICATOR_AT / GraphLineRenderer.SecondsPerGraphUnit); // Shift right by SECONDS_INDICATOR_VALUE graph units
            GUILayout.Label("| " + SECONDS_INDICATOR_AT + " sec (Animation time)");
            EditorGUILayout.EndHorizontal();
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
            _animatorControl = _testRunner.GetComponent<AnimatorControl>();

            Assert.IsNotNull(_poseErrorTracker);
            Assert.IsNotNull(_animatorControl);

            _jointNames = _poseErrorTracker.GetJointNames().ToArray();
            _selectedJointIndex = 0;
        }

        private void DrawTestControlButton()
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

        private void DrawRecordingControlButton()
        {
            switch (_testRunner.State)
            {
                case TestRunner.TestRunnerState.Ready:

                    if (GUILayout.Button("Start Recording", GUILayout.Width(BUTTON_WIDTH)))
                    {
                        _testRunner.StartManualRecord();
                    }

                    break;

                case TestRunner.TestRunnerState.RunningTest:

                    if (GUILayout.Button("Stop Recording", GUILayout.Width(BUTTON_WIDTH)))
                    {
                        _testRunner.StopManualRecord();
                    }

                    break;
            }
        }

        private void DrawAllEvaluations()
        {
            var sb = new StringBuilder();

            foreach (var animToJointToEval in _testRunner.LatestAnimationToJointToEvaluation)
            {
                var eval = animToJointToEval.Value[_jointNames[_selectedJointIndex]];
                GUILayout.Label(GetSingleEvaluationString(sb, string.Format("{0} ({1})", _jointNames[_selectedJointIndex], animToJointToEval.Key), eval));
            }

            GUILayout.Label(GetSingleEvaluationString(sb, "All Joints & Recordings", _testRunner.LatestEvaluation));
        }

        private string GetSingleEvaluationString(StringBuilder sb, string title, PerformanceEvaluation evaluation)
        {
            // Clear the SB
            sb.Length = 0;

            sb.Append("Evaluation: ");
            sb.AppendLine(title);

            sb.AppendFormat("Avg Absolute Error: {0}\n", evaluation.AvgAbsoluteError);
            sb.AppendFormat("Avg Signed Error: {0}\n", evaluation.AvgSignedError);
            sb.AppendFormat("Max Absolute Error: {0}\n", evaluation.MaxAbsoluteError);

            AppendMetricIfNotNull(sb, "Max Overshoot", evaluation.MaxOvershoot);

            AppendMetricIfNotNull(sb, "Avg Settling Time (10 deg)", evaluation.AvgSettlingTime10Degrees);
            AppendMetricIfNotNull(sb, "Avg Settling Time (5 deg)", evaluation.AvgSettlingTime5Degrees);
            AppendMetricIfNotNull(sb, "Avg Settling Time (2 deg)", evaluation.AvgSettlingTime2Degrees);

            AppendMetricIfNotNull(sb, "Avg Response Time (10%)", evaluation.Avg10PercentResponseTime);
            AppendMetricIfNotNull(sb, "Avg Response Time (50%)", evaluation.Avg50PercentResponseTime);
            AppendMetricIfNotNull(sb, "Avg Response Time (Complete)", evaluation.AvgCompleteResponseTime);

            return sb.ToString();
        }

        private void AppendMetricIfNotNull(StringBuilder sb, string metricName, float? metric)
        {
            if (metric.HasValue)
            {
                sb.Append(metricName);
                sb.AppendFormat(": {0}\n", metric.Value);
            }
        }
    }
}