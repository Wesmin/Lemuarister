﻿////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2020 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace KmaxXR.Extensions
{
    public static class LineRendererExtensions
    {
        ////////////////////////////////////////////////////////////////////////
        // Public Extension Methods
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update the positions of the line renderer to comform to 
        /// a quadratic bezier curve based on the specified control points.
        /// </summary>
        /// 
        /// <param name="p0">
        /// First control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="p1">
        /// Second control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="p2">
        /// Third control point defining the quadratic bezier curve.
        /// </param>
        public static void SetBezierCurve(
            this LineRenderer l, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            l.SetBezierCurve(0, l.positionCount, p0, p1, p2);
        }

        /// <summary>
        /// Update the positions of the line renderer (defined by the specified 
        /// start index) to comform to a quadratic bezier curve based on the 
        /// specified control points.
        /// </summary>
        /// 
        /// <param name="startIndex">
        /// The index of the first position to update.
        /// </param>
        /// <param name="p0">
        /// First control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="p1">
        /// Second control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="p2">
        /// Third control point defining the quadratic bezier curve.
        /// </param>
        public static void SetBezierCurve(this LineRenderer l,
            int startIndex, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            l.SetBezierCurve(startIndex, l.positionCount - startIndex, p0, p1, p2);
        }

        /// <summary>
        /// Update the positions of the line renderer (defined by the specified 
        /// start index and length) to comform to a quadratic bezier curve 
        /// based on the specified control points.
        /// </summary>
        /// 
        /// <param name="startIndex">
        /// The index of the first position to update.
        /// </param>
        /// <param name="length">
        /// The number of positions to update.
        /// </param>
        /// <param name="p0">
        /// First control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="p1">
        /// Second control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="p2">
        /// Third control point defining the quadratic bezier curve.
        /// </param>
        public static void SetBezierCurve(this LineRenderer l,
            int startIndex, int length, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float t = 0;
            float step = 1 / (float)(length - 1);

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Vector3 position = ComputePointOnBezierCurve(p0, p1, p2, t);

                l.SetPosition(i, position);

                t += step;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Static Methods
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Computes a point on a quadratic bezier curve defined by the 
        /// specified control points.
        /// </summary>
        /// 
        /// <param name="p0">
        /// First control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="p1">
        /// Second control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="p2">
        /// Third control point defining the quadratic bezier curve.
        /// </param>
        /// <param name="t">
        /// The value between 0 and 1 (inclusive) defining where along the 
        /// bezier curve to compute the point. A value of 0 corresponds to the
        /// beginning of the curve. A value of 1 corresponds to the end of the
        /// curve.
        /// </param>
        /// 
        /// <returns>
        /// The point on the bezier curve.
        /// </returns>
        public static Vector3 ComputePointOnBezierCurve(
            Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 point =
                (p0 * Mathf.Pow(1 - t, 2)) +
                (p1 * 2 * (1 - t) * t) +
                (p2 * Mathf.Pow(t, 2));

            return point;
        }
    }
}
