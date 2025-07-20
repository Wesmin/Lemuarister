using System;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace KmaxXR
{
    public static partial class KmaxNative
    {
        private static bool trackingState = false;
        /// <summary>
        /// 设置追踪状态
        /// </summary>
        /// <param name="enable">开启/关闭追踪</param>
        /// <param name="sbs">是否切换成立体显示模式</param>
        internal static void SetTracking(bool enable, bool sbs)
        {
            if (trackingState == enable) { return; }
            trackingState = enable;
            Debug.Log(enable ? "start tracking" : "stop tracking");
#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass jutil = new AndroidJavaClass("com.kmax.track_conn.AidlConn"))
            {
                //jutil.CallStatic("validate", jo);
                if (enable) jutil.CallStatic("Open", jo, sbs);
                else jutil.CallStatic("Close", jo);
            }
#elif UNITY_STANDALONE// && !UNITY_EDITOR
            InitializeAndDeterminDisplayFunction();
            kxrSetTracking(enable ? 1 : 0);
#if !UNITY_EDITOR
            if (!SoftwareStereoDisplay) kxrSetDisplayMode(sbs ? 1 : 0);
#endif
#endif
        }

        public static string DeviceId
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaClass jutil = new AndroidJavaClass("com.kmax.track_conn.AidlConn"))
                {
                    return jutil.CallStatic<string>("getDeviceId", jo);
                }
#else
                Debug.LogWarning("DeviceId is not support on this platform.");
                return string.Empty;
#endif
            }
        }

        public static Version SDKVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        internal static void Log(string message)
        {
            Debug.Log("<color=green>KmaxXR </color>" + message);
        }

        public const int HighFPS = 120;
        public const int LowFPS = 60;
        public static bool EnableHighFPS
        {
            set
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                Application.targetFrameRate = value ? HighFPS : LowFPS;
#endif
            }
        }
    }
}
