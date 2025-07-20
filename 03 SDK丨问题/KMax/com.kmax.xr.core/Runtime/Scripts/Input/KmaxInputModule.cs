using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    public class KmaxInputModule : StandaloneInputModule
    {
        [Space]
        [SerializeField, Tooltip("fix stereo display mode")]
        private bool mouseOverride = true;

        public bool MouseOverride
        {
            get => mouseOverride; set
            {
                mouseOverride = value;
                inputOverride = null;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!mouseOverride) { inputOverride = null; }
        }
#endif

        public override void Process()
        {
            base.Process();
            ProcessStylusEvent();
        }

        virtual protected bool ProcessStylusEvent()
        {
            OverrideBaseInput();
            foreach (var p in KmaxPointer.Pointers)
            {
                // 主键
                var data = GetStylusEventData(p, PointerEventData.InputButton.Left);
                ProcessStylusPress(data, p.StateOf(PointerEventData.InputButton.Left));
                ProcessStylusMove(data);
                ProcessStylusDrag(data);
                // 处理中键和右键
                var rdata = GetStylusEventData(p, PointerEventData.InputButton.Right, data);
                ProcessStylusPress(rdata, p.StateOf(PointerEventData.InputButton.Right));
                ProcessStylusDrag(rdata);
                var mdata = GetStylusEventData(p, PointerEventData.InputButton.Middle, data);
                ProcessStylusPress(mdata, p.StateOf(PointerEventData.InputButton.Middle));
                ProcessStylusDrag(mdata);
            }
            return KmaxPointer.Enable;
        }

        private PointerEventData GetStylusEventData(KmaxPointer pointer, PointerEventData.InputButton button, PointerEventData fromData = null)
        {
            PointerEventData pData;
            int bid = (int)button;
            bool created = GetPointerData(pointer.Id + bid, out pData, true);
            pData.Reset();
            if (created)
            {
                pData.position = pointer.ScreenPosition;
                pData.button = button;
            }

            if (fromData != null)
            {
                // 从其他按钮数据拷贝
                CopyFromTo(fromData, pData);
            }
            else
            {
                pointer.UpdateState();
                pointer.Raycast(pData);
            }
            return pData;
        }

        protected void ProcessStylusPress(PointerEventData data, PointerEventData.FramePressState state)
        {
            var currentObj = data.pointerCurrentRaycast.gameObject;

            if (state == PointerEventData.FramePressState.Pressed)
            {
                data.eligibleForClick = true;
                data.delta = Vector2.zero;
                data.dragging = false;
                data.useDragThreshold = true;
                data.pressPosition = data.position;
                data.pointerPressRaycast = data.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentObj, data);
                // Debug.Log("point down");
                PointerEventData pointerEvent = data;
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentObj, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObj);

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentObj;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentObj);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }
            else if (state == PointerEventData.FramePressState.Released)
            {
                // Debug.Log("point up");
                PointerEventData pointerEvent = data;
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObj);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentObj, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over something that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentObj != pointerEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentObj);
                }
            }
        }

        protected void ProcessStylusMove(PointerEventData data)
        {
            HandlePointerExitAndEnter(data, data.pointerCurrentRaycast.gameObject);
        }

        protected void ProcessStylusDrag(PointerEventData data)
        {
            ProcessDrag(data);
        }

        /// <summary>
        /// 针对立体显示覆盖输入
        /// </summary>
        virtual protected void OverrideBaseInput()
        {
            if (inputOverride == null && mouseOverride)
            {
                var current = GetComponent<KmaxBaseInput>();
                inputOverride = current == null ? gameObject.AddComponent<KmaxBaseInput>() : current;
            }
        }
    }
}