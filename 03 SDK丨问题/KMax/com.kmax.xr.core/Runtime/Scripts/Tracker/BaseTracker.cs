using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace KmaxXR
{
    /// <summary>
    /// 追踪状态
    /// </summary>
    public enum TrackingStatus { Missing, Detected }
    /// <summary>
    /// 追踪状态变化事件
    /// </summary>
    [System.Serializable]
    public class TrackingStatusChangeEvent : UnityEvent<TrackingStatus> { };
    /// <summary>
    /// 追踪基本类
    /// </summary>
    public class BaseTracker : MonoBehaviour
    {
        protected TrackingStatus status;
        public TrackingStatus Status => status;
        /// <summary>
        /// 追踪结果的缩放倍率，相对于真实世界的尺寸缩放。
        /// </summary>
        public float ScalingFactor => PNClient.DataFactor;

        virtual protected void OnEnable()
        {
            PNClient.Handlers += ParseTrackerData;
        }

        virtual protected void OnDisable()
        {
            PNClient.Handlers -= ParseTrackerData;
        }

        virtual internal void ParseTrackerData(TrackerData data)
        {

        }

        protected static Vector3 leftVec = new Vector3(-0.03f, 0, 0);
        protected static Vector3 rightVec = new Vector3(0.03f, 0, 0);
    }

}