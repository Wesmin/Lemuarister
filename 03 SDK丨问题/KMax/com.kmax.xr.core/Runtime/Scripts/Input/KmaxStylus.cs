using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KmaxXR
{
    public class KmaxStylus : KmaxPointer, IVibrate
    {
        [SerializeField] protected Transform stylus;
        protected IPointerVisualize pointerVisualize;
        [SerializeField] float rayLength = 1f;
        [SerializeField, Tooltip("平滑的末端端点")]
        protected bool smoothEndPoint = false;
        public bool SmoothEndPoint { get => smoothEndPoint; set => smoothEndPoint = value; }
        [Range(0, 0.1f), Tooltip("末端端点渐变间隔")]
        public float EndPointSmoothTime = 0.02f;
        public LayerMask layer;
        public const int StylusButtnLeft = 1000;
        public const int StylusButtnRigth = StylusButtnLeft + 1;
        public const int StylusButtnCenter = StylusButtnLeft + 2;
        public override int Id => StylusButtnLeft;
        [Tooltip("主键，直接参与交互。")]
        public PointerEventData.InputButton PrimaryKey = PointerEventData.InputButton.Middle;

        protected IStylus pen;
        protected readonly bool[] curState = new bool[] { false, false, false };
        protected readonly bool[] lastState = new bool[] { false, false, false };
        public bool AnyButtonPressed => curState[0] || curState[1] || curState[2];

        [System.Serializable]
        public class RaycastEvent : UnityEvent<GameObject> { }
        private GameObject _enteredObject, _exitedObject;
        [Header("Events")]
        /// <summary>
        /// Event dispatched when the pointer enters an object.
        /// </summary>
        [Tooltip("Event dispatched when the pointer enters an object.")]
        public RaycastEvent OnObjectEntered = new RaycastEvent();

        /// <summary>
        /// Event dispatched when the pointer exits an object.
        /// </summary>
        [Tooltip("Event dispatched when the pointer exits an object.")]
        public RaycastEvent OnObjectExited = new RaycastEvent();

        /// <summary>
        /// Event dispatched when a pointer button becomes pressed.
        /// </summary>
        [Tooltip("Event dispatched when a pointer button becomes pressed.")]
        public UnityEvent<int> OnButtonPressed = new ();

        /// <summary>
        /// Event dispatched when a pointer button becomes released.
        /// </summary>
        [Tooltip("Event dispatched when a pointer button becomes released.")]
        public UnityEvent<int> OnButtonReleased = new ();
        void Start()
        {
            pen = GetComponent<IStylus>();
            if (pen == null)
            {
                Debug.LogError($"Can not find {nameof(IStylus)} on this GameObject.");
            }
            PointerPosition = stylus.position + rayLength * stylus.forward;
            pointerVisualize = stylus.GetComponent<IPointerVisualize>();
            pointerVisualize?.InitVisualization(this);
        }

        void Update()
        {
            if (eventCamera == null) return;
            // Send collision events.
            if (_exitedObject != null)
            {
                OnObjectExited.Invoke(_exitedObject);
            }

            if (_enteredObject != null)
            {
                OnObjectEntered.Invoke(_enteredObject);
            }

            for (int i = 0; i < curState.Length; i++)
            {
                if (lastState[i] != curState[i])
                {
                    if (!lastState[i] && curState[i])
                        OnButtonPressed.Invoke(StylusButtnLeft + i);
                    else
                        OnButtonReleased.Invoke(StylusButtnLeft + i);
                }
            }
        }

        public override PointerEventData.FramePressState StateOf(PointerEventData.InputButton button)
        {
            int i = (int)button;
            bool primaryIsLeft = PrimaryKey == PointerEventData.InputButton.Left;
            // 如果主键不是左键则两个按键的状态互换
            if (!primaryIsLeft)
            {
                if (i == 0) i = (int)PrimaryKey;
                else if (i == (int)PrimaryKey) i = 0;
            }
            return StateOfId(i);
        }

        private PointerEventData.FramePressState StateOfId(int i)
        {
            if (lastState[i] != curState[i])
            {
                return curState[i] ? PointerEventData.FramePressState.Pressed : PointerEventData.FramePressState.Released;
            }
            return PointerEventData.FramePressState.NotChanged;
        }

        public override void UpdateState()
        {
            pen.UpdatePose(stylus);
            for (int i = 0; i < curState.Length; i++)
            {
                lastState[i] = curState[i];
                curState[i] = pen.GetButton(i);
            }
        }

        private RaycastResult result3D, result2D, resultCache;

        /// <summary>
        /// 端点位置世界坐标
        /// </summary>
        public Vector3 PointerPosition { get; private set; }
        private Vector3 smoothPosition, _velocity;
        public GameObject CurrentHitObject { get; private set; }
        public override Vector2 ScreenPosition => eventCamera.WorldToScreenPoint(PointerPosition);

        public override Pose StartpointPose => new Pose(stylus.position, stylus.rotation);
        public override Pose EndpointPose => new Pose(PointerPosition, stylus.rotation);

        public float RayLength { get => rayLength; set => rayLength = value; }

        public override void Raycast(PointerEventData eventData)
        {
            Ray ray = new Ray(stylus.position, stylus.forward);
            resultCache = Hit3D ? result3D : result2D;
            // 3D
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayLength, layer))
            {
                result3D = new RaycastResult
                {
                    gameObject = hit.collider.gameObject,
                    distance = hit.distance,
                    worldPosition = hit.point,
                    worldNormal = hit.normal,
                    screenPosition = eventCamera.WorldToScreenPoint(hit.point),
                    index = 0,
                    sortingLayer = 0,
                    sortingOrder = 0
                };
            }
            else
            {
                result3D = default(RaycastResult);
            }

            // UI
            foreach (var item in KmaxUIRaycaster.UIList)
            {
                item.Raycast(ray, raycastResultCache, rayLength, ~0);
            }
            result2D = FindFirstRaycast(raycastResultCache);
            raycastResultCache.Clear();

            // set event data
            if (result3D.gameObject == null)
            {
                eventData.pointerCurrentRaycast = result2D;
                Hit3D = false;
            }
            else if (result2D.gameObject == null)
            {
                eventData.pointerCurrentRaycast = result3D;
                Hit3D = true;
            }
            else
            {
                Hit3D = result2D.distance > result3D.distance;
                eventData.pointerCurrentRaycast = Hit3D ? result3D : result2D;
            }

            // 更新屏幕位置及差值
            Vector2 curPosition = eventData.pointerCurrentRaycast.screenPosition;
            if (eventData.dragging) // 避免跳变
            {
                var vPosition = stylus.position + eventData.pointerPressRaycast.distance * stylus.forward;
                curPosition = eventCamera.WorldToScreenPoint(vPosition);
            }
            eventData.delta = curPosition - eventData.position;
            eventData.position = curPosition;

            UpdatePointerPosition(eventData);
            ProcessCollisions();
            UpdateVisualization();
        }

        protected virtual void UpdatePointerPosition(PointerEventData eventData)
        {
            // 更新端点位置
            smoothPosition = PointerPosition;
            float d = result3D.gameObject || result2D.gameObject ? eventData.pointerCurrentRaycast.distance : rayLength;
            if (eventData.dragging && GrabObject != null) d = eventData.pointerPressRaycast.distance;
            PointerPosition = stylus.position + d * stylus.forward;

            if (smoothEndPoint)
            {
                PointerPosition = Vector3.SmoothDamp(
                    smoothPosition, PointerPosition, ref _velocity, EndPointSmoothTime);
                //PointerPosition = Vector3.Lerp(previousPosition, PointerPosition, 0.1f); // smooth
            }
        }

        public virtual void UpdateVisualization()
        {
            pointerVisualize?.UpdateVisualization(this);
        }

        private void ProcessCollisions()
        {
            var curObj = CurrentHitObject = Hit3D ? result3D.gameObject : result2D.gameObject;
            // Update the cached entered object.
            _enteredObject = null;

            if (curObj != null &&
                curObj != resultCache.gameObject)
            {
                _enteredObject = curObj;
            }

            // Update the cached exited object.
            _exitedObject = null;

            if (resultCache.gameObject != null &&
                resultCache.gameObject != curObj)
            {
                _exitedObject = resultCache.gameObject;
            }
        }

        /// <summary>
        /// 震动一次
        /// </summary>
        /// <param name="t">时长s</param>
        /// <param name="s">强度0-100</param>
        public void VibrationOnce(float t, int s)
        {
            pen?.Vibrate(Mathf.FloorToInt(t * 1000), s);
        }

        /// <summary>
        /// 开始震动
        /// </summary>
        /// <param name="intensity">强度0-100</param>
        public void StartVibration(int intensity)
        {
            pen?.Vibrate(-1, intensity);
        }

        /// <summary>
        /// 停止震动
        /// </summary>
        public void StopVibration()
        {
            pen?.Vibrate(0, 0);
        }
    }
}
