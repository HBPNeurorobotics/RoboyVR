using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ROSBridgeLib.geometry_msgs;
using UnityEditor;
using UnityEngine;

namespace PIDTuning.Editor
{
    [CustomEditor(typeof(AutoTuningService))]
    public class AutoTuningServiceInspector : UnityEditor.Editor
    {
        private string[] jointNames = null;

        private int jointToTuneIdx = 0;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var ats = (AutoTuningService)target;

            if (!Application.isPlaying)
            {
                GUILayout.Label("Functionality only available in play mode");
                return;
            }

            if (null == jointNames)
            {
                Initialize(ats);
            }

            GUILayout.Label("WARNING! Tune all joints rudimentary implementation, should be looked at again.");
            if (GUILayout.Button("Tune all joints"))
            {
                ats.StartCoroutine(ats.TuneAllJoints());
            }

            EditorGUILayout.Space();

            jointToTuneIdx = EditorGUILayout.Popup(jointToTuneIdx, jointNames);

            if (GUILayout.Button("Tune selected joint"))
            {
                ats.StartCoroutine(ats.TuneSingleJoint(jointNames[jointToTuneIdx]));
            }

            EditorGUILayout.Space();

            DisplayAvailableTunings(ats);

            Repaint();
        }

        private static void DisplayAvailableTunings(AutoTuningService ats)
        {
            if (null != ats.LastTuningData)
            {
                GUILayout.Label("Tunings for " + ats.LastTuningData.Joint);

                if (GUILayout.Button("Export as JSON"))
                {
                    var path = EditorUtility.SaveFilePanel("Export Heuristics", Application.dataPath, ats.LastTuningData.Joint + "-heuristics", "json");

                    if (path.Length != 0)
                    {
                        File.WriteAllText(path, ats.LastTuningData.ToJson().ToString());
                    }
                }

                foreach (var variantToTuning in ats.LastTuningData.Tunings)
                {
                    EditorGUILayout.Separator();

                    GUILayout.Label(string.Format("{0}:\n- P: {1}\n- I: {2}\n- D: {3}",
                        variantToTuning.Key,
                        variantToTuning.Value.Kp,
                        variantToTuning.Value.Ki,
                        variantToTuning.Value.Kd));

                    if (GUILayout.Button("Use this tuning"))
                    {
                        var configStorage = ats.gameObject.GetComponent<PidConfigurationStorage>();

                        // We use to copy constructor here to avoid that the user can modify the original tuning
                        configStorage.Configuration.Mapping[ats.LastTuningData.Joint] = new PidParameters(variantToTuning.Value);
                        configStorage.TransmitSingleJointConfiguration(ats.LastTuningData.Joint);
                    }
                }
            }
        }

        private void Initialize(AutoTuningService ats)
        {
            jointNames = ats.PoseErrorTracker.GetJointNames().ToArray();
        }
    }
}

