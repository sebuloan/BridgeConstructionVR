using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class SliderEvents : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    // Use events instead of UnityEvent
    public event Action onBeginDrag;
    public event Action onEndDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        onBeginDrag?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEndDrag?.Invoke();
    }
}