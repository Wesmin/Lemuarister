using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Text;

namespace KmaxXR
{
    public class HeadTracker : BaseTracker, IEyeTracker
    {
        /// <summary>
        /// 超过3秒没有检测到眼部将触发眼部位置回归
        /// </summary>
        [SerializeField, Range(0, 60)]
        private float WaitForEyeScends = 3f;
        /// <summary>
        /// 眼部追踪丢失回归的速度
        /// </summary>
        [SerializeField, Range(0, 60)]
        private float missBackSpeed = 2f;
        /// <summary>
        /// 是否要在对象销毁时停止追踪
        /// 注意，不建议在立体场景切换到立体场景时开启这个选项
        /// </summary>
        [Tooltip("Enabling this option will cause tracking and stereoscopic display to turn off when switching scenes")]
        public bool StopTrackingOnDestroy = false;

        private EyePose _eyePose;
        public EyePose eyePose => _eyePose;
        public bool EyeVisible => _eyePose.visible;

        private Transform eyeCenter, eyeLeft, eyeRight;
        private StereoCamera _camera;
        /// <summary>
        /// 眼镜追踪状态变化事件
        /// </summary>
        public TrackingStatusChangeEvent OnEyeStatusChanged;

        readonly Vector3 defaultPos = -StereoCamera.DefaultDistance * Vector3.forward;

        protected virtual void Start()
        {
            _eyePose.head = defaultPos;
            _eyePose.left = leftVec;
            _eyePose.right = rightVec;

            PNClient.Start();
            StartTracking();
        }

        private Quaternion headRot = Quaternion.identity;
        private float vscale = 1f;
        private float loseEyeTimer = 0f;
        void Update()
        {
            UpdateTargetPose();
            _camera?.Converge();
        }

        void OnDestroy()
        {
            // 如果你需要在普通场景和立体场景之间来回切换
            // 你可以把这个属性设置成true
            if (StopTrackingOnDestroy) StopTracking();
        }

        /// <summary>
        /// 更新目标的姿态
        /// </summary>
        protected virtual void UpdateTargetPose()
        {
            if (eyeCenter == null) { return; }
            bool scaleChanged = vscale != XRRig.ViewScale;
            vscale = XRRig.ViewScale;
            if (_eyePose.visible)
            {
                eyeCenter.localPosition = _eyePose.head * vscale;
                //eyeCenter.localRotation = _eyePos.look;
                eyeLeft.localPosition = _eyePose.left * vscale;
                eyeRight.localPosition = _eyePose.right * vscale;
                loseEyeTimer = WaitForEyeScends;
                UpdateTrackingStatus(TrackingStatus.Detected);
            }
            else
            {
                if (loseEyeTimer > 0)
                {
                    loseEyeTimer -= Time.deltaTime;
                }
                else
                {
                    float f = Time.deltaTime * missBackSpeed;
                    //f += scaleChanged ? 1 : 0;
                    eyeCenter.localPosition = Vector3.Lerp(transform.localPosition, defaultPos * vscale, f);
                    eyeLeft.localPosition = Vector3.Lerp(eyeLeft.localPosition, Vector3.zero, f);
                    eyeRight.localPosition = Vector3.Lerp(eyeRight.localPosition, Vector3.zero, f);
                    UpdateTrackingStatus(TrackingStatus.Missing);
                }
            }
        }

        /// <summary>
        /// 开始追踪
        /// </summary>
        public virtual void StartTracking()
        {
            KmaxNative.SetTracking(true, true);
        }

        /// <summary>
        /// 停止追踪
        /// </summary>
        public virtual void StopTracking()
        {
            KmaxNative.SetTracking(false, false);
        }

        protected bool isPaused = false;

        void OnApplicationFocus(bool hasFocus)
        {
            isPaused = !hasFocus;
            KmaxNative.SetTracking(!isPaused, !isPaused);
            //Debug.Log($"Application Pause: {isPaused}");
        }

        void OnApplicationPause(bool pauseStatus)
        {
            isPaused = pauseStatus;
            KmaxNative.SetTracking(!isPaused, !isPaused);
            //Debug.Log($"Application Pause: {isPaused}");
        }

        void OnApplicationQuit()
        {
            StopTracking();
            PNClient.Stop();
        }

        /// <summary>
        /// 更新追踪状态
        /// </summary>
        /// <param name="s">状态</param>
        private void UpdateTrackingStatus(TrackingStatus s)
        {
            if (status != s)
            {
                status = s;
                OnEyeStatusChanged?.Invoke(status);
                //Debug.Log($"OnGlassStatusChanged:{status}");
            }
        }

        internal override void ParseTrackerData(TrackerData data)
        {
            base.ParseTrackerData(data);
            var r0 = data.eye;
            // 眼部追踪
            _eyePose.visible = data.eyeVisible > 0;
            // 选择0刚体
            if (_eyePose.visible)
            {
                _eyePose.head = r0.pos;
                headRot = r0.rot;
                var mr = Matrix4x4.Rotate(headRot.normalized);
                _eyePose.look = headRot.normalized;
                _eyePose.left = mr.MultiplyPoint(leftVec * PNClient.DataFactor);
                _eyePose.right = mr.MultiplyPoint(rightVec * PNClient.DataFactor);
            }
        }

        internal void SetTrackTarget(Transform c, Transform l, Transform r)
        {
            eyeCenter = c;
            eyeLeft = l;
            eyeRight = r;
            _camera = c.GetComponent<StereoCamera>();
        }

        StringBuilder sb = new StringBuilder();
        public override string ToString()
        {
            sb.Clear();
            sb.AppendLine($"<b>{GetType().Name}</b>");
            sb.AppendLine($"<color=yellow>[眼]</color>");
            sb.AppendLine($"[visible]: {eyePose.visible}");
            sb.AppendLine($"[pos]: {eyePose.head}");
            sb.AppendLine($"[rot]: {eyePose.look.eulerAngles}");
            return sb.ToString();
        }
    }
}
