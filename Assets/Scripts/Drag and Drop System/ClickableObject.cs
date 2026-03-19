using System;
using NaughtyAttributes;
using UnityEngine;

public interface IClickableObject
{
    void OnHoverStart();
    void OnHoverEnd();
    
    void OnPointerDown();
    void OnPointerHold();
    void OnPointerUp();
    
    /// <summary><para>This is getting called when the pointer is released on the same object as the pointerDown event was registered on.</para>
    /// <para>This is called before the OnDragEnd event.</para></summary>
    void OnPointerUpAsClick();
    
    void OnAltClickDown();
    void OnAltClickUp();
    
    void OnDragStart(Vector2 pointerPos);
    void OnDrag(Vector2 pointerPos);
    void OnDragEnd(Vector2 pointerPos);
    bool IsDraggable();
}