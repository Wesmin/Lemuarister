////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2020 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR.Core
{
    public class KPointerEventData : PointerEventData
    {
        public KPointerEventData(EventSystem eventSystem)
            : base(eventSystem)
        {
            this.Pointer = null;
            this.ButtonId = -1;
            this.Delta3D = Vector3.zero;
            this.IsUIObject = false;
        }

        /// <summary>
        /// A reference to the pointer responsible for dispatching
        /// the pointer event.
        /// </summary>
        public KPointer Pointer { get; set; }

        /// <summary>
        /// The event's associated pointer button id.
        /// </summary>
        public int ButtonId { get; set; }

        /// <summary>
        /// The pointer's 3D position delta since the last update.
        /// </summary>
        public Vector3 Delta3D { get; set; }

        /// <summary>
        /// Is the pointer event in regards to a UI Object
        /// </summary>
        public bool IsUIObject { get; set; }

        /// <summary>
        /// Checks whether the pointer is moving based on its 3D position delta.
        /// </summary>
        /// 
        /// <returns>
        /// True if the pointer is moving. False otherwise.
        /// </returns>
        public bool IsPointerMoving3D()
        {
            return (this.Delta3D.sqrMagnitude > 0);
        }
    }
}
