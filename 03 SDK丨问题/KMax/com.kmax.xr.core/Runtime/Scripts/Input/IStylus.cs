using System;
using UnityEngine;

namespace KmaxXR
{
    public interface IStylus: IStylusPose
    {
        int buttons { get; }
        int version { get; }

        bool visible { get; }

        /// <summary>
        /// 获取按钮是否按下
        /// </summary>
        /// <param name="button">按钮id</param>
        /// <returns>是否按下</returns>
        bool GetButton(int button);
        void Vibrate(int t, int value);
    }

    public interface IStylusPose
    {
        Pose pose { get; }
        void UpdatePose(Transform target);
    }

    public interface IVibrate
    {
        /// <summary>
        /// 开始震动
        /// </summary>
        /// <param name="intensity">强度0-100</param>
        void StartVibration(int intensity);
        /// <summary>
        /// 停止震动
        /// </summary>
        void StopVibration();
        /// <summary>
        /// 震动一次
        /// </summary>
        /// <param name="t">时长s</param>
        /// <param name="s">强度0-100</param>
        void VibrationOnce(float t, int s);
    }
}
