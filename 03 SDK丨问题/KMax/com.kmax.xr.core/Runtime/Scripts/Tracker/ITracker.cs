using System;
using UnityEngine;

namespace KmaxXR
{

    /// <summary>
    /// 追踪器接口
    /// </summary>
    public interface IEyeTracker
    {
        /// <summary>
        /// 眼部位置信息
        /// </summary>
        /// <value>眼部位置</value>
        EyePose eyePose { get; }
    }

    public interface IHandTracker
    {
        /// <summary>
        /// 手部姿态信息
        /// </summary>
        /// <value>手部姿态</value>
        HandPose[] handsPose { get; }
    }

    /// <summary>
    /// 眼部位置信息
    /// </summary>
    public struct EyePose
    {
        /// <summary>
        /// 眼部可见性
        /// </summary>
        public bool visible;
        /// <summary>
        /// 相对追踪系统原点的左手坐标系眉心坐标
        /// </summary>
        /// <value>坐标</value>
        public Vector3 head;
        /// <summary>
        /// 注视方向
        /// </summary>
        /// <value>姿态</value>
        public Quaternion look;
        /// <summary>
        /// 相对head的左眼坐标
        /// </summary>
        /// <value>坐标</value>
        public Vector3 left;
        /// <summary>
        /// 相对head的右眼坐标
        /// </summary>
        /// <value>坐标</value>
        public Vector3 right;
    }

    /// <summary>
    /// 手部或输入设备姿态
    /// </summary>
    public struct HandPose
    {
        /// <summary>
        /// 手部可见性
        /// </summary>
        public bool visible;
        /// <summary>
        /// 手部索引
        /// 区分左右手
        /// </summary>
        public int id;
        /// <summary>
        /// 相对追踪系统原点的左手坐标系手部坐标
        /// </summary>
        /// <value>坐标</value>
        public Vector3 pos;
        /// 相对追踪系统原点的左手坐标系手部旋转
        /// </summary>
        /// <value>旋转</value>
        public Quaternion rot;
    }

}
