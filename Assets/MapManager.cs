using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapManager : SimpleMonoBehaviorSingleton<MapManager>
{
    private void OnEnable()
    {
        Node.NodeClicked += OnNodeClicked;
        PlayerMini.PlayerStartedMove += OnPlayerMoving;
        PlayerMini.PlayerEndedMove += OnPlayerStopped;
    }
    private void OnDisable()
    {
        Node.NodeClicked -= OnNodeClicked;
        PlayerMini.PlayerStartedMove -= OnPlayerMoving;
        PlayerMini.PlayerEndedMove -= OnPlayerStopped;
    }

    
    private void OnPlayerMoving()
    {
        
    }

    private void OnPlayerStopped()
    {
        CalculateCosts();
    }
    
    [Button]
    private void CalculateCosts()
    {
        CalculateDijkstra(PlayerMini.Instance.currentNode, ECostType.CostToReach);
        CalculateDijkstra(Node.s_ExitNodes[0], ECostType.CostToLeave);
    }
    
    private void CalculateDijkstra(Node startingNode, ECostType costType)
    {
        // set up dict table
        Dictionary<Node, Waypoint> waypoints = Node.s_Nodes.ToDictionary(node => node, node => new Waypoint(node, int.MaxValue, null));

        // set up starting node
        waypoints[startingNode].LowestMoveCost = 0; // no dist because we are already there
        switch (costType)
        {
            case ECostType.CostToReach:
                startingNode.SetCostToReach(0);
                break;
            case ECostType.CostToLeave:
                startingNode.SetCostToLeave(0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(costType), costType, null);
        }
        
        
        while (waypoints.Values.Any(x => !x.Visited))
        {
            List<Waypoint> unvisitedWaypoints = waypoints.Values.Where(waypoint => !waypoint.Visited).ToList();
            
            // grab closest waypoint
            Waypoint closestWaypoint = unvisitedWaypoints[0];
            foreach (Waypoint waypointToCheck in unvisitedWaypoints)
                if (waypointToCheck.LowestMoveCost < closestWaypoint.LowestMoveCost) closestWaypoint = waypointToCheck;
            
            int baseMovementCost = closestWaypoint.LowestMoveCost;
            
            // iterate through connections
            List<Connector> connections = Connector.GetAllConnectionsFrom(closestWaypoint.Node);
            foreach (Connector connection in connections)
            {
                Node connectedNode = connection.GetOther(closestWaypoint.Node);
                Waypoint waypoint = waypoints[connectedNode];
                int totalCostToConnectedNode = baseMovementCost + connection.MoveCost;
                
                if (totalCostToConnectedNode >= waypoint.LowestMoveCost) continue;
                
                waypoint.LowestMoveCost = totalCostToConnectedNode;
                waypoint.PreviousPrevNode = closestWaypoint.Node;

                switch (costType)
                {
                    case ECostType.CostToReach:
                        waypoint.Node.SetCostToReach(totalCostToConnectedNode);
                        break;
                    case ECostType.CostToLeave:
                        waypoint.Node.SetCostToLeave(totalCostToConnectedNode);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(costType), costType, null);
                }
                
            }
            closestWaypoint.Visited = true;
        }

        foreach (Waypoint waypoint in waypoints.Values)
        {
            Debug.Log($"Closest dist from {startingNode.name} to {waypoint.Node.name} is {waypoint.LowestMoveCost}");
        }
        
    }
    
    private void OnNodeClicked(Node clickedNode)
    {
        // check move legality
        List<Connector> neighborConnections = Connector.GetAllConnectionsFrom(PlayerMini.Instance.currentNode);
        Connector connection = neighborConnections.FirstOrDefault(connector => connector.Connects(clickedNode));
        
        if (!connection)
        {
            Debug.Log($"No valid connection found between {PlayerMini.Instance.currentNode.name} and {clickedNode.name}.");
            return;
        }
        
        PlayerMini.Instance.MoveToNode(clickedNode);
    }

    public class Waypoint
    {
        public Node Node { get; private set; }
        public int LowestMoveCost;
        public Node PreviousPrevNode;
        public bool Visited;

        public Waypoint(Node node, int dist, Node prevNode)
        {
            Node = node;
            LowestMoveCost = dist;
            PreviousPrevNode = prevNode;
            Visited = false;
        }
    }
}

public enum ECostType
{
    CostToReach,
    CostToLeave
}
