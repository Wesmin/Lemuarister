using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KmaxXR
{
    [System.Serializable]
    public class VirtualScreen
    {
        public enum ScreenType { Screen15_6, Screen27, Screen24 }
        [SerializeField]
        ScreenType screenType;
        
        public float ScreenSizeInch
        {
            get
            {
                switch (screenType)
                {
                    case ScreenType.Screen15_6: return 15.6f;
                    case ScreenType.Screen24: return 24f;
                    case ScreenType.Screen27: return 27f;
                }
                return 15.6f;
            }
        }
        [SerializeField] private Vector2Int ratio = new Vector2Int(16, 9);
        const float INCH2M = 0.0254f;
        float left, right, top, bottom;
        float width, height;
        public float Width => width;
        public float Height => height;
        public Vector2 Size => new Vector2(width, height);

        public Vector3 LeftTop => new Vector3(left, top, 0);
        public Vector3 RightTop => new Vector3(right, top, 0);
        public Vector3 LeftBottom => new Vector3(left, bottom, 0);
        public Vector3 RightBottom => new Vector3(right, bottom, 0);

        public VirtualScreen()
        {
            CalculateRect();
        }

        internal void CalculateRect()
        {
            float size = ScreenSizeInch * INCH2M;
            var widthRatio = ratio.x;
            var heightRatio = ratio.y;
            float sizeRatio = Mathf.Sqrt(widthRatio * widthRatio + heightRatio * heightRatio);
            width = size * widthRatio / sizeRatio;
            height = size * heightRatio / sizeRatio;
            right = width / 2f;
            left = -right;
            top = height / 2f;
            bottom = -top;
        }
    }

}