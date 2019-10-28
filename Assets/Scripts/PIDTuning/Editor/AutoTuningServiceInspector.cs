using System.Collections;
using System.Collections.Generic;
using ROSBridgeLib.geometry_msgs;
using UnityEditor;
using UnityEngine;

namespace PIDTuning.Editor
{
    [CustomEditor(typeof(AutoTuningService))]
    public class AutoTuningServiceInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var ats = (AutoTuningService) target;

            if (GUILayout.Button("Tune mixamorig_LeftArm_y"))
            {
                ats.StartCoroutine(ats.TuneSingleJoint("mixamorig_LeftArm_y"));
            }

            if (null != ats.LastTuningData)
            {
                GUILayout.Label("Tunings for " + ats.LastTuningData.Joint);

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

                        configStorage.Configuration.Mapping[ats.LastTuningData.Joint] = variantToTuning.Value;
                        configStorage.TransmitPidConfiguration();
                    }
                }
            }

            Repaint();
        }
    }
}

