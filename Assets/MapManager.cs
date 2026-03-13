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
    
    private static Dictionary<Node, Waypoint> CalculateDijkstra(Node startingNode)
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
        
        return waypoints;
    }
    
    private void CalculateCostsToReach()
    {
        waypointsToReach = CalculateDijkstra(PlayerNode);
        
        // write values to node objects
        foreach (Waypoint waypoint in waypointsToReach.Values)
            waypoint.Node.SetCost(waypoint.LowestMoveCost, ECostType.CostToReach);
    } 
    private void CalculateCostsToLeave()
    {
        waypointsToLeave = CalculateDijkstra(Node.s_ExitNodes[0]);
        // write values to node objects
        foreach (Waypoint waypoint in waypointsToLeave.Values)
            waypoint.Node.SetCost(waypoint.LowestMoveCost, ECostType.CostToLeave);
    }

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
        
        // check if we can ignore routing
        bool canMoveDirectlyToHoveredNode = 
            hoveredNodeIsNeighbor // is it a neighbor?
            && connectorToNeighbor.MoveCost <= CurrentFuel // can I cover the direct movement cost?
            && connectorToNeighbor.MoveCost + HoveredNode.CostToLeave <= CurrentFuel; // can you still leave from there?

        List<Waypoint> route = new List<Waypoint>();
        if (canMoveDirectlyToHoveredNode)
        {
            route.Add(new Waypoint(HoveredNode, connectorToNeighbor.MoveCost, PlayerNode));
        }
        else
        {
            // construct route
            Waypoint evaluatedWaypoint = waypointsToReach[HoveredNode];
            for (int i = 0; i < waypointsToReach.Count; i++)
            {
                route.Add(evaluatedWaypoint);
                evaluatedWaypoint = waypointsToReach[evaluatedWaypoint.PreviousNode];
                
                // break when we reach player node
                if (evaluatedWaypoint.Node == PlayerNode)
                    break;
            }

            route.Reverse(); // sort it that neighbor target is [0]
        }
        
        
        
        for (int i = 0; i < route.Count; i++)
        {
            Node evaluatedNode = route[i].Node;
            Node precedingNode = route[i].PreviousNode;
            
            if (i == 0)
            {
                // target
                Connector.TryGetConnector(evaluatedNode, precedingNode, out Connector targetConnector);
                TargetedNode = evaluatedNode;
                TargetMoveCost = targetConnector.MoveCost;
                LegalMove = TargetMoveCost + TargetedNode.CostToLeave <= CurrentFuel;
        
                //highlight connectors and nodes
                TargetedNode.CurrentTargetingState = TargetingState.Targeted;
                targetConnector.HighlightState = HighlightState.Targeted;
                
                continue;
            }
            
            //hovered
            evaluatedNode.CurrentTargetingState = TargetingState.Hovered;
            if (Connector.TryGetConnector(evaluatedNode, precedingNode, out Connector connector))
                connector.HighlightState = HighlightState.Hovered;
        }
        
        
        // out of reach preview
        // find furthest legal target
        Node furthestLegalTarget = null;
        // I think, I have to track this separately because otherwise it would use optimal values,
        // which might get thrown off, when using neighbor override
        int costToFurthestLegalTarget = 0; 
        foreach (Waypoint waypoint in route)
        {
            if (waypoint.Node.CurrentNodeState is not NodeState.InReach) continue;
            furthestLegalTarget = waypoint.Node;
            costToFurthestLegalTarget = waypoint.LowestMoveCost;
        }
        
        if(!furthestLegalTarget) return;
        
        Dictionary<Node, Waypoint> waypoints = CalculateDijkstra(furthestLegalTarget);
        foreach (Waypoint waypoint in waypoints.Values)
        {
            if(waypoint.Node.CurrentNodeState is NodeState.OutOfReach) continue;
            bool willBeOutOfReach = (waypoint.LowestMoveCost + waypoint.Node.CostToLeave) > CurrentFuel - costToFurthestLegalTarget;
            if (willBeOutOfReach)
                waypoint.Node.PreviewingOutOfReach = true;
        }
    }

    private void EndHoverPreview()
    {
        TargetedNode = null;
        LegalMove = false;
        TargetMoveCost = 0;

        foreach (Node node in Node.s_Nodes)
        {
            node.CurrentTargetingState = TargetingState.Idle;
            node.PreviewingOutOfReach = false;
        }
            
        
        foreach (Connector connector in Connector.s_Connectors)
            connector.HighlightState = HighlightState.Idle;
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
