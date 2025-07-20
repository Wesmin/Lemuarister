using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KmaxXR
{
    public abstract class StereoCamera : MonoBehaviour
    {
        public const float DefaultDistance = 0.5f;

        public abstract Camera CenterCamera { get; }
        protected abstract Camera LeftCamera { get; }
        protected abstract Camera RightCamera { get; }

        internal virtual void Validate() { }

        /// <summary>
        /// 双目聚焦
        /// </summary>
        internal void Converge()
        {
            Converge(XRRig.Screen, XRRig.ViewScale);
        }

        internal void Converge(VirtualScreen screen, float scale = 1f)
        {
            if (screen == null) return;
            var size = screen.Size * scale;
            setScreenParams(size.x, size.y, scale);
            setCameraParams(CenterCamera.nearClipPlane, CenterCamera.farClipPlane);
            SetFrustum(CenterCamera);
            SetFrustum(LeftCamera, CenterCamera.transform.localPosition);
            SetFrustum(RightCamera, CenterCamera.transform.localPosition);
        }

        internal void SetFrustum(Camera cam)
        {
            SetFrustum(cam, Vector3.zero);
        }

        internal void SetFrustum(Camera cam, Vector3 offset)
        {
            if (cam == null) return;
            Matrix4x4 matrix4X4 = Matrix4x4.identity;
            Vector3 pos = cam.transform.localPosition + offset;
            getFrustum(pos, ref matrix4X4);
            cam.projectionMatrix = matrix4X4;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        const string dllName = "__Internal";
#else
        const string dllName = "KmaxStereo";
#endif

        [DllImport(dllName)]
        internal static extern void setScreenParams(float width, float height, float scale);
        [DllImport(dllName)]
        internal static extern void setCameraParams(float nearClip, float farClip);
        [DllImport(dllName)]
        internal static extern void getFrustum(Vector3 pos, ref Matrix4x4 mat4x4);

    }

}