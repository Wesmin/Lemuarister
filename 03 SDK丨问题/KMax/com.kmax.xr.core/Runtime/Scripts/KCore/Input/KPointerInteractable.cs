////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2020 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace KmaxXR.Core
{
    public class ZPointerInteractable : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////////
        // Public Methods
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Overrides the pointer's current drag policy for this interactable.
        /// </summary>
        /// 
        /// <param name="pointer">
        /// A reference to the pointer currently interacting with this 
        /// interactable.
        /// </param>
        /// 
        /// <returns>
        /// The interactable's drag policy.
        /// </returns>
        public virtual KPointer.DragPolicy GetDragPolicy(KPointer pointer)
        {
            if (this.GetComponent<RectTransform>() != null)
            {
                return pointer.UIDragPolicy;
            }
            else
            {
                return pointer.ObjectDragPolicy;
            }
        }

        /// <summary>
        /// Get the interactable's specified drag plane.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public virtual Plane GetDragPlane(KPointer pointer)
        {
            if (pointer.DefaultCustomDragPlane != null)
            {
                return pointer.DefaultCustomDragPlane(pointer);
            }

            return default(Plane);
        }
    }
}
