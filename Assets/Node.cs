using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Node : MonoBehaviour, IClickableObject
{
    #region static management
    public static List<Node> s_Nodes = new List<Node>();
    public static List<Node> s_ExitNodes = new List<Node>();
    
    public static event Action<Node> NodeClicked; 
    #endregion
    
    public bool IsExit;
    
    [field: SerializeField] public int CostToReach { get; private set; } = int.MaxValue;
    [field: SerializeField] public int CostToLeave { get; private set; } = int.MaxValue;

    private void OnEnable()
    {
        s_Nodes.Add(this);
        if (IsExit) s_ExitNodes.Add(this);
    }

    private void OnDisable()
    {
        s_Nodes.Remove(this);
        if (IsExit) s_ExitNodes.Remove(this);
    }
    
    public void SetCostToReach(int newCost) { CostToReach = newCost; }
    public void SetCostToLeave(int newCost) { CostToLeave = newCost; }
    private void OnDrawGizmos()
    {
        EditorStyles.label.fontSize = 16;
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorStyles.label.alignment = TextAnchor.LowerCenter;
        
        string labelText = $"{name}";
        if (IsExit) labelText += " - Exit";
        labelText += $"\nctR: {CostToReach}";
        labelText += $"\nctL: {CostToLeave}";
        Handles.Label(transform.position, labelText, EditorStyles.label);
    }
    
    public void OnPointerUpAsClick()
    {
        Debug.Log($"Clicked on {gameObject.name}.");
        NodeClicked?.Invoke(this);
    }
    
    public void OnHoverStart() { }

    public void OnHoverEnd() { }

    public void OnPointerDown() { }
    
    public void OnPointerHold() { }
    public void OnPointerUp() { }
    public void OnDragStart(Vector2 pointerPos) { }
    public void OnDrag(Vector2 pointerPos) { }
    public void OnDragEnd(Vector2 pointerPos) { }
    public bool IsDraggable() => true;
}
