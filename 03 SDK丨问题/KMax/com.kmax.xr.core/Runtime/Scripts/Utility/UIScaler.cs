using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
namespace KmaxXR
{
    [RequireComponent(typeof(Canvas))]
    public class UIScaler : MonoBehaviour
    {
        private int width => resolution.x;
        private int height => resolution.y;
        [SerializeField, Tooltip("pixel size")]
        Vector2Int resolution = new Vector2Int(1920, 1080);
        [SerializeField, Tooltip("Sync transform per frame.\n每帧同步虚拟屏幕位置姿态。")]
        bool syncAlways = true;
        private Canvas canvas;
        /// <summary>
        /// 当前对象上的画布组件
        /// </summary>
        public Canvas ThisCanvas { get {
            if (canvas == null) canvas = GetComponent<Canvas>();
            return canvas;
        } }
        /// <summary>
        /// 是否每帧跟随屏幕位置旋转
        /// </summary>
        public bool SyncAlways { get => syncAlways; set => syncAlways = value; }

        void OnValidate()
        {
            if (width <= 0 || height <= 0)
            resolution = new Vector2Int(1920, 1080);
        }

        void Start()
        {
            AutoFix();
        }

        void Update()
        {
            if (syncAlways) AutoFix();
        }

        [ContextMenu("AutoFix")]
        public void AutoFix()
        {
            FixSize(XRRig.ViewSize);
            FixPose(XRRig.ScreenTrans);
        }

        public void FixSize(Vector2 size)
        {
            RectTransform rt = transform as RectTransform;
            OnValidate();
            var scacleX = size.x / width;
            var scacleY = size.y / height;
            var scaleValue = Mathf.Min(scacleX, scacleY);
            rt.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            rt.sizeDelta = new Vector2(width, height);
        }

        public void FixPose(Pose p)
        {
            transform.position = p.position;
            transform.rotation = p.rotation;
        }

        public void FixPose(Transform t)
        {
            transform.position = t.position;
            transform.rotation = t.rotation;
        }
    }
}
