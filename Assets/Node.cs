using UnityEngine;

public class Node : MonoBehaviour, IClickableObject
{
    public void OnHoverStart()
    {
        Debug.Log($"Started hovering {gameObject.name}.");
    }

    public void OnHoverEnd()
    {
        Debug.Log($"Stopped hovering {gameObject.name}.");
    }

    public void OnPointerDown()
    {
        Debug.Log($"PointerDownOn {gameObject.name}.");
    }

    public void OnPointerUpAsClick()
    {
        Debug.Log($"Clicked on {gameObject.name}.");
    }
    
    
    public void OnPointerHold() { }
    public void OnPointerUp() { }
    public void OnDragStart(Vector2 pointerPos) { }
    public void OnDrag(Vector2 pointerPos) { }
    public void OnDragEnd(Vector2 pointerPos) { }
    public bool IsDraggable() => true;
}
