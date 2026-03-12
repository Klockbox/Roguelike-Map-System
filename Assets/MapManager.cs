using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapManager : SimpleMonoBehaviorSingleton<MapManager>
{
    [SerializeField] private PlayerMini playerMini;
    [field: SerializeField] public Node PlayerNode { get; private set; }
    [field: SerializeField, ReadOnly] public Node HoveredNode { get; private set; }
    [field: SerializeField, ReadOnly, BoxGroup("Move Preview")] public Node TargetedNode { get; private set; }
    [field: SerializeField, ReadOnly, BoxGroup("Move Preview")] public bool LegalMove { get; private set; }
    [field: SerializeField, ReadOnly, BoxGroup("Move Preview")] public int TargetMoveCost { get; private set; }
    public static event Action<int> OnFuelChanged;
    public static event Action<int> MovePreviewChanged;
    
    [SerializeField, BoxGroup("Fuel")] private int startingFuel;
    [SerializeField, ReadOnly, BoxGroup("Fuel")] private int currentFuel;
    public int CurrentFuel
    {
        get => currentFuel;
        private set
        {
            currentFuel = value;
            OnFuelChanged?.Invoke(currentFuel);
        }
    }
    
    private Dictionary<Node, Waypoint> waypointsToReach = new();
    private Dictionary<Node, Waypoint> waypointsToLeave = new();
    
    private bool CurrentlyAcceptingInput()
    {
        return !PlayerMini.IsMoving;
    }
    
    private void OnEnable()
    {
        Node.NodeClicked += OnNodeClicked;
        Node.NodeHoverChanged += OnHoveringChanged;
        
        PlayerMini.PlayerStartedMove += OnPlayerStartMoving;
        PlayerMini.PlayerReachedNode += OnPlayerReachedNode;
    }

    

    private void OnDisable()
    {
        Node.NodeClicked -= OnNodeClicked;
        Node.NodeHoverChanged -= OnHoveringChanged;
        
        PlayerMini.PlayerStartedMove -= OnPlayerStartMoving;
        PlayerMini.PlayerReachedNode -= OnPlayerReachedNode;
    }
    

    protected override void Awake()
    {
        base.Awake();
        CurrentFuel = startingFuel;
    }

    private void Start()
    {
        playerMini.SetToNode(PlayerNode);
        CalculateCostsToLeave();
        CalculateCostsToReach();
    }

    private void OnPlayerStartMoving()
    {
        CalculateCostsToReach();
        UpdateHoverPreview();
    }

    private void OnPlayerReachedNode(Node node)
    {
        
    }
    
    private Dictionary<Node, Waypoint> CalculateDijkstra(Node startingNode, ECostType costType)
    {
        // set up dict table
        Dictionary<Node, Waypoint> waypoints = Node.s_Nodes.ToDictionary(node => node, node => new Waypoint(node, int.MaxValue, null));

        // set up starting node
        waypoints[startingNode].LowestMoveCost = 0; // no dist because we are already there
        
        List<Waypoint> unvisitedWaypoints = waypoints.Values.ToList();
        
        for (int i = 0; i < waypoints.Count; i++)
        {
            // grab closest waypoint
            Waypoint closestWaypoint = unvisitedWaypoints[0];
            foreach (Waypoint waypointToCheck in unvisitedWaypoints)
                if (waypointToCheck.LowestMoveCost < closestWaypoint.LowestMoveCost) 
                    closestWaypoint = waypointToCheck;
            
            //set base move cost for next check
            int baseMovementCost = closestWaypoint.LowestMoveCost;
            
            // iterate through connections and update lowest costs if applicable
            List<Connector> connections = Connector.GetAllConnectorsFrom(closestWaypoint.Node);
            foreach (Connector connection in connections)
            {
                Node connectedNode = connection.GetOther(closestWaypoint.Node);
                Waypoint waypoint = waypoints[connectedNode];
                int totalCostToConnectedNode = baseMovementCost + connection.MoveCost;
                
                if (totalCostToConnectedNode >= waypoint.LowestMoveCost) continue;
                
                waypoint.LowestMoveCost = totalCostToConnectedNode;
                waypoint.PreviousNode = closestWaypoint.Node;
            }
            unvisitedWaypoints.Remove(closestWaypoint);
        }
        
        // write values to node objects
        foreach (Waypoint waypoint in waypoints.Values)
            waypoint.Node.SetCost(waypoint.LowestMoveCost, costType);
        
        return waypoints;
    }
    
    private void CalculateCostsToReach() => waypointsToReach = CalculateDijkstra(PlayerNode, ECostType.CostToReach);
    private void CalculateCostsToLeave() => waypointsToLeave = CalculateDijkstra(Node.s_ExitNodes[0], ECostType.CostToLeave);
    
    private void OnHoveringChanged(Node hoveredNode)
    {
        HoveredNode = hoveredNode;
        UpdateHoverPreview();
    }

    private void UpdateHoverPreview()
    {
        EndHoverPreview();
        if (HoveredNode && HoveredNode != PlayerNode)
            EvaluateHoveredNode();
        
        MovePreviewChanged?.Invoke(TargetMoveCost);
    }

    private void EvaluateHoveredNode()
    {
        LegalMove = false;
        if(HoveredNode == PlayerNode) return;
        
        bool hoveredNodeIsNeighbor = Connector.TryGetConnector(PlayerNode, HoveredNode, out Connector connectorToNeighbor);
        bool canMoveDirectlyToHoveredNode = 
            hoveredNodeIsNeighbor // is it a neighbor?
            && connectorToNeighbor.MoveCost <= CurrentFuel // can I cover the direct movement cost?
            && HoveredNode.TotalCost <= CurrentFuel; // can you still leave from there?
        
        List<Waypoint> routeBeyondTarget = new ();
        
        if (canMoveDirectlyToHoveredNode)
        {
            TargetedNode = HoveredNode;
        }
        else
        {
            // construct route
            Waypoint evaluatedWaypoint = waypointsToReach[HoveredNode];
            for (int i = 0; i < waypointsToReach.Count; i++)
            {
                if (evaluatedWaypoint.PreviousNode == PlayerNode)
                {
                    TargetedNode = evaluatedWaypoint.Node;
                    break;
                }
                routeBeyondTarget.Add(evaluatedWaypoint);
                evaluatedWaypoint = waypointsToReach[evaluatedWaypoint.PreviousNode];
            }
        }
        
        //evaluate target
        Connector.TryGetConnector(PlayerNode, TargetedNode, out Connector moveConnector);
        TargetMoveCost = moveConnector.MoveCost;
        LegalMove = TargetedNode.TotalCost <= CurrentFuel;
        
        
        
        //highlight connectors and nodes
        TargetedNode.CurrentTargetingState = TargetingState.Targeted;
        if (Connector.TryGetConnector(PlayerNode, TargetedNode, out Connector connectorToTarget))
            connectorToTarget.HighlightState = HighlightState.Highlighted;
        
            
        foreach (Waypoint waypoint in routeBeyondTarget)
        {
            waypoint.Node.CurrentTargetingState = TargetingState.Hovered;
            
            if (Connector.TryGetConnector(waypoint.Node, waypoint.PreviousNode, out Connector connector))
            {
                connector.HighlightState = HighlightState.LightlyHighlighted;
            }
        }
    }

    private void EndHoverPreview()
    {
        TargetedNode = null;
        LegalMove = false;
        TargetMoveCost = 0;

        foreach (Node node in Node.s_Nodes)
            node.CurrentTargetingState = TargetingState.Idle;
        
        foreach (Connector connector in Connector.s_Connectors)
            connector.HighlightState = HighlightState.NotHighlighted;
    }
    
    
    
    private void OnNodeClicked(Node clickedNode)
    {
        if (!LegalMove
            || !CurrentlyAcceptingInput()
            || !Connector.TryGetConnector(PlayerNode, TargetedNode, out Connector connector))
        {
            return;
        } 
        
        PlayerNode = TargetedNode;
        CurrentFuel -= TargetMoveCost;
        PlayerMini.Instance.MoveToNode(TargetedNode);
    }

    public class Waypoint
    {
        public Node Node { get; private set; }
        public int LowestMoveCost;
        public Node PreviousNode;

        public Waypoint(Node node, int dist, Node prevNode)
        {
            Node = node;
            LowestMoveCost = dist;
            PreviousNode = prevNode;
        }
    }
}

public enum ECostType
{
    CostToReach,
    CostToLeave
}
