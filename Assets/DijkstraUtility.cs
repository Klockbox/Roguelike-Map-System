using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DijkstraUtility
{
    public static Dictionary<Node, Waypoint> CalculateDijkstra(List<Node> mapNodes, Node startingNode)
    {
        // set up dict table
        Dictionary<Node, Waypoint> waypoints = mapNodes.ToDictionary(node => node, node => new Waypoint(node, int.MaxValue, null));

        // set up starting node
        waypoints[startingNode].OverrideMoveCost(0); // no dist because we are already there
        
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
                waypoint.SetBetterPreviousNode(closestWaypoint.Node, totalCostToConnectedNode);
            }
            unvisitedWaypoints.Remove(closestWaypoint);
        }
        
        return waypoints;
    }
    
    public static List<Waypoint> GetRouteBetweenTwoPointsOnMap(Dictionary<Node, Waypoint> map, Node routeStart, Node routeEnd)
    {
        List<Waypoint> waypointChain = new();
        
        // construct route
        Waypoint evaluatedWaypoint = map[routeEnd];

        while (evaluatedWaypoint.Node != routeStart)
        {
            waypointChain.Add(evaluatedWaypoint);
            evaluatedWaypoint = map[evaluatedWaypoint.PreviousNode];
        }
        
        waypointChain.Reverse(); // sort it that neighbor target is [0]
        return waypointChain;
    }
}
