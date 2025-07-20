////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2020 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace KmaxXR.Core
{
    public class ArrowPointerVisualization : KPointerVisualization
    {
        ////////////////////////////////////////////////////////////////////////
        // Public Methods
        ////////////////////////////////////////////////////////////////////////

        public override void Process(KPointer pointer, float worldScale)
        {
            base.Process(pointer, worldScale);

            this.transform.localPosition =
                Vector3.forward * pointer.HitInfo.distance;

            if (pointer.HitInfo.worldNormal != Vector3.zero)
            {
                this.transform.rotation = Quaternion.LookRotation(
                    Vector3.zero - pointer.HitInfo.worldNormal, Vector3.up);
            }
        }
    }
}
