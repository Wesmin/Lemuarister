////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2020 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using KmaxXR;
using UnityEngine;

using KmaxXR.Extensions;

namespace KmaxXR.Core
{
    public class KStylus : KPointer, IVibrate
    {
        ////////////////////////////////////////////////////////////////////////
        // MonoBehaviour Callbacks
        ////////////////////////////////////////////////////////////////////////

        protected override void Awake()
        {
            base.Awake();

            _target = GetComponentInParent<IStylus>();
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Properties
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The unique id of the stylus pointer.
        /// </summary>
        public override int Id
        {
            get
            {
                //return 0;
                return KmaxStylus.StylusButtnLeft;
            }
        }

        /// <summary>
        /// The current visibility state of the stylus.
        /// </summary>
        public override bool IsVisible
        {
            get
            {
                if (this._target != null)
                {
                    return this._target.visible;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The number of buttons supported by the stylus.
        /// </summary>
        public override int ButtonCount
        {
            get
            {
                if (this._target != null)
                {
                    return 3;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// The current scroll delta of the stylus.
        /// </summary>
        /// 
        /// <remarks>
        /// Since the stylus has not scroll support, the current implementation
        /// is hard-coded to the zero vector.
        /// </remarks>
        public override Vector2 ScrollDelta
        {
            get
            {
                return Vector2.zero;
            }
        }

        /// <summary>
        /// The pose of the stylus in tracker space.
        /// </summary>
        public Pose TrackerPose
        {
            get
            {
                if (this._target != null)
                {
                    //return this._target.Pose;
                    return transform.ToPose();
                }
                else
                {
                    return default(Pose);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Methods
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets whether the specified button is pressed.
        /// </summary>
        /// 
        /// <param name="id">
        /// The integer id of the specified button.
        /// </param>
        /// 
        /// <returns>
        /// True if the specified button is pressed. False otherwise.
        /// </returns>
        public override bool GetButton(int id)
        {
            if (this._target != null)
            {
                int rid = (id + 2) % 3;
                return this._target.GetButton(rid);
                //return this._target.GetButton(id);
            }
            else
            {
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Protected Methods
        ////////////////////////////////////////////////////////////////////////

        protected override void ProcessMove()
        {
            if (this._target == null)
            {
                return;
            }
            // 更新位置
            transform.localPosition = _target.pose.position * KProvider.WorldScale;
            transform.localRotation = _target.pose.rotation;
        }

        public void StartVibration(int intensity)
        {
            _target?.Vibrate(-1, intensity);
        }

        public void StopVibration()
        {
            _target?.Vibrate(0, 0);
        }

        public void VibrationOnce(float t, int s)
        {
            _target?.Vibrate(Mathf.FloorToInt(t * 1000), s);
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Members
        ////////////////////////////////////////////////////////////////////////

        private IStylus _target = null;
    }
}
