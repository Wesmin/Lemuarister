using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KmaxXR.Extensions;

namespace KmaxXR
{
    /// <summary>
    /// 笔尖射线可视化
    /// </summary>
    public class StylusRay : MonoBehaviour, IPointerVisualize
    {
        [SerializeField] protected Transform pointer;
        [SerializeField] protected LineRenderer line;
        [Range(0, 1), Tooltip("射线弯曲的起始位置比例，需要启用末端平滑并确保 line.positionCount > 2 才能生效。")]
        public float CurveStartPivot = 0.35f;
        [SerializeField, Tooltip("没有命中物体时是否要自动隐藏末端。")]
        bool autoHidePointer = true;
        /// <summary>
        /// 初始的射线宽度
        /// </summary>
        private float originWidth;
        /// <summary>
        /// 初始的射线长度
        /// </summary>
        private float originLength;
        private float scaleFactor = 1f;
        private KmaxStylus stylus;

        public void InitVisualization(KmaxPointer pointer)
        {
            if (line == null)
            {
                line = GetComponentInChildren<LineRenderer>();
            }
            float rayLength = 1f;
            if (stylus = pointer as KmaxStylus)
            {
                rayLength = stylus.RayLength;
            }
            originLength = rayLength;
            this.pointer.localPosition = rayLength * Vector3.forward;

            if (line != null)
            {
                line.useWorldSpace = false;
                line.SetPosition(1, rayLength * Vector3.forward);
                originWidth = line.startWidth;
            }
        }

        public void UpdateVisualization(KmaxPointer pointer)
        {
            stylus = pointer as KmaxStylus;
            this.pointer.position = stylus.PointerPosition;
            if (autoHidePointer) this.pointer.gameObject.SetActive(stylus.CurrentHitObject);
            
            if (XRRig.ViewScale != scaleFactor)
            {
                SetLineFactor(XRRig.ViewScale);
            }
            if (line != null) UpdateLineRendererPositions();
        }

        /// <summary>
        /// 设置射线的缩放倍率
        /// 主要影响射线的宽度及长度
        /// </summary>
        /// <param name="factor">倍率</param>
        public void SetLineFactor(float factor)
        {
            if (line != null)
            {
                line.startWidth = line.endWidth = factor * originWidth;
            }
            if (stylus)
                stylus.RayLength = originLength * factor;
            scaleFactor = factor;
        }

        private void UpdateLineRendererPositions()
        {
            // 本地坐标
            Vector3 p0 = Vector3.zero;
            Vector3 p2 = this.pointer.localPosition - line.transform.localPosition;

            if (line.positionCount <= 2)
            {
                line.SetPosition(1, p2);
            }
            else
            {
                Vector3 p1 = p0 + Vector3.Project(p2 - p0, Vector3.forward);
                //line.SetPosition(0, p0);
                line.SetBezierCurve(
                    1, Vector3.Lerp(p0, p1, this.CurveStartPivot), p1, p2);
            }
        }
    }
}
