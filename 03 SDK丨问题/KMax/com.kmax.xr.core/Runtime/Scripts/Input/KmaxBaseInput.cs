using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    public class KmaxBaseInput : BaseInput
    {
        /// <summary>
        /// 鼠标位置
        /// override mouse position
        /// </summary>
        public override Vector2 mousePosition
        {
            get
            {
                var pos = base.mousePosition;
                FixPosition(ref pos);
                return pos;
            }
        }

        /// <summary>
        /// 触摸信息
        /// override touch position
        /// </summary>
        /// <param name="index">手指id</param>
        /// <returns>触摸信息</returns>
        public override Touch GetTouch(int index)
        {
            var touch = base.GetTouch(index);
            // 是否半画幅左右格式
            // 如果是半画幅左右格式则不需要做触摸转换
            bool half_sbs = Screen.width / Screen.height < 2;
            if (half_sbs) return touch;
            Vector2 pos = touch.position;
            FixPosition(ref pos);
            touch.position = pos;
            return touch;
        }

        /// <summary>
        /// 屏幕分片数量
        /// </summary>
        private const int splitCount = 2;
        /// <summary>
        /// 针对立体显示模式修正输入的位置
        /// </summary>
        /// <param name="position">输入的位置</param>
        static void FixPosition(ref Vector2 pos)
        {
            float fragmentWidth = Screen.width / splitCount;
            pos.x = pos.x % fragmentWidth * splitCount;
        }
    }
}
