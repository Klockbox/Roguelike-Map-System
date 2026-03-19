using System;
using System.Collections.Generic;
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
    public static event Action<Node> NodeRightClicked;
    public static event Action<Node> AnyNodeHoverChanged;
    #endregion
    
    //###############| instanced class behavior |###############
    [Header("References")]
    [SerializeField]
    private MeshRenderer visual;
    
    public bool IsExit
    {
        get
        {
            IEncounterNode encounterNode = GetComponent<IEncounterNode>();
            return encounterNode != null && encounterNode.IsExit();
        }
    }
    
    [field: SerializeField, ReadOnly, BoxGroup("Info")] public int CostToReach { get; private set; } = int.MaxValue;
    [field: SerializeField, ReadOnly, BoxGroup("Info")] public int CostToLeave { get; private set; } = int.MaxValue;
    public int TotalCost => CostToLeave + CostToReach;
    
    // relay
    public MapEncounter GetEncounter() { return GetComponent<IEncounterNode>()?.GetEncounter(); }
    
    // Node State Info
    #region NodeState
    public event Action NodeStateChanged;

    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private bool _visited = false;
    public bool Visited 
    {
        get => _visited;
        set
        {
            _visited = value;
            NodeStateChanged?.Invoke();
        }
    }
    
    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private bool _hovered;
    public bool Hovered
    {
        get => _hovered;
        private set
        {
            _hovered = value;
            NodeStateChanged?.Invoke();
        }
    }
    
    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private NodeState _currentNodeState = NodeState.InReach;
    public NodeState CurrentNodeState
    {
        get => _currentNodeState;
        private set
        {
            _currentNodeState = value;
            NodeStateChanged?.Invoke();
        } 
    }
    
    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private RoutingState _currentRoutingState = RoutingState.NotOnRoute;
    public RoutingState CurrentRoutingState
    {
        get => _currentRoutingState;
        set
        {
            _currentRoutingState = value;
            NodeStateChanged?.Invoke();
        } 
    }
    
    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private TargetState _currentTargetState = TargetState.NotTargeted;
    public TargetState CurrentTargetState
    {
        get => _currentTargetState;
        set
        {
            _currentTargetState = value;
            NodeStateChanged?.Invoke();
        } 
    }

    private bool _previewingOutOfReach;
    public bool PreviewingOutOfReach
    {
        get => _previewingOutOfReach;
        set
        {
            _previewingOutOfReach = value;
            NodeStateChanged?.Invoke();
        } 
    }
    
    public event Action NeighboringChanged;
    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private bool _isNeighbor = false;
    public bool IsNeighbor 
    {
        get => _isNeighbor;
        set
        {
            _isNeighbor = value;
            NeighboringChanged?.Invoke();
        }
    }
    #endregion
    
    
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
                break;
            case ECostType.CostToLeave:
                CostToLeave = newCost;
                break;
        }
        
        CurrentNodeState = TotalCost <= MapManager.CurrentFuel ? NodeState.InReach : NodeState.OutOfReach;
    }
    
    #region Debug
    [Button]
    private void ToggleReachability()
    {
        CurrentNodeState = CurrentNodeState switch
        {
            NodeState.InReach => NodeState.OutOfReach,
            NodeState.OutOfReach => NodeState.InReach,
            _ => CurrentNodeState
        };
    }
    
    [Button]
    private void ToggleMarking()
    {
        switch (CurrentRoutingState)
        {
            case RoutingState.NotOnRoute:
                CurrentRoutingState = RoutingState.Marked;
                break;
            case RoutingState.Marked:
                CurrentRoutingState = RoutingState.NotOnRoute;
                break;
        }
    }
    
    [Button]
    private void ToggleNeighbor()
    {
        IsNeighbor = !IsNeighbor;
    }
    
    [Button]
    private void CycleTargeting()
    {
        switch (CurrentTargetState)
        {
            case TargetState.NotTargeted:
                CurrentTargetState = TargetState.RouteEnd;
                break;
            case TargetState.RouteEnd:
                CurrentTargetState = TargetState.Targeted;
                break;
            case TargetState.Targeted:
                CurrentTargetState = TargetState.NotTargeted;
                break;
        }
    }
    
    [Button]
    private void ToggleOutOfReachPreview()
    {
        PreviewingOutOfReach = !PreviewingOutOfReach;
    }
    #endregion
    
    public void OnPointerUpAsClick()
    {
        NodeClicked?.Invoke(this);
    }
    
    public void OnAltClickUp()
    {
        NodeRightClicked?.Invoke(this);
    }

    public void OnHoverStart()
    {
        Hovered = true;
        AnyNodeHoverChanged?.Invoke(this);
    }

    public void OnHoverEnd()
    {
        Hovered = false;
        AnyNodeHoverChanged?.Invoke(null);
    }
    
    private void OnDrawGizmos()
    {
        string labelText = $"{name}";
        if (IsExit) labelText += " - Exit";
        labelText += $"\nState: {CurrentNodeState}";
        labelText += $"\nctR: {CostToReach}";
        labelText += $"\nctL: {CostToLeave}";
        labelText += $"\ntC: {TotalCost}";
        
        Handles.Label(transform.position, labelText, EditorStyles.label);
    }
    
    #region UnusedInterface
    public void OnPointerDown() { }
    public void OnPointerHold() { }
    public void OnPointerUp() { }
    public void OnAltClickDown() { }
    public void OnDragStart(Vector2 pointerPos) { }
    public void OnDrag(Vector2 pointerPos) { }
    public void OnDragEnd(Vector2 pointerPos) { }
    public bool IsDraggable() => true;

    #endregion
    
}

public enum NodeState : byte
{
    InReach,
    OutOfReach
}

public enum RoutingState : byte
{
    NotOnRoute,
    Marked
}

public enum TargetState : byte
{
    NotTargeted,
    RouteEnd,
    Targeted
}