using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    /// <summary>
    /// 抽象的指针基类
    /// </summary>
    public abstract class KmaxPointer : MonoBehaviour
    {
        [SerializeField] protected Camera eventCamera;
        private static List<KmaxPointer> pointers = new List<KmaxPointer>();

        public static IEnumerable<KmaxPointer> Pointers => pointers;
        public static KmaxPointer PointerById(int id) => pointers.Find(p => p.Contains(id));
        public static bool Enable => pointers.Count > 0;
        public abstract int Id { get; }
        public abstract Vector2 ScreenPosition { get; }
        public abstract Pose StartpointPose { get; }
        public abstract Pose EndpointPose { get; }
        public bool Hit3D { get; protected set; }
        public GameObject GrabObject { get; set; }
        
        /// <summary>
        /// 根据按钮类别获取按钮状态，子类通过改写此方法实现按钮映射。
        /// </summary>
        /// <param name="button">按钮类别</param>
        /// <returns>按钮状态</returns>
        public virtual PointerEventData.FramePressState StateOf(PointerEventData.InputButton button)
        {
            switch (button)
            {
                case PointerEventData.InputButton.Left:
                case PointerEventData.InputButton.Right:
                case PointerEventData.InputButton.Middle:
                default:
                    return PointerEventData.FramePressState.NotChanged;
            }
        }

        /// <summary>
        /// 是否包含指针
        /// </summary>
        /// <param name="pointerId">指针标识</param>
        /// <returns>是否包含该指针</returns>
        public virtual bool Contains(int pointerId)
        {
            return pointerId >= Id && pointerId <= Id + 2;
        }

        virtual protected void OnEnable()
        {
            pointers.Add(this);
            if (eventCamera == null) eventCamera = Camera.main;
        }
        virtual protected void OnDisable()
        {
            pointers.Remove(this);
        }

        public abstract void UpdateState();
        [System.NonSerialized]
        protected List<RaycastResult> raycastResultCache = new List<RaycastResult>();
        public abstract void Raycast(PointerEventData eventData);

        internal static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
        {
            for (var i = 0; i < candidates.Count; ++i)
            {
                if (candidates[i].gameObject == null)
                    continue;

                return candidates[i];
            }
            return new RaycastResult();
        }
    }
}