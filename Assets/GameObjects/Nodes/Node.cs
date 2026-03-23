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
    public static List<Node> s_allNodes = new List<Node>();
    
    public static Node s_ExitNode = null;

    public static List<Node> AllNodesExcept(List<Node> nodesToExclude)
    {
        if (nodesToExclude == null) return s_allNodes;
        return s_allNodes.Except(nodesToExclude).ToList();
    }
    
    public static event Action<Node> NodeClicked;
    public static event Action<Node> AnyNodeHoverChanged;

    public static Dictionary<Node, Waypoint> CurrentMapFromPlayer = new();
    public static Dictionary<Node, Waypoint> CurrentMapFromExit = new();
    
    public static void UpdateCostToReach(Node currentPlayerNode) => UpdateDijkstraCost(currentPlayerNode, ECostType.CostToReach);
     
    public static void UpdateCostToLeave() => UpdateDijkstraCost(s_ExitNode, ECostType.CostToLeave);
    
    private static void UpdateDijkstraCost(Node nodeToCheckFrom, ECostType costType)
    {
        Dictionary<Node, Waypoint> dijkstraMap = DijkstraUtility.CalculateDijkstra(s_allNodes, nodeToCheckFrom);
        
        switch (costType)
        {
            default:
            case ECostType.CostToReach:
                CurrentMapFromPlayer = dijkstraMap;
                break;
            case ECostType.CostToLeave:
                CurrentMapFromExit = dijkstraMap;
                break;
        }

        foreach (Node node in s_allNodes) // set node state according to reachability
            node.UpdateNodeState();
    }

    public static void UpdateNodeInteractability(bool isInteractable)
    {
        foreach (Node node in s_allNodes)
            node.IsInteractable = isInteractable;
    }
    
    #endregion
    
    //###############| instanced class behavior |###############
    [Header("References")]
    [SerializeField]
    private MeshRenderer visual;

    private bool IsExit
    {
        get
        {
            IEncounterNode encounterNode = GetComponent<IEncounterNode>();
            return encounterNode != null && encounterNode.IsExit();
        }
    }

    public int CostToReach => CurrentMapFromPlayer.ContainsKey(this) ? CurrentMapFromPlayer[this].LowestMoveCost : int.MaxValue;
    public int CostToLeave => CurrentMapFromExit.ContainsKey(this) ? CurrentMapFromExit[this].LowestMoveCost : int.MaxValue;
    public int TotalCost => CostToLeave + CostToReach;
    
    // relay
    public MapEncounter GetEncounter() { return GetComponent<IEncounterNode>()?.GetEncounter(); }

    public bool IsEncounterRepeatable()
    {
        IEncounterNode encounter = GetComponent<IEncounterNode>();
        return encounter?.IsRepeatable() ?? false;
    }
    
    // Node State Info
    #region NodeState
    
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
    
    private bool _interactable = true;
    public bool IsInteractable
    {
        get => _interactable;
        private set
        {
            _interactable = value;
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
    
    private bool _isStopover;
    public bool IsStopover
    {
        get => _isStopover;
        set
        {
            _isStopover = value;
            StopoverStateChanged?.Invoke(_isStopover);
        } 
    }
    public event Action<bool> StopoverStateChanged;
    #endregion
    
    
    private void OnEnable()
    {
        s_allNodes.Add(this);
        
        if (!IsExit) return;
        
        if (s_ExitNode is null) 
            s_ExitNode = this;
        else
        {
            Debug.LogWarning("Currently only support one exit node.");
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        s_allNodes.Remove(this);

        if (s_ExitNode == this)
            s_ExitNode = null;
    }

    private void UpdateNodeState()
    {
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
        CurrentTargetState = CurrentTargetState switch
        {
            TargetState.NotTargeted => TargetState.Targeted,
            TargetState.Targeted => TargetState.NotTargeted,
            _ => CurrentTargetState
        };
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
    public void OnAltClickUp() { }
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

/// <summary>
/// <see cref="NotTargeted"/>,
/// <see cref="Targeted"/>,
/// </summary>
public enum TargetState : byte
{
    NotTargeted,
    Targeted
}