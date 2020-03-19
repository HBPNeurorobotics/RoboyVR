using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PIDTuning.Editor
{
    [CustomEditor(typeof(PidConfigurationStorage))]
    public class PidConfigurationStorageInspector : UnityEditor.Editor
    {
        private float _baseKp = 1000f, _baseKi = 100f, _baseKd = 500f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var configStorage = (PidConfigurationStorage) target;

            var isValidConfiguration = null != configStorage.Configuration;

            if (!Application.isPlaying)
            {
                GUILayout.Label("Due to developer laziness, this component\ncan only be edited in play mode.");
                return;
            }

            if (!isValidConfiguration)
            {
                GUILayout.Label("Configuration doesn't seem to be initialized...\nCheck the Console for errors.");
                return;
            }

            if (GUILayout.Button("Transmit all PIDs"))
            {
                if (configStorage.GetComponent<TestRunner>().State == TestRunner.TestRunnerState.RunningTest)
                {
                    Debug.LogWarning("Cannot transmit PID configuration while test is running");
                }

                configStorage.TransmitFullConfiguration(true);
            }

            if (GUILayout.Button("Fill from JSON"))
            {
                var path = EditorUtility.OpenFilePanel("Locate pid-config.json", Application.dataPath, "json");

                if (path.Length != 0)
                {
                    configStorage.ReplaceWithConfigFromJson(File.ReadAllText(path));
                }
            }

            if (!UserAvatarService.Instance.use_gazebo)
            {
                if (GUILayout.Button("Fill from ConfigurableJoints"))
                {
                    configStorage.ReplaceWithConfigInTemplate(); 
                }
            }

            DrawResetGui(configStorage);

            EditorGUILayout.Space();

            DrawJointPidMapping(configStorage.Configuration, configStorage);

            Repaint();
        }

        private void DrawResetGui(PidConfigurationStorage configStorage)
        {
            GUILayout.Label("The following values can be used to reset\na PID configuration. They don't serve any\nother purpose.");

            _baseKp = EditorGUILayout.FloatField("Base P", _baseKp);
            _baseKi = EditorGUILayout.FloatField("Base I", _baseKi);
            _baseKd = EditorGUILayout.FloatField("Base D", _baseKd);

            if (GUILayout.Button("Reset to base values"))
            {
                configStorage.ResetConfiguration(_baseKp, _baseKi, _baseKd);
                configStorage.TransmitFullConfiguration();
            }
        }

        private void DrawJointPidMapping(PidConfiguration config, PidConfigurationStorage configStorage)
        {
            foreach (var joinToPid in config.Mapping)
            {
                var jointParams = config.Mapping[joinToPid.Key];

                GUILayout.Label(joinToPid.Key);
                jointParams.Kp = EditorGUILayout.FloatField("P", jointParams.Kp);
                jointParams.Ki = EditorGUILayout.FloatField("I", jointParams.Ki);
                jointParams.Kd = EditorGUILayout.FloatField("D", jointParams.Kd);

                if (GUILayout.Button("Transmit Joint PID"))
                {
                    configStorage.TransmitSingleJointConfiguration(joinToPid.Key, configStorage.gameObject.GetComponent<AutoTuningService>().RelayConstantForce);
                }

                EditorGUILayout.Space();
            }
        }
    }
}