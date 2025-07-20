using KmaxXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button3D : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler
{
    [SerializeField]
    public bool verbOnPress;
    [SerializeField, Range(0, 100)]
    public int Intensity = 50;
    [SerializeField]
    public float Duration = 0.3f;
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("begin drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("draging");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end drag");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("button was clicked");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var pointer = KmaxPointer.PointerById(eventData.pointerId);
        if (verbOnPress && pointer is IVibrate stylus)
        {
            stylus.VibrationOnce(Duration, Intensity);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("button exit");
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("button enter");
    }

}
