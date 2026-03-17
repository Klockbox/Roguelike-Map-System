using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Connector : MonoBehaviour
{
    //###############| static class behavior |###############
    #region static class behavior
    public static List<Connector> s_Connectors = new List<Connector>();
    public static List<Connector> GetAllConnectorsFrom(Node node) => s_Connectors.Where(connector => connector.Connects(node)).ToList();
    public static List<Node> GetAllConnectedNodesOf(Node node) => GetAllConnectorsFrom(node).Select(c => c.GetOther(node)).ToList();

    public static bool TryGetConnector(Node nodeA, Node nodeB, out Connector connector)
    {
        connector = s_Connectors.FirstOrDefault(c => c.Connects(nodeA) && c.Connects(nodeB));
        return connector != null;
    }
    #endregion

    
    //###############| instanced behavior |###############
    [Header("References")]
    [SerializeField] 
    private ConnectionMeshGenerator meshGenerator;
    [SerializeField] 
    private ConnectorVisuals visuals;

    [Header("Info")] 
    public Node Node1; 
    public Node Node2;
    public int MoveCost;
    private bool IsValid => Node1 && Node2 && Node1 != Node2;

    [SerializeField, ReadOnly]
    private HighlightState _highlightState = HighlightState.Idle;
    public HighlightState HighlightState
    {
        get => _highlightState;
        set
        {
            _highlightState = value;
            HighlightStateChanged?.Invoke(_highlightState);
        }
    }
    public event Action<HighlightState> HighlightStateChanged;

    private void Awake()
    {
        MoveCost = Random.Range(1, 4);
        visuals?.SetCostText(MoveCost);
    }
    private void OnEnable() { s_Connectors.Add(this); }
    private void OnDisable() { s_Connectors.Remove(this); }

    private void OnDestroy()
    {
        HighlightStateChanged = null; // clean all listeners
    }

    private void Start()
    {
        UpdateSetUp();
        meshGenerator?.GenerateMesh(Node1.transform.position, Node2.transform.position);
    }

    private void OnValidate()
    {
        UpdateSetUp();
    }

    /// <summary>Check if the connector is connecting the given <see cref="Node"/>.</summary>
    public bool Connects(Node node) => Node1 == node || Node2 == node; // does this node connect the given node to anything?

    public Node GetOther(Node node)
    {
        if (Node1 == node || Node2 == node)
            return node == Node1 ? Node2 : Node1; // we know its one of the two, so if its 1 return 2, or if it is not node 1 return 1.
        
        Debug.Log($"{name} does not connect {node.name}");
        return null;
    }

    [Button]
    private void UpdateSetUp()
    {
        if (!IsValid)
        {
            name = $"Connector";
            return;
        }
        
        transform.position = Node1.transform.position + (Node2.transform.position - Node1.transform.position) / 2;
        name = $"Connector: {Node1.name} - {Node2.name} @ {MoveCost}";
    }
    
    [Button]
    private void DebugCycleHighlight()
    {
        HighlightState = HighlightState switch
        {
            HighlightState.Idle => HighlightState.Hovered,
            HighlightState.Hovered => HighlightState.Targeted,
            HighlightState.Targeted => HighlightState.Idle,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void OnDrawGizmos()
    {
        if(!IsValid) return;
        Gizmos.DrawLine(Node1.transform.position, Node2.transform.position);
        Handles.Label(transform.position, MoveCost.ToString());
    }
}

public enum HighlightState : byte
{
    Idle,
    Hovered,
    Targeted,
}
