using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR.Core
{
    public class KInputModule : KmaxInputModule
    {
        /// <summary>
        /// 处理笔的事件
        /// </summary>
        /// <returns>是否存在可用的笔</returns>
        protected override bool ProcessStylusEvent()
        {
            OverrideBaseInput();
            return ProcessPointers();
        }

        ////////////////////////////////////////////////////////////////////////
        // Protected Methods
        ////////////////////////////////////////////////////////////////////////

        protected bool ProcessPointers()
        {
            IList<KPointer> pointers = KPointer.GetInstances();

            for (int i = 0; i < pointers.Count; ++i)
            {
                pointers[i].UpdateState();
                this.ProcessPointerEvent(pointers[i]);
            }
            return pointers.Count > 0;
        }

        protected void ProcessPointerEvent(KPointer pointer)
        {
            for (int i = 0; i < pointer.ButtonCount; ++i)
            {
                KPointerEventData eventData = this.GetEventData(pointer, i);

                // Process button press/release events.
                if (pointer.GetButtonDown(i))
                {
                    this.ProcessButtonPress(eventData);
                }

                if (pointer.GetButtonUp(i))
                {
                    this.ProcessButtonRelease(eventData);
                }

                // Process move/scroll events only for the primary button.
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    this.ProcessMove(eventData);
                    this.ProcessScroll(eventData);
                }

                // Process drag events.
                this.ProcessDrag(eventData);
            }
        }

        protected KPointerEventData GetEventData(KPointer pointer, int buttonId)
        {
            int id = pointer.Id + buttonId;

            RaycastResult hitInfo = pointer.HitInfo;

            // Attempt to retrieve the pointer event data. If it doesn't exist,
            // create it.
            KPointerEventData eventData = null;

            if (!this._eventDataCache.TryGetValue(id, out eventData))
            {
                eventData = new KPointerEventData(this.eventSystem);
                eventData.position = hitInfo.screenPosition;
                eventData.pointerId = id; // 新增

                this._eventDataCache.Add(id, eventData);
            }

            // Reset the pointer event data before populating it with latest 
            // information from the pointer.
            eventData.Reset();

            eventData.Pointer = pointer;
            eventData.ButtonId = buttonId;

            if (hitInfo.gameObject != null)
            {
                eventData.IsUIObject =
                    (hitInfo.gameObject.GetComponent<RectTransform>() != null);
            }
            else
            {
                eventData.IsUIObject = false;
            }

            eventData.Delta3D =
                hitInfo.worldPosition -
                eventData.pointerCurrentRaycast.worldPosition;

            eventData.button = pointer.GetButtonMapping(buttonId);
            eventData.delta = hitInfo.screenPosition - eventData.position;
            eventData.position = hitInfo.screenPosition;
            eventData.scrollDelta = pointer.ScrollDelta;
            eventData.pointerCurrentRaycast = hitInfo;

            return eventData;
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Methods
        ////////////////////////////////////////////////////////////////////////

        private void ProcessButtonPress(KPointerEventData eventData)
        {
            GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

            eventData.eligibleForClick = true;
            eventData.delta = Vector2.zero;
            eventData.dragging = false;
            eventData.useDragThreshold = true;
            eventData.pressPosition = eventData.position;
            eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;

            this.DeselectIfSelectionChanged(hitObject, eventData);

            // Attempt to execute pointer down event.
            GameObject pressHandler = ExecuteEvents.ExecuteHierarchy(
                hitObject, eventData, ExecuteEvents.pointerDownHandler);

            // If a pointer down handler could not be found, attempt to
            // grab the hit object's pointer click handler as a fallback.
            if (pressHandler == null)
            {
                pressHandler =
                    ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                        hitObject);
            }

            // Determine the click count.
            float time = Time.unscaledTime;

            if (pressHandler == eventData.lastPress)
            {
                float timeSincePress = time - eventData.clickTime;

                eventData.clickCount =
                    (timeSincePress < eventData.Pointer.ClickTimeThreshold) ?
                        eventData.clickCount + 1 : 1;
            }
            else
            {
                eventData.clickCount = 1;
            }

            // Update the event data's press/click information.
            eventData.clickTime = time;
            eventData.rawPointerPress = hitObject;
            eventData.pointerPress = pressHandler;
            eventData.pointerDrag =
                ExecuteEvents.GetEventHandler<IDragHandler>(hitObject);

            if (eventData.pointerDrag != null)
            {
                ExecuteEvents.Execute(
                    eventData.pointerDrag,
                    eventData,
                    ExecuteEvents.initializePotentialDrag);
            }
        }

        private void ProcessButtonRelease(KPointerEventData eventData)
        {
            GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;
            float timeSincePress = Time.unscaledTime - eventData.clickTime;

            // Execute pointer up event.
            ExecuteEvents.Execute(
                eventData.pointerPress,
                eventData,
                ExecuteEvents.pointerUpHandler);

            GameObject clickHandler =
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject);

            // Execute pointer click and drop events.
            if (eventData.eligibleForClick &&
                (eventData.pointerPress == clickHandler ||
                 timeSincePress < eventData.Pointer.ClickTimeThreshold))
            {
                ExecuteEvents.Execute(
                    eventData.pointerPress,
                    eventData,
                    ExecuteEvents.pointerClickHandler);
            }
            else if (eventData.pointerDrag != null)
            {
                ExecuteEvents.ExecuteHierarchy(
                    hitObject, eventData, ExecuteEvents.dropHandler);
            }

            // Reset the event data's press/click information.
            eventData.eligibleForClick = false;
            eventData.pointerPress = null;
            eventData.rawPointerPress = null;

            // Execute end drag event.
            if (eventData.pointerDrag != null && eventData.dragging)
            {
                ExecuteEvents.Execute(
                    eventData.pointerDrag,
                    eventData,
                    ExecuteEvents.endDragHandler);
            }

            // Reset the event data's drag information.
            eventData.dragging = false;
            eventData.pointerDrag = null;

            // Redo pointer enter/exit to refresh state.
            if (hitObject != eventData.pointerEnter)
            {
                this.HandlePointerExitAndEnter(eventData, null);
                this.HandlePointerExitAndEnter(eventData, hitObject);
            }
        }

        // NOTE: The base ProcessDrag() implementation was originally
        //       being used until there was a need to determine if the input
        //       device's 3D position is changing as well as having the ability 
        //       to associate input scrolling with movement (e.g. scrolling 
        //       moves mouse input in the Z direction). So, reimplementing this 
        //       here to account for 3D movement and scrolling.
        private void ProcessDrag(KPointerEventData eventData)
        {
            bool isPointerActive =
                eventData.IsPointerMoving3D() || eventData.IsScrolling();

            bool shouldStartDrag =
                !eventData.IsUIObject ||
                !eventData.useDragThreshold ||
                this.ShouldStartDrag(
                    eventData.pressPosition,
                    eventData.position,
                    eventSystem.pixelDragThreshold);

            // Execute drag begin event.
            if (shouldStartDrag &&
                eventData.pointerDrag != null &&
                !eventData.dragging)
            {
                ExecuteEvents.Execute(
                    eventData.pointerDrag,
                    eventData,
                    ExecuteEvents.beginDragHandler);

                eventData.dragging = true;
            }

            // Execute drag event.
            if (eventData.dragging &&
                isPointerActive &&
                eventData.pointerDrag != null)
            {
                // Before performing a drag, cancel any pointer down state
                // and clear the current selection.
                if (eventData.pointerPress != eventData.pointerDrag)
                {
                    ExecuteEvents.Execute(
                        eventData.pointerPress,
                        eventData,
                        ExecuteEvents.pointerUpHandler);

                    eventData.eligibleForClick = false;
                    eventData.pointerPress = null;
                    eventData.rawPointerPress = null;
                }

                ExecuteEvents.Execute(
                    eventData.pointerDrag,
                    eventData,
                    ExecuteEvents.dragHandler);
            }
        }

        private void ProcessScroll(KPointerEventData eventData)
        {
            if (!Mathf.Approximately(eventData.scrollDelta.sqrMagnitude, 0))
            {
                GameObject hitObject =
                    eventData.pointerCurrentRaycast.gameObject;

                GameObject scrollHandler =
                    ExecuteEvents.GetEventHandler<IScrollHandler>(hitObject);

                ExecuteEvents.ExecuteHierarchy(
                    scrollHandler, eventData, ExecuteEvents.scrollHandler);
            }
        }

        private bool ShouldStartDrag(
            Vector2 pressPosition, Vector2 currentPosition, float threshold)
        {
            Vector2 deltaPosition = (pressPosition - currentPosition);

            return deltaPosition.sqrMagnitude >= (threshold * threshold);
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Members
        ////////////////////////////////////////////////////////////////////////

        private Dictionary<int, KPointerEventData> _eventDataCache =
            new Dictionary<int, KPointerEventData>();
    }
}
