using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KmaxXR
{
    [CustomEditor(typeof(XRRig))]
    public class XRRigEditor : Editor
    {
        XRRig rig;
        void OnEnable()
        {
            rig = (XRRig)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox($"Screen Size: {XRRig.Screen.Size:F3}\nView Size: {XRRig.ViewSize:F3}", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            bool d = rig.DebugMode;
            if (GUILayout.Button(d ? "Switch to Side By Side" : "Switch to Normal"))
                rig.DebugMode = !d;
            EditorGUILayout.EndHorizontal();
        }
    }

}
