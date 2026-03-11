using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

public class Node : MonoBehaviour, IClickableObject
{
    //###############| static class behavior |###############
    #region static management
    public static List<Node> s_Nodes = new List<Node>();
    public static List<Node> s_ExitNodes = new List<Node>();
    
    public static event Action<Node> NodeClicked; 
    #endregion
    
    public bool IsExit;
    
    [field: SerializeField, ReadOnly, BoxGroup("Info")] public int CostToReach { get; private set; } = int.MaxValue;
    [field: SerializeField, ReadOnly, BoxGroup("Info")] public int CostToLeave { get; private set; } = int.MaxValue;
    public int TotalCost => CostToLeave + CostToReach;

    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private NodeState _currentState = NodeState.Viable;
    public NodeState CurrentState
    {
        get => _currentState;
        private set
        {
            _currentState = value;
            visual.material.color = _currentState is NodeState.Viable ? viableColor : outOfReachColor;
        } 
    }

    [SerializeField]
    private MeshRenderer visual;
    [SerializeField] private Color viableColor;
    [SerializeField] private Color outOfReachColor;
    
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

    public void SetCost(int newCost, ECostType type)
    {
        switch (type)
        {
            case ECostType.CostToReach:
                CostToReach = newCost;
                CurrentState = TotalCost <= MapManager.Instance.CurrentFuel ? NodeState.Viable : NodeState.OutOfReach;
                break;
            case ECostType.CostToLeave:
                CostToLeave = newCost;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    private void OnDrawGizmos()
    {
        string labelText = $"{name}";
        if (IsExit) labelText += " - Exit";
        labelText += $"\nState: {CurrentState}";
        labelText += $"\nctR: {CostToReach}";
        labelText += $"\nctL: {CostToLeave}";
        labelText += $"\ntC: {TotalCost}";
        
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

public enum NodeState : byte
{
    Viable,
    OutOfReach
}