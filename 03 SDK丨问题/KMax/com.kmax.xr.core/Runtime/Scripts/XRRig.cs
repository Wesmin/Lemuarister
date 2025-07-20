using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KmaxXR
{
    [DefaultExecutionOrder(ScriptPriority)]
    public class XRRig : MonoBehaviour
    {
        public const int ScriptPriority = -100;
        [SerializeField, Header("View")]
        protected VirtualScreen screen;
        [SerializeField, Range(0.001f, 1000f)]
        protected float viewScale = 1f;
        [SerializeField, Header("Camera")]
        protected StereoCamera stereoCamera;
        public StereoCamera StereoRender => stereoCamera;
        protected event System.Action<float> onViewScaleChanged;

        private static XRRig rig;
        void Awake()
        {
            if (rig == null)
            {
                KmaxNative.Log($"SDK Version: {KmaxNative.SDKVersion}");
                KmaxNative.EnableHighFPS = true; // for Android
            }
            rig = this;
        }

        void OnValidate()
        {
            rig = this;
            screen.CalculateRect();
            AdjustCameraPosition();
            stereoCamera?.Validate();
        }

        public static VirtualScreen Screen => rig.screen;
        public static Vector3[] ScreenCorners => rig.GetScreenCorners();
        /// <summary>
        /// 虚拟屏幕位置及姿态，等同于对象本身的位置及姿态。
        /// </summary>
        public static Transform ScreenTrans => rig.transform;
        /// <summary>
        /// 视窗比例改变事件。
        /// </summary>
        public static event System.Action<float> OnViewScaleChanged
        {
            add
            {
                rig.onViewScaleChanged += value;
            }
            remove
            {
                rig.onViewScaleChanged -= value;
            }
        }

        /// <summary>
        /// 视窗缩放比例，默认为1。
        /// </summary>
        public static float ViewScale
        {
            get => rig.viewScale;
            set
            {
                rig.viewScale = value;
                rig.onViewScaleChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// 视窗尺寸，表示虚拟屏幕的大小。
        /// </summary>
        public static Vector2 ViewSize => rig.screen.Size * ViewScale;

        /// <summary>
        /// 获取屏幕四个角点的位置。
        /// </summary>
        /// <returns>屏幕四角位置(世界坐标)</returns>
        public Vector3[] GetScreenCorners()
        {
            Vector3[] cs = new Vector3[] {
                screen.LeftTop * viewScale,
                screen.LeftBottom * viewScale,
                screen.RightBottom * viewScale,
                screen.RightTop * viewScale,
            };
            for (int i = 0; i < cs.Length; i++)
            {
                cs[i] = transform.localToWorldMatrix.MultiplyPoint(cs[i]);
            }
            return cs;
        }

        void AdjustCameraPosition()
        {
            if (stereoCamera == null) return;
            stereoCamera.transform.localPosition =
                -StereoCamera.DefaultDistance * Vector3.forward * viewScale;
        }
        
        /// <summary>
        /// 切换视口显示模式
        /// </summary>
        /// <param name="sbs">是否左右显示</param>
        internal void SwitchViewMode(bool sbs)
        {
            if (!Application.isPlaying) return;
            var inputModule = FindObjectOfType<KmaxInputModule>();
            if (inputModule != null)
                inputModule.MouseOverride = sbs;
            if (stereoCamera != null && stereoCamera is VRRenderer renderer)
                renderer.ViewSplit(sbs ? 2 : 1);
        }

#if UNITY_EDITOR

        [SerializeField, Header("Editor")]
        bool alwaysShowGizmos = true;
        bool debugMode = true;
        public bool DebugMode
        {
            get => debugMode; set
            {
                debugMode = value;
                SwitchViewMode(!value);
            }
        }

        void Start()
        {
            SwitchViewMode(!debugMode);
        }

        void OnDrawGizmos()
        {
            if (alwaysShowGizmos)
            {
                DrawVisualScreen();
                DrawFrustum();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!alwaysShowGizmos)
            {
                DrawVisualScreen();
                DrawFrustum();
            }
        }

        private readonly Color color_screen = new Color(1f, 1f, 1f, 0f);
        private readonly Color color_screen_frame = new Color(0f, 1f, 0f, 0.5f);
        void DrawVisualScreen()
        {
            var cs = GetScreenCorners();
            if (cs == null) return;
            Handles.DrawSolidRectangleWithOutline(cs, color_screen, color_screen_frame);
            Handles.Label(cs[0], nameof(VirtualScreen));
        }

        private readonly Rect view_port = new Rect(0, 0, 1, 1);
        private readonly Vector3[] near = new Vector3[4];
        private readonly Vector3[] far = new Vector3[4];
        void DrawFrustum()
        {
            if (stereoCamera == null) return;

            // calculate by camera api
            var cam = stereoCamera.CenterCamera;
            if (cam == null) cam = stereoCamera.GetComponent<Camera>();
            if (!Application.isPlaying) stereoCamera.Converge();
            cam.CalculateFrustumCorners(view_port, 0.37f * viewScale, Camera.MonoOrStereoscopicEye.Mono, near);
            cam.CalculateFrustumCorners(view_port, 0.8f * viewScale, Camera.MonoOrStereoscopicEye.Mono, far);

            Handles.matrix = stereoCamera.transform.localToWorldMatrix;
            void DrawComfortZone(Vector3[] startCorners, Vector3[] endCorners)
            {
                var lineColor = new Color(1f, 1f, 1f, 0.5f);
                Handles.DrawSolidRectangleWithOutline(startCorners, Color.clear, lineColor);
                Handles.DrawSolidRectangleWithOutline(endCorners, Color.clear, lineColor);
                Handles.color = lineColor;
                for (int i = 0; i < startCorners.Length; ++i)
                {
                    Handles.DrawLine(startCorners[i], endCorners[i]);
                }
            }
            DrawComfortZone(near, far);
        }
#elif UNITY_WEBGL
        void Start()
        {
            SwitchViewMode(false);
        }
#endif

    }
}
