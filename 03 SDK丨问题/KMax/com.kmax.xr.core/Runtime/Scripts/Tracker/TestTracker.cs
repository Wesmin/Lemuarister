using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Text;

namespace KmaxXR
{
    public class TestTracker : HeadTracker
    {
        private HandPose _handPose;

        public Pose pose => new Pose(_handPose.pos, _handPose.rot);

        [SerializeField]
        Transform glass, pen;

        public override void StartTracking()
        {
            //base.StartTracking();
            KmaxNative.SetTracking(true, false);
        }

        public override void StopTracking()
        {
            //base.StopTracking();
            KmaxNative.SetTracking(false, false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            PNClient.Start();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PNClient.Stop();
        }

        protected override void UpdateTargetPose()
        {
            if (glass != null)
            {
                glass.gameObject.SetActive(eyePose.visible);
                glass.localPosition = eyePose.head;
                glass.localRotation = eyePose.look;
            }
            if (pen != null)
            {
                pen.gameObject.SetActive(_handPose.visible);
                pen.localPosition = _handPose.pos;
                pen.localRotation = _handPose.rot;
            }
        }

        internal override void ParseTrackerData(TrackerData data)
        {
            base.ParseTrackerData(data);
            _handPose.visible = data.penVisible > 0;
            _handPose.pos = data.pen.pos;
            _handPose.rot = data.pen.rot;
        }

    }
}
