using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KmaxXR
{
    [CustomEditor(typeof(UIScaler))]
    public class UIScalerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fix Canvas to Virtual Screen"))
                KmaxMenu.FixCanvas();
            EditorGUILayout.EndHorizontal();
        }
    }
}
