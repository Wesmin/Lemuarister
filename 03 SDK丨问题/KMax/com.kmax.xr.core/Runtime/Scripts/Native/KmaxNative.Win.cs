using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KmaxXR
{
#if UNITY_STANDALONE// || UNITY_EDITOR
    public static partial class KmaxNative
    {
        const string CoreDllName = "kXRCore";
        [DllImport(CoreDllName)]
        internal static extern int kxrCreateStereoOverlay(IntPtr left_texture, IntPtr right_texture, int texture_format);
        [DllImport(CoreDllName)]
        internal static extern int kxrGetSupportedVRMode();
        [DllImport(CoreDllName)]
        internal static extern int kxrSetTracking(int s_tracking);
        [DllImport(CoreDllName)]
        internal static extern int kxrSetDisplayMode(int s_display);
        [DllImport(CoreDllName)]
        internal static extern int kxrPenShake(int time, int strength);
        [DllImport(CoreDllName)]
        internal static extern int kxrGetTrackData(ref TrackerData data);

        /// <summary>
        /// 是否采用软件接口立体显示
        /// </summary>
        private static bool softwareStereoDisplay = false;
        public static bool SoftwareStereoDisplay => softwareStereoDisplay;
        private static int initCode = -1;
        /// <summary>
        /// 初始化并确定立体显示方法
        /// </summary>
        /// <returns>初始化结果</returns>
        internal static int InitializeAndDeterminDisplayFunction()
        {
            if (initCode >= 0) return initCode;
            int width = 32, height = 32;
            var Tex = new Texture2D(width, height);
            initCode = kSetup(System.IntPtr.Zero, Tex.GetNativeTexturePtr(), Tex.GetNativeTexturePtr());
            if (initCode == 0)
            {
                softwareStereoDisplay = true;
            }
            else
            {
                Debug.Log($"Do not support FS VR mode. Error code {initCode}.");
            }
            UnityEngine.Object.Destroy(Tex);
            return initCode;
        }

        const string DisplayDllName = "kStereoOverlay";
        [DllImport(DisplayDllName)]
        internal extern static int kSetup(System.IntPtr hwnd, System.IntPtr texture, System.IntPtr texture2);
        [DllImport(DisplayDllName)]
        internal extern static int kSetTexture(System.IntPtr texture, System.IntPtr texture2);
        [DllImport(DisplayDllName)]
        internal extern static System.IntPtr kGetRenderFunc();

        [DllImport(DisplayDllName)]
        internal extern static int kDestroy();
    }
#endif
}
