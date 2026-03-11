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

    [SerializeField] private ConnectorVisualization visualizer;

    public Node Node1, Node2;
    public int MoveCost = 0;
    private bool IsValid => Node1 && Node2 && Node1 != Node2;

    private HighlightState _highlight = HighlightState.NotHighlighted;

    public HighlightState Highlight
    {
        get => _highlight;
        set
        {
            _highlight = value;
            visualizer?.SetHighlight(_highlight);
        }
    }
    
    private void Awake() { MoveCost = Random.Range(1, 7); }
    private void OnEnable() { s_Connectors.Add(this); }
    private void OnDisable() { s_Connectors.Remove(this); }

    private void Start()
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

    private void UpdateSetUp()
    {
        if (!IsValid)
        {
            name = $"Connector";
            return;
        }
        
        transform.position = Node1.transform.position + (Node2.transform.position - Node1.transform.position) / 2;
        name = $"Connector: {Node1.name} - {Node2.name} @ {MoveCost}";
        
        if(Application.isPlaying)
            visualizer?.GenerateMesh(Node1.transform.position, Node2.transform.position);
    }
    
    [Button]
    private void DebugCycleHighlight()
    {
        Highlight = Highlight switch
        {
            HighlightState.NotHighlighted => HighlightState.LightlyHighlighted,
            HighlightState.LightlyHighlighted => HighlightState.Highlighted,
            HighlightState.Highlighted => HighlightState.NotHighlighted,
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
    NotHighlighted,
    LightlyHighlighted,
    Highlighted,
}
