using UnityEditor;
using UnityEngine;

namespace PIDTuning.Editor
{
    public class TestControlWindow : EditorWindow
    {
        private const float GRAPH_HEIGHT = 200f;
        private const float CONTROL_GAP = 2f;
        private const float BUTTON_WIDTH = 175f;

        private PreviewRenderUtility _graphRenderer;

        private Rect _graphRect;

        private TestRunner _testRunner = null;

        private string[] _jointNames = new[]
        {
            "lhand", "rhand" // etc.
        };

        private void OnEnable()
        {
            _graphRenderer = new PreviewRenderUtility();
            _graphRect = new Rect(0f, 0f, 200f, GRAPH_HEIGHT);
            titleContent = new GUIContent("PID Test Ctrl");
            minSize = new Vector2(200f, 200f); // We define a non-zero minSize so the window cannot disappear
        }

        private void OnDisable()
        {
            _graphRenderer.Cleanup();
        }

        [MenuItem("Window/PID Test Control")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TestControlWindow));
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            _testRunner = EditorGUILayout.ObjectField("PidTestRunner Scene Obj", _testRunner, typeof(TestRunner), true) as TestRunner;

            // For now, we disable all controls until this class is finished
            using (new EditorGUI.DisabledScope(_testRunner == null))
            {
                GUILayout.Space(10f);

                EditorGUIUtility.labelWidth = 65f;
                EditorGUILayout.DelayedTextField("Test Label", string.Empty);
                EditorGUIUtility.labelWidth = 0f; // Reset label width

                MultiPurposeButton();

                EditorGUILayout.EndHorizontal();

                _graphRect.y = GUILayoutUtility.GetLastRect().yMax + CONTROL_GAP;
                _graphRect.width = position.width;

                _graphRenderer.BeginPreview(_graphRect, GUIStyle.none);
                _graphRenderer.camera.Render();
                _graphRenderer.EndAndDrawPreview(_graphRect);

                GUILayout.Space(GRAPH_HEIGHT + CONTROL_GAP);

                EditorGUILayout.Popup(0, _jointNames);
            }
        }

        private void MultiPurposeButton()
        {
            if (null != _testRunner)
            {
                switch (_testRunner.State)
                {
                    case TestRunner.TestRunnerState.NotReady:
                        using (new EditorGUI.DisabledScope(true))
                        {
                            GUILayout.Button("Waiting for Connection...", GUILayout.Width(BUTTON_WIDTH));
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
                            _testRunner.Reset();
                        }

                        break;
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Button("No TestRunner selected", GUILayout.Width(BUTTON_WIDTH));
                }
            }
        }
    }
}