using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RouteManager : MonoBehaviour
{
    [SerializeField] private Node startNode;
    private Dictionary<Node, Waypoint> startMap;
    private int FuelAtStart => MapManager.CurrentFuel;
    
    private Node previewNode;
    [SerializeField]
    private RouteSegment previewRouteSegment;
    
    public Node NodeToCheckPreviewFrom =>
        route is { Count: > 0 } ? // if list exists and has entries
            route[^1].DestinationNode : // return last stopover
            startNode; // else return start node
    
    public Dictionary<Node, Waypoint> MapStageToCheckPreviewFrom =>
        route is { Count: > 0 } ? // if list exists and has entries
            route[^1].DijkstraMapFromDestination : // return last map stage
            startMap; // else return map stage
    
    public int FuelToCheckPreviewFrom =>
        route is { Count: > 0 } ? // if list exists and has entries
            route[^1].FuelAtDestination : // return last stopover
            FuelAtStart; // else return start node

    [SerializeField]
    private List<RouteSegment> route = new();
    
    //includes possible preview segment
    private List<RouteSegment> GetCompleteRoute()
    {
        List<RouteSegment> completeRoute = route.ToList();
        if(previewRouteSegment != null) completeRoute.Add(previewRouteSegment);
        return completeRoute;
    }

    private void Awake()
    {
        Node.AnyNodeHoverChanged += OnNodeHoverChanged;
        Node.NodeRightClicked += OnNodeRightClicked;
        MapManager.UpdatedPlayerNode += OnPlayerNodeUpdated;
    }
    
    private void OnDestroy()
    {
        Node.AnyNodeHoverChanged -= OnNodeHoverChanged;
        Node.NodeRightClicked -= OnNodeRightClicked;
        MapManager.UpdatedPlayerNode -= OnPlayerNodeUpdated;
    }

    private void OnNodeHoverChanged(Node hoveredNode)
    {
        previewNode = hoveredNode;
        UpdatePreview();
    }
    
    private void OnNodeRightClicked(Node obj)
    {
        if(previewRouteSegment == null) return;
        route.Add(previewRouteSegment);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if(previewNode && previewNode != NodeToCheckPreviewFrom)
            ConstructPreviewRouteSegment();
        else
            previewRouteSegment = null;

        EvaluateRoute();
    }

    private void OnPlayerNodeUpdated(Node newPlayerNode)
    {
        startNode = newPlayerNode;
        startMap = DijkstraUtility.CalculateDijkstra(Node.s_Nodes, startNode);
    }
    
    

    private void ConstructPreviewRouteSegment()
    {
        bool previewedNodeIsNeighbor = Connector.TryGetConnector(NodeToCheckPreviewFrom, previewNode, out Connector connectorToNeighbor);
        
        // check if we can ignore routing
        bool canMoveDirectlyToDestination = 
            previewedNodeIsNeighbor // is it a neighbor?
            && connectorToNeighbor.MoveCost <= MapManager.CurrentFuel // can I cover the direct movement cost?
            && connectorToNeighbor.MoveCost + previewNode.CostToLeave <= FuelToCheckPreviewFrom; // can you still leave from there?

        // Write dijkstra route as list
        previewRouteSegment = new RouteSegment
        {
            DestinationNode = previewNode,
            Waypoints = canMoveDirectlyToDestination ? 
                new List<Waypoint> { new (previewNode, connectorToNeighbor.MoveCost, NodeToCheckPreviewFrom) } : 
                DijkstraUtility.GetRouteBetweenTwoPointsOnMap(MapStageToCheckPreviewFrom, NodeToCheckPreviewFrom, previewNode)
        };

        previewRouteSegment.FuelAtDestination = FuelToCheckPreviewFrom - previewRouteSegment.Waypoints[^1].LowestMoveCost;
        previewRouteSegment.DijkstraMapFromDestination = DijkstraUtility.CalculateDijkstra(Node.s_Nodes, previewNode);
    }


    private void EvaluateRoute()
    {
        foreach (RouteSegment segment in GetCompleteRoute())
        {
            foreach (Waypoint waypoint in segment.Waypoints)
            {
                waypoint.Node.CurrentRoutingState = RoutingState.Marked;
            }
        }
    }
}

[Serializable]
public class RouteSegment
{
    public Node DestinationNode;
    public int FuelAtDestination;
    public Dictionary<Node, Waypoint> DijkstraMapFromDestination = new();
    public List<Waypoint> Waypoints = new();
}