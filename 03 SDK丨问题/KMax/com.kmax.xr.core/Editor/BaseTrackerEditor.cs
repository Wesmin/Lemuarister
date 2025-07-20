using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KmaxXR
{
    [CustomEditor(typeof(BaseTracker), true)]
    public class BaseTrackerEditor : Editor
    {
        public override bool HasPreviewGUI()
        {
            return Application.isPlaying;
        }

        private GUIStyle m_PreviewLabelStyle;

        protected GUIStyle previewLabelStyle
        {
            get
            {
                if (m_PreviewLabelStyle == null)
                {
                    m_PreviewLabelStyle = new GUIStyle("PreOverlayLabel")
                    {
                        richText = true,
                        alignment = TextAnchor.UpperLeft,
                        fontStyle = FontStyle.Normal
                    };
                }

                return m_PreviewLabelStyle;
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            var tracker = target as BaseTracker;
            if (tracker == null)
                return;

            GUI.Label(rect, tracker.ToString(), previewLabelStyle);
        }
    }
}
