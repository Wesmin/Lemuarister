using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    /// <summary>
    /// 双指触摸操作扩展
    /// </summary>
    public class StylusDragable2 : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {

        /// <summary>
        /// 对象初始抓取偏移
        /// </summary>
        protected Vector3 _initialObjGrabOffset = Vector3.zero;
        /// <summary>
        /// 对象初始抓取旋转
        /// </summary>
        protected Quaternion _initialObjGrabRotation = Quaternion.identity;
        /// <summary>
        /// 对象初始自身旋转
        /// </summary>
        protected Quaternion _initObjSelfRotation = Quaternion.identity;
        /// <summary>
        /// 对象初始缩放
        /// </summary>
        protected Vector3 _initObjScale = Vector3.one;
        /// <summary>
        /// 初始的触摸位置集合
        /// </summary>
        private Vector2[] _initTouchPositions = new Vector2[0];
        /// <summary>
        /// 对象初始距离
        /// </summary>
        private float _initObjGrabDistance;

        private bool _isKinematic = false;
        private enum Mode { None, Drag, Rotate }
        private enum Source { None, Stylus, Touch, Mouse }
        private Source _souce = Source.None;
        private Mode _mode = Mode.None;

        private float _rotateSpeed = 0.2f;
        private float _scaleSpeed = 0.5f;
        private Camera _eventCamera = null;
        private int _currentPointerId = -1000;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentPointerId > -10) return;
            //Debug.Log($"current pointerId {eventData.pointerId}");
            if (StylusDragable.IsStylusAndPrimary(eventData))
                StylusOnBeginDrag(eventData);
            else if (eventData.pointerId < 0)
                MouseOnBeginDrag(eventData);
            else
                TouchOnBeginDrag(eventData);

            if (_mode != Mode.None && _currentPointerId > -10) RigidbodyFreeze();
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _currentPointerId) return;

            if (_souce == Source.Stylus)
                StylusOnDrag(eventData);
            else if (_souce == Source.Touch)
                TouchOnDrag(eventData);
            else if (_souce == Source.Mouse)
                MouseOnDrag(eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _currentPointerId) return;

            if (_mode != Mode.None) RigidbodyRestore();

            if (_souce == Source.Stylus)
                StylusOnEndDrag(eventData);
            else if (_souce == Source.Touch)
                TouchOnEndDrag(eventData);
            else if (_souce == Source.Mouse)
                MouseOnEndDrag(eventData);

            _mode = Mode.None;
            _souce = Source.None;
            _eventCamera = null;
            _currentPointerId = -1000;
        }

        #region Stylus
        public virtual void StylusOnBeginDrag(PointerEventData eventData)
        {
            _souce = Source.Stylus;
            _mode = Mode.Drag;
            _currentPointerId = eventData.pointerId;

            var pointer = KmaxPointer.PointerById(eventData.pointerId);
            if (pointer == null) return;
            var pose = pointer.EndpointPose;
            // Cache the initial grab state.
            this._initialObjGrabOffset =
                Quaternion.Inverse(this.transform.rotation) *
                (this.transform.position - pose.position);

            this._initialObjGrabRotation =
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
                pose.rotation * this._initialObjGrabRotation;

            // Update the grab object's position.
            this.transform.position =
                pose.position +
                (this.transform.rotation * this._initialObjGrabOffset);
        }
        public virtual void StylusOnEndDrag(PointerEventData eventData)
        {
            var pointer = KmaxPointer.PointerById(eventData.pointerId);
            if (pointer == null) return;
            pointer.GrabObject = null;
        }
        #endregion

        #region Touch
        private Vector2[] GetTouchPositions(BaseInputModule module)
        {
            Vector2[] positions = new Vector2[module.input.touchCount];
            for (int i = 0; i < module.input.touchCount; i++)
                positions[i] = module.input.GetTouch(i).position;
            return positions;
        }
        private Vector2 GetTouchAve(Vector2[] vecs)
        {
            Vector2 v = new Vector2();
            for (int i = 0; i < vecs.Length; i++)
                v += vecs[i];
            return v / vecs.Length;
        }
        private float GetTouchDistance(Vector2[] vecs)
        {
            return (vecs[1] - vecs[0]).magnitude;
        }
        public void TouchOnBeginDrag(PointerEventData eventData)
        {
            _souce = Source.Touch;
            _mode = Mode.None;

            if (eventData.pointerId != 0) return;
            if (eventData.currentInputModule == null) return;

            RaycastResult rayRes = eventData.pointerCurrentRaycast;
            if (rayRes.module == null || rayRes.module.eventCamera == null) return;

            this._mode = Mode.Drag;
            this._currentPointerId = eventData.pointerId;
            this._eventCamera = rayRes.module.eventCamera;

            this._initTouchPositions = GetTouchPositions(eventData.currentInputModule);
            this._initObjGrabDistance = (rayRes.module.eventCamera.transform.position - rayRes.worldPosition).magnitude;
            // 以多指中心点为基准
            Vector2 position = GetTouchAve(this._initTouchPositions);
            Ray ray = this._eventCamera.ScreenPointToRay(new Vector3(position.x, position.y, 1));
            this._initialObjGrabOffset = Quaternion.Inverse(this.transform.rotation) * (this.transform.position - (this._eventCamera.transform.position + ray.direction * _initObjGrabDistance));

            this._initialObjGrabRotation = Quaternion.Inverse(Quaternion.Euler(rayRes.worldNormal)) * this.transform.rotation;
            this._initObjScale = this.transform.localScale;
            this._initObjSelfRotation = this.transform.rotation;
        }
        public void TouchOnDrag(PointerEventData eventData)
        {
            if (_initTouchPositions == null) return;
            if (_eventCamera == null) return;
            if (eventData.pointerId != 0) return;
            if (eventData.currentInputModule == null) return;
            if (eventData.currentInputModule.input.touchCount < this._initTouchPositions.Length) return;

            Vector2[] positions = GetTouchPositions(eventData.currentInputModule);

            // 一个手指旋转 
            if (_initTouchPositions.Length == 1)
            {
                Vector2 delta = positions[0] - this._initTouchPositions[0];
                Quaternion q1 = Quaternion.AngleAxis(delta.x * _rotateSpeed, Vector3.down);
                Quaternion q2 = Quaternion.AngleAxis(delta.y * _rotateSpeed, Vector3.right);
                this.transform.rotation = q2 * q1 * this._initObjSelfRotation;
                return;
            }

            // 多个手指 拖动 多指中心为基准
            Vector2 position = GetTouchAve(positions);
            Ray ray = this._eventCamera.ScreenPointToRay(new Vector3(position.x, position.y, _initObjGrabDistance));
            this.transform.position = (this._eventCamera.transform.position + ray.direction * _initObjGrabDistance) + (this.transform.rotation * this._initialObjGrabOffset);

            //多个手指 缩放
            float d0 = GetTouchDistance(_initTouchPositions);
            float d1 = GetTouchDistance(positions);
            float dx = d1 / d0;
            dx = 1 - _scaleSpeed + _scaleSpeed * dx;
            dx = Mathf.Max(0, dx);
            this.transform.localScale = this._initObjScale * dx;
        }
        public void TouchOnEndDrag(PointerEventData eventData)
        {

        }
        #endregion

        #region Mouse
        public void MouseOnBeginDrag(PointerEventData eventData)
        {
            _souce = Source.Mouse;
            _mode = Mode.None;
            _currentPointerId = eventData.pointerId;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _mode = Mode.Drag;
                RaycastResult rayRes = eventData.pointerCurrentRaycast;
                if (rayRes.module == null || rayRes.module.eventCamera == null) return;
                this._initObjGrabDistance = (rayRes.module.eventCamera.transform.position - rayRes.worldPosition).magnitude;
                this._eventCamera = rayRes.module.eventCamera;
                this._initialObjGrabOffset = Quaternion.Inverse(this.transform.rotation) * (this.transform.position - rayRes.worldPosition);
                this._initialObjGrabRotation = Quaternion.Inverse(Quaternion.Euler(rayRes.worldNormal)) * this.transform.rotation;
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _mode = Mode.Rotate;
                this._initialObjGrabRotation = this.transform.rotation;
                this._initialObjGrabOffset = eventData.position;
            }
        }
        public void MouseOnDrag(PointerEventData eventData)
        {
            if (_mode == Mode.Drag)
            {
                if (this._eventCamera == null) return;
                Ray ray = this._eventCamera.ScreenPointToRay(new Vector3(eventData.position.x, eventData.position.y, _initObjGrabDistance));
                this.transform.position = (this._eventCamera.transform.position + ray.direction * _initObjGrabDistance) + (this.transform.rotation * this._initialObjGrabOffset);
            }
            else if (_mode == Mode.Rotate)
            {
                Vector2 delta = eventData.position - new Vector2(_initialObjGrabOffset.x, _initialObjGrabOffset.y);
                Quaternion q1 = Quaternion.AngleAxis(delta.x * _rotateSpeed, Vector3.down);
                Quaternion q2 = Quaternion.AngleAxis(delta.y * _rotateSpeed, Vector3.right);
                this.transform.rotation = q2 * q1 * this._initialObjGrabRotation;
            }
        }
        public void MouseOnEndDrag(PointerEventData eventData)
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
    }
}