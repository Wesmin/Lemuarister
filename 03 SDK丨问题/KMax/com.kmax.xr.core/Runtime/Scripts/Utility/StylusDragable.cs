using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    public class StylusDragable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        /// <summary>
        /// 初始抓取偏移
        /// </summary>
        protected Vector3 _initialGrabOffset = Vector3.zero;
        /// <summary>
        /// 初始抓取旋转
        /// </summary>
        protected Quaternion _initialGrabRotation = Quaternion.identity;
        private bool _isKinematic = false;
        private enum Mode { None, Drag, Rotate }
        private enum Source { None, Stylus, Touch, Mouse }
        private Source m_souce = Source.None;
        private Mode m_mode = Mode.None;
        private float m_distance;
        private float m_rotateSpeed = 0.2f;
        private Camera m_eventCamera = null;
        private int m_currentPointerId = -1000;

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (m_currentPointerId > -10) return;
            //Debug.Log($"current pointerId {eventData.pointerId}");
            if (IsStylusAndPrimary(eventData))
                StylusOnBeginDrag(eventData);
            else if (eventData.pointerId < 0)
                MouseOnBeginDrag(eventData);
            else
                TouchOnBeginDrag(eventData);

            if (m_mode != Mode.None && m_currentPointerId > -10) RigidbodyFreeze();
        }
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != m_currentPointerId) return;

            if (m_souce == Source.Stylus)
                StylusOnDrag(eventData);
            else if (m_souce == Source.Touch)
                TouchOnDrag(eventData);
            else if (m_souce == Source.Mouse)
                MouseOnDrag(eventData);
        }
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != m_currentPointerId) return;

            if (m_mode != Mode.None) RigidbodyRestore();

            if (m_souce == Source.Stylus)
                StylusOnEndDrag(eventData);
            else if (m_souce == Source.Touch)
                TouchOnEndDrag(eventData);
            else if (m_souce == Source.Mouse)
                MouseOnEndDrag(eventData);

            m_mode = Mode.None;
            m_souce = Source.None;
            m_eventCamera = null;
            m_currentPointerId = -1000;
        }

        #region Stylus
        public virtual void StylusOnBeginDrag(PointerEventData eventData)
        {
            m_souce = Source.Stylus;
            m_mode = Mode.Drag;
            m_currentPointerId = eventData.pointerId;

            var pointer = KmaxPointer.PointerById(eventData.pointerId);
            if (pointer == null) return;
            var pose = pointer.EndpointPose;
            // Cache the initial grab state.
            this._initialGrabOffset =
                Quaternion.Inverse(this.transform.rotation) *
                (this.transform.position - pose.position);

            this._initialGrabRotation =
                Quaternion.Inverse(pose.rotation) *
                this.transform.rotation;

            pointer.GrabObject = gameObject;
        }
        public virtual void StylusOnDrag(PointerEventData eventData)
        {
            var pointer = KmaxPointer.PointerById(eventData.pointerId);
            if (pointer == null) return;
            var pose = pointer.EndpointPose;

            // Update the grab object's rotation.
            this.transform.rotation =
                pose.rotation * this._initialGrabRotation;

            // Update the grab object's position.
            this.transform.position =
                pose.position +
                (this.transform.rotation * this._initialGrabOffset);
        }
        public virtual void StylusOnEndDrag(PointerEventData eventData)
        {
            var pointer = KmaxPointer.PointerById(eventData.pointerId);
            if (pointer == null) return;
            pointer.GrabObject = null;
        }
        #endregion

        #region Touch
        public virtual void TouchOnBeginDrag(PointerEventData eventData)
        {
            m_souce = Source.Touch;
            m_mode = Mode.None;

            if (eventData.pointerId != 0) return;
            m_currentPointerId = eventData.pointerId;

            if (Input.touchCount == 1)
            {
                m_mode = Mode.Drag;
                RaycastResult rayRes = eventData.pointerCurrentRaycast;
                if (rayRes.module == null || rayRes.module.eventCamera == null) return;
                this.m_distance = (rayRes.module.eventCamera.transform.position - rayRes.worldPosition).magnitude;
                this.m_eventCamera = rayRes.module.eventCamera;
                this._initialGrabOffset = Quaternion.Inverse(this.transform.rotation) * (this.transform.position - rayRes.worldPosition);
                this._initialGrabRotation = Quaternion.Inverse(Quaternion.Euler(rayRes.worldNormal)) * this.transform.rotation;
            }
            else
            {
                m_mode = Mode.Rotate;
                this._initialGrabRotation = this.transform.rotation;
                this._initialGrabOffset = eventData.position;
            }
        }
        public virtual void TouchOnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != 0) return;

            if (m_mode == Mode.Drag)
            {
                if (this.m_eventCamera == null) return;
                Ray ray = this.m_eventCamera.ScreenPointToRay(new Vector3(eventData.position.x, eventData.position.y, m_distance));
                this.transform.position = (this.m_eventCamera.transform.position + ray.direction * m_distance) + (this.transform.rotation * this._initialGrabOffset);
            }
            else if (m_mode == Mode.Rotate)
            {
                Vector2 delta = eventData.position - new Vector2(_initialGrabOffset.x, _initialGrabOffset.y);
                Quaternion q1 = Quaternion.AngleAxis(delta.x * m_rotateSpeed, Vector3.down);
                Quaternion q2 = Quaternion.AngleAxis(delta.y * m_rotateSpeed, Vector3.right);
                this.transform.rotation = q2 * q1 * this._initialGrabRotation;
            }
        }
        public virtual void TouchOnEndDrag(PointerEventData eventData)
        {

        }
        #endregion

        #region Mouse
        public virtual void MouseOnBeginDrag(PointerEventData eventData)
        {
            m_souce = Source.Mouse;
            m_mode = Mode.None;
            m_currentPointerId = eventData.pointerId;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                m_mode = Mode.Drag;
                RaycastResult rayRes = eventData.pointerCurrentRaycast;
                if (rayRes.module == null || rayRes.module.eventCamera == null) return;
                this.m_distance = (rayRes.module.eventCamera.transform.position - rayRes.worldPosition).magnitude;
                this.m_eventCamera = rayRes.module.eventCamera;
                this._initialGrabOffset = Quaternion.Inverse(this.transform.rotation) * (this.transform.position - rayRes.worldPosition);
                this._initialGrabRotation = Quaternion.Inverse(Quaternion.Euler(rayRes.worldNormal)) * this.transform.rotation;
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                m_mode = Mode.Rotate;
                this._initialGrabRotation = this.transform.rotation;
                this._initialGrabOffset = eventData.position;
            }
        }
        public virtual void MouseOnDrag(PointerEventData eventData)
        {
            if (m_mode == Mode.Drag)
            {
                if (this.m_eventCamera == null) return;
                Ray ray = this.m_eventCamera.ScreenPointToRay(new Vector3(eventData.position.x, eventData.position.y, m_distance));
                this.transform.position = (this.m_eventCamera.transform.position + ray.direction * m_distance) + (this.transform.rotation * this._initialGrabOffset);
            }
            else if (m_mode == Mode.Rotate)
            {
                Vector2 delta = eventData.position - new Vector2(_initialGrabOffset.x, _initialGrabOffset.y);
                Quaternion q1 = Quaternion.AngleAxis(delta.x * m_rotateSpeed, Vector3.down);
                Quaternion q2 = Quaternion.AngleAxis(delta.y * m_rotateSpeed, Vector3.right);
                this.transform.rotation = q2 * q1 * this._initialGrabRotation;
            }
        }
        public virtual void MouseOnEndDrag(PointerEventData eventData)
        {

        }
        #endregion

        private void RigidbodyFreeze()
        {
            var rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                this._isKinematic = rigidbody.isKinematic;
                rigidbody.isKinematic = true;
            }
        }
        private void RigidbodyRestore()
        {
            var rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = this._isKinematic;
            }
        }

        public static bool IsStylusAndPrimary(PointerEventData eventData)
        {
            return IsStylus(eventData.pointerId) && eventData.button == PointerEventData.InputButton.Left;
        }

        public static bool IsStylus(int pointerId)
        {
            return KmaxPointer.PointerById(pointerId);
        }
    }
}