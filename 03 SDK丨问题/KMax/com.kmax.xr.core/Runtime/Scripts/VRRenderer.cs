using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KmaxXR
{
    /// <summary>
    /// 将画面渲染成左右格式
    /// render to side by side
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class VRRenderer : StereoCamera
    {
        [SerializeField] Camera leftEye, rightEye;
        Camera centerEye;

        public override Camera CenterCamera => centerEye;
        protected override Camera LeftCamera => leftEye;
        protected override Camera RightCamera => rightEye;

        protected RenderTexture leftTex, rightTex;

        void Awake()
        {
            centerEye = GetComponent<Camera>();
            if (leftEye == null || rightEye == null)
            {
                leftEye = AddRenderCamera("l-eye");
                rightEye = AddRenderCamera("r-eye");
            }
            ViewSplit();

            // set tracking target
            var tracker = GetComponent<HeadTracker>();
            tracker?.SetTrackTarget(centerEye.transform, leftEye.transform, rightEye.transform);
        }

        internal void ViewSplit(int count = 2)
        {
            float uvx = 1.0f / count;
            float px = uvx >= 1 ? (uvx-1) : uvx;
            leftEye.rect = new Rect(0, 0, uvx, 1);
            rightEye.rect = new Rect(px, 0, uvx, 1);
            leftEye.stereoTargetEye = StereoTargetEyeMask.Left;
            rightEye.stereoTargetEye = StereoTargetEyeMask.Right;
            centerEye.enabled = false;
            leftEye.enabled = rightEye.enabled = true;
        }

        internal override void Validate()
        {
            centerEye = GetComponent<Camera>();
            Converge();
        }

        public Texture[] RenderToTexture()
        {
            if (leftTex != null || rightTex != null)
            {
                RenderTexture.ReleaseTemporary(leftTex);
                RenderTexture.ReleaseTemporary(rightTex);
            }
            int width = 1920, height = 1080;
            leftEye.targetTexture = leftTex = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            rightEye.targetTexture = rightTex = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            leftEye.rect = rightEye.rect = new Rect(0, 0, 1, 1);
            return new Texture[] { leftTex, rightTex };
        }

        Camera AddRenderCamera(string name)
        {
            var rc = new GameObject(name);
            rc.transform.SetParent(transform);
            var c = rc.AddComponent<Camera>();
            c.CopyFrom(centerEye);
            return c;
        }

    }
}
