using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    /// <summary>
    /// 射线可视化
    /// </summary>
    public interface IPointerVisualize
    {
        void InitVisualization(KmaxPointer pointer);
        void UpdateVisualization(KmaxPointer pointer);
    }

}