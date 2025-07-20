using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIDragZoom : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector3 lastMousePosition;

    private bool isZoom = true;
    private bool isDrag = true;

    private float zoomSpeed = 0.5f;
    private float minScale = 0.5f;
    private float maxScale = 2f;

    public static UIDragZoom Instance;

    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    void Awake()
    {
        Instance = this;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        eventSystem = EventSystem.current;

        raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogWarning("Canvas 上缺少 GraphicRaycaster 组件。");
        }

        if (canvas == null)
        {
            Debug.LogWarning("此脚本需要挂在 Canvas 下的 UI 上。");
        }
    }

    private void OnEnable()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        HandleZoom();
        HandleRightClickDrag();
    }


    void HandleZoom()
    {
        if (!isZoom || !IsPointerAllowed())
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 newScale = rectTransform.localScale + Vector3.one * scroll * zoomSpeed;
            newScale = ClampScale(newScale);
            rectTransform.localScale = newScale;
        }
    }

    void HandleRightClickDrag()
    {
        if (Input.GetMouseButtonDown(2) && isDrag)
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2) && isDrag)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            rectTransform.anchoredPosition += new Vector2(delta.x, delta.y) / canvas.scaleFactor;
            lastMousePosition = Input.mousePosition;
        }
    }

    Vector3 ClampScale(Vector3 scale)
    {
        float clampedX = Mathf.Clamp(scale.x, minScale, maxScale);
        float clampedY = Mathf.Clamp(scale.y, minScale, maxScale);
        return new Vector3(clampedX, clampedY, 1f);
    }

    bool IsPointerAllowed()
    {
        // 如果鼠标没指向任何 UI，允许操作
        if (!EventSystem.current.IsPointerOverGameObject())
            return true;

        // 否则执行更精细的检测：是否指向子物体上的 UI
        pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == null) continue;

            // 检查是否是自己或其子物体
            if (result.gameObject.transform.IsChildOf(transform))
                return true; // 是子物体，允许操作
        }

        // 否则，鼠标是在别的 UI 上
        return false;
    }
}
