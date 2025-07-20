using KmaxXR.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR.Core
{
    public partial class KPointer
    {
        public Camera EventCamera => eventCamera;
        #region 新增实现虚函数
        public override Vector2 ScreenPosition => _hitInfo.screenPosition;
        public override Pose StartpointPose => transform.ToPose();
        public override Pose EndpointPose => EndPointWorldPose;
        public override void UpdateState()
        {
            Process();
        }
        public override void Raycast(PointerEventData eventData) { }
        #endregion
    }
}
