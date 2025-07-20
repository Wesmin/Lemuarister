
using UnityEngine;

namespace KmaxXR
{
    public static class KProvider
    {
        public const int ScriptPriority = -1000;

        #region 视口和显示
        public static Transform Viewport => XRRig.ScreenTrans;
        public static Vector2 WindowSize => XRRig.ViewSize;

        public static float WorldScale => XRRig.ViewScale;

        public static Transform ZeroParallaxPlane => XRRig.ScreenTrans;
        public static Vector3 ZeroParallaxPlaneNormal => XRRig.ScreenTrans.forward;

        #endregion

    }
}
