using System;
using System.Collections;
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
    public static event Action<Node> NodeHoverChanged;
    #endregion
    
    //###############| instanced class behavior |###############
    [Header("References")]
    [SerializeField]
    private MeshRenderer visual;
    
    
    public bool IsExit;
    
    [field: SerializeField, ReadOnly, BoxGroup("Info")] public int CostToReach { get; private set; } = int.MaxValue;
    [field: SerializeField, ReadOnly, BoxGroup("Info")] public int CostToLeave { get; private set; } = int.MaxValue;
    public int TotalCost => CostToLeave + CostToReach;

    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private NodeState _currentNodeState = NodeState.InReach;
    public NodeState CurrentNodeState
    {
        get => _currentNodeState;
        private set
        {
            _currentNodeState = value;
            UpdateBaseColor();
        } 
    }
    
    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    private TargetingState currentTargetingState = TargetingState.Idle;
    public TargetingState CurrentTargetingState
    {
        get => currentTargetingState;
        set
        {
            currentTargetingState = value;
            UpdateBaseColor();
        } 
    }

    private bool _previewingOutOfReach;
    public bool PreviewingOutOfReach
    {
        get => _previewingOutOfReach;
        set
        {
            _previewingOutOfReach = value;
            if(_previewingOutOfReach)
                StartCoroutine(PreviewOutOfReach()); // it stops in itself when bool is false
        } 
    }

    private Color idleColor;
    private Color baseColor;
    [SerializeField] private Color outOfReachColor;
    [SerializeField] private Color legalColor;
    [SerializeField] private Color illegalColor;
    
    private void Awake()
    {
        idleColor = visual.material.color;
    }

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
                CurrentNodeState = TotalCost <= MapManager.Instance.CurrentFuel ? NodeState.InReach : NodeState.OutOfReach;
                break;
            case ECostType.CostToLeave:
                CostToLeave = newCost;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
    private void UpdateBaseColor()
    {
        baseColor = CurrentNodeState switch
        {
            NodeState.InReach => idleColor,
            NodeState.OutOfReach => outOfReachColor,
            _ => baseColor
        };

        switch (CurrentTargetingState)
        {
            case TargetingState.Hovered when CurrentNodeState is NodeState.InReach:
                baseColor = ColorUtility.NegativeMultiplyBlend(baseColor, legalColor, 0.4f);
                break;
            case TargetingState.Hovered when CurrentNodeState is NodeState.OutOfReach:
                baseColor = ColorUtility.MultiplyBlend(baseColor, illegalColor, 0.6f);
                break;
            case TargetingState.Targeted when CurrentNodeState is NodeState.InReach:
                baseColor = ColorUtility.NegativeMultiplyBlend(baseColor, legalColor, 0.8f);
                break;
            case TargetingState.Targeted when CurrentNodeState is NodeState.OutOfReach:
                baseColor = ColorUtility.MultiplyBlend(baseColor, illegalColor, 0.9f);
                break;
        }
        
        visual.material.color = baseColor;
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
    private void CycleHighlighting()
    {
        switch (CurrentTargetingState)
        {
            case TargetingState.Idle:
                CurrentTargetingState = TargetingState.Hovered;
                break;
            case TargetingState.Hovered:
                CurrentTargetingState = TargetingState.Targeted;
                break;
            case TargetingState.Targeted:
                CurrentTargetingState = TargetingState.Idle;
                break;
        }
    }
    
    [Button]
    private void TogglePrev()
    {
        PreviewingOutOfReach = !PreviewingOutOfReach;
    }
    #endregion
    
    
    private IEnumerator PreviewOutOfReach()
    {
        float timer = 0;
        
        while (_previewingOutOfReach)
        {
            timer += Time.deltaTime * 4;
            float delta = (Mathf.Sin(timer) + 1) / 2;
            delta *= 0.5f;
            visual.material.color = ColorUtility.MultiplyBlend(baseColor, illegalColor, delta);
            yield return null;
        }

        visual.material.color = baseColor;
    }
    
    public void OnPointerUpAsClick()
    {
        Debug.Log($"Clicked on {gameObject.name}.");
        NodeClicked?.Invoke(this);
    }

    public void OnHoverStart()
    {
        NodeHoverChanged?.Invoke(this);
    }

    public void OnHoverEnd()
    {
        NodeHoverChanged?.Invoke(null);
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

public enum TargetingState : byte
{
    Idle,
    Hovered,
    Targeted
}