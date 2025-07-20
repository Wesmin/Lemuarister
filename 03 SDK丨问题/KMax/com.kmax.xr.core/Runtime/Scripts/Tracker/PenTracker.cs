using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KmaxXR
{
    [Serializable]
    public class ReportEvent : UnityEvent<string> { }

    public class PenTracker : BaseTracker, IStylus
    {
        [SerializeField] Transform pen;

        public int buttons { get; private set; }

        public int version { get; private set; }

        private HandPose _handPose;
        public HandPose handPose => _handPose;

        public bool visible => _handPose.visible;

        public Pose pose => new Pose(_handPose.pos, _handPose.rot);

        public ReportEvent OnReport;
        /// <summary>
        /// 操控笔的状态变化事件
        /// </summary>
        public TrackingStatusChangeEvent OnPenStatusChanged;

        internal override void ParseTrackerData(TrackerData data)
        {
            base.ParseTrackerData(data);
            _handPose.visible = data.penVisible > 0;
            _handPose.pos = data.pen.pos;
            _handPose.rot = data.pen.rot;
            HandlePenButtons(data);
        }

        float viewScaler = 1f;
        void Update()
        {
            if (pen != null)
            {
                UpdatePose(pen);
            }
            UpdateTrackingStatus(_handPose.visible ? TrackingStatus.Detected : TrackingStatus.Missing);
            OnReport?.Invoke(ToString());
        }

        public void UpdatePose(Transform pen)
        {
            viewScaler = XRRig.ViewScale;
            pen.gameObject.SetActive(_handPose.visible);
            if (_handPose.visible)
            {
                pen.localPosition = _handPose.pos * viewScaler;
                pen.localRotation = _handPose.rot;
            }
        }

        private void UpdateTrackingStatus(TrackingStatus s)
        {
            if (status != s)
            {
                status = s;
                OnPenStatusChanged?.Invoke(status);
                //Debug.Log($"OnPenStatusChanged:{status}");
            }
        }
        private void HandlePenButtons(TrackerData data)
        {
            buttons = data.penKey;
            //version = data.version;
        }

        StringBuilder reportBuilder = new StringBuilder();
        public override string ToString()
        {
            reportBuilder.Clear();
            reportBuilder.AppendLine($"<b>{GetType().Name}</b>");
            reportBuilder.AppendLine("<color=yellow>[按键]</color>");
            reportBuilder.AppendLine($"按键(左): {GetButton(0)}");
            reportBuilder.AppendLine($"按键(右): {GetButton(1)}");
            reportBuilder.AppendLine($"按键(中): {GetButton(2)}");
            reportBuilder.AppendLine("<color=yellow>[笔]</color>");
            reportBuilder.AppendLine($"位置: {_handPose.pos}");
            reportBuilder.AppendLine($"欧拉角: {_handPose.rot.eulerAngles}");
            reportBuilder.AppendLine($"可见性: {_handPose.visible}");
            return reportBuilder.ToString();
        }

        public bool GetButton(int button)
        {
            return (buttons & (1 << button)) != 0;
        }

        public void Vibrate(int t, int value)
        {
            //var obj = new { time = t, strength = value };
            var obj = new PenShakeCommand() { time = t, strength = value };
            PNClient.SendCommand(PNClient.CommandID.Control, obj);
        }

    }

}