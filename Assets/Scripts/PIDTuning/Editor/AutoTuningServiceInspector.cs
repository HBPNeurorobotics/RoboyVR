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

            if (GUILayout.Button("Tune LeftArm_y"))
            {
                ats.StartCoroutine(ats.TuneSingleJoint("mixamorig_LeftArm_y"));
            }
        }
    }
}

