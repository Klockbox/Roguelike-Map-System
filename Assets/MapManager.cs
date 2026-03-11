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

    
    public static event Action<int> OnFuelChanged;
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
    
    private bool AcceptInput()
    {
        return !PlayerMini.IsMoving;
    }
    
    private void OnEnable()
    {
        Node.NodeClicked += OnNodeClicked;
        PlayerMini.PlayerStartedMove += OnPlayerMoving;
        PlayerMini.PlayerReachedNode += OnPlayerReachedNode;
    }
    private void OnDisable()
    {
        Node.NodeClicked -= OnNodeClicked;
        PlayerMini.PlayerStartedMove -= OnPlayerMoving;
        PlayerMini.PlayerReachedNode -= OnPlayerReachedNode;
    }

    protected override void Awake()
    {
        base.Awake();
        CurrentFuel = startingFuel;
    }

    private void Start()
    {
        CalculateCostsToLeave();
        playerMini.SetToNode(PlayerNode);
    }

    private void OnPlayerMoving()
    {
        
    }

    private void OnPlayerReachedNode(Node node)
    {
        PlayerNode = node;
        CalculateCostsToReach();
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
    
    private void OnNodeClicked(Node clickedNode)
    {
        if(clickedNode == PlayerNode || !AcceptInput()) return;

        Node targetNode = clickedNode;
        
        bool canMoveDirectly = Connector.TryGetConnector(PlayerNode, clickedNode, out Connector directConnector) &&
                               directConnector.MoveCost <= CurrentFuel;
        
        // if not neighboring get next node on optimal route
        if (!canMoveDirectly)
        {
            Waypoint waypoint = waypointsToReach[clickedNode];
            for (int i = 0; i < waypointsToReach.Count; i++)
            {
                if(waypoint.PreviousNode == PlayerNode) break;
                waypoint = waypointsToReach[waypoint.PreviousNode];
            }
            
            targetNode = waypoint.Node;
        }
        
        // try grab connector from player node to target node
        if(!Connector.TryGetConnector(PlayerNode, targetNode, out Connector connector))
        {
            Debug.Log($"No connector connects {PlayerNode.name} to {targetNode.name}.");
            return;
        }
        
        if(targetNode.CurrentState is NodeState.OutOfReach)
        {
            Debug.Log($"Reaching {targetNode.name} costs {targetNode.CostToReach}." +
                      $" To reach the exit then it would cost {targetNode.CostToLeave}, for a total cost of {targetNode.TotalCost}." +
                      $" This exceeds the fuel reserves of {CurrentFuel}.");
            return;
        }
        
        // here the move is legal
        CurrentFuel -= connector.MoveCost;
        PlayerMini.Instance.MoveToNode(targetNode);
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
