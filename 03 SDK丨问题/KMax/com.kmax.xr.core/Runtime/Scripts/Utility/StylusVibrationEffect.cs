using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    /// <summary>
    /// 笔的震动效果
    /// </summary>
    public class StylusVibrationEffect : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// 震动强度
        /// </summary>
        [Range(0, 100)]
        public int Intensity;
        [SerializeField]
        protected bool OnHover, OnPress;

        void OnValidate()
        {
            if (OnPress) OnHover = false;
            if (OnHover) OnPress = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!OnHover) return;
            _stylus = KmaxPointer.PointerById(eventData.pointerId) as IVibrate;
            _stylus?.StartVibration(Intensity);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!OnHover) return;
            _stylus?.StopVibration();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!OnPress) return;
            _stylus = KmaxPointer.PointerById(eventData.pointerId) as IVibrate;
            _stylus?.StartVibration(Intensity);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!OnPress) return;
            _stylus?.StopVibration();
        }

        private IVibrate _stylus;
    }
}
