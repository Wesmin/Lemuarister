using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KmaxXR
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct NativePose
    {
        public Vector3 pos;
        public Quaternion rot;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct TrackerData
    {
        public int eyeVisible;
        public NativePose eye;
        public int penVisible;
        public NativePose pen;
        public int penKey;
        /// <summary>
        /// 数据版本
        /// </summary>
        public int dataVersion;
        /// <summary>
        /// 帧序号
        /// </summary>
        public int frameId;
        /// <summary>
        /// 追踪对应的屏幕宽度
        /// </summary>
        public float screenWidth;
        /// <summary>
        /// 追踪对应的屏幕高度
        /// </summary>
        public float screenHeight;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct PenData
    {
        public int buttons, version;
        public float yaw, pitch, roll;
        public float x, y, z, w;
    };

    #region Command
    /// <summary>
    /// 连接
    /// </summary>
    [Serializable]
    struct ConnectionCommand
    {
        /// <summary>
        /// SDK大版本号
        /// </summary>
        public int sdkMajor;
        /// <summary>
        /// SDK小版本号
        /// </summary>
        public int sdkMinor;
        /// <summary>
        /// 平台
        /// </summary>
        public int platform;
        /// <summary>
        /// 应用ID，一般为进程id(0-65535)或开发者平台对应id
        /// </summary>
        public int appId;
        /// <summary>
        /// 应用名
        /// </summary>
        public string appName;
    }
    /// <summary>
    /// XR开关控制
    /// </summary>
    [Serializable]
    struct XRModeCommand
    {
        /// <summary>
        /// 是否开启追踪
        /// -1(不更改),0(关闭),1(开启)
        /// </summary>
        public int tracking;
        /// <summary>
        /// 显示模式
        /// -1(不更改),0(常规2D显示),1(立体显示)
        /// </summary>
        public int displayMode;
    }

    /// <summary>
    /// 笔震动控制
    /// </summary>
    [Serializable]
    struct PenShakeCommand
    {
        /// <summary>
        /// 震动时长，单位为ms
        /// </summary>
        public int time;
        /// <summary>
        /// 震动强度(0-100)
        /// 0表示停止震动
        /// </summary>
        public int strength;
    }
    #endregion
}
