using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RouteManager : SimpleMonoBehaviorSingleton<RouteManager>
{
    public Node TargetedNode;
    
    [SerializeField] private Node startNode;
    private Dictionary<Node, Waypoint> startMap;
    private int FuelAtStart => MapManager.CurrentFuel;
    
    private Node previewNode;
    [SerializeField]
    private Route previewRoute;
    
    private Node NodeToCheckPreviewFrom =>
        RoutePlanned ? // if list exists and has entries
            routeSegments[^1].DestinationNode : // return last stopover
            startNode; // else return start node
    
    
    private int FuelToCheckPreviewFrom =>
        RoutePlanned ? // if list exists and has entries
            routeSegments[^1].FuelAtDestination : // return last stopover
            FuelAtStart; // else return start node

    private Dictionary<Node, Waypoint> GetLatestPlannedMapStage()
    {
        return RoutePlanned ? LastPlannedRoute.DijkstraMapFromDestination : startMap; // else return map stage
    }
    
    private bool RoutePlanned => routeSegments is { Count: > 0 };
    
    private Node FurthestStopoverNode => RoutePlanned ? SegmentStopoverNodes[^1] : null;
    private List<Node> SegmentStopoverNodes => routeSegments.Select(x => x.DestinationNode).ToList();
    private List<Waypoint> SegmentStopovers => routeSegments.Select(x => x.FurthestWaypoint).ToList();
    
    [SerializeField]
    private List<Route> routeSegments = new();
    private Route LastPlannedRoute => routeSegments[^1];
    
    [SerializeField]
    public Route Route;
    
    
    //includes possible preview segment
    private List<Route> GetAllRouteSegments()
    {
        List<Route> completeRoute = routeSegments.ToList();
        if(previewRoute != null) completeRoute.Add(previewRoute);
        return completeRoute;
    }
    
    protected override void Awake()
    {
        base.Awake();
        Node.AnyNodeHoverChanged += OnNodeHoverChanged;
        MapManager.UpdatedPlayerNode += OnPlayerNodeUpdated;
        PointerHandler.PointerAltDown += OnRightClick;
    }

    private void OnDestroy()
    {
        Node.AnyNodeHoverChanged -= OnNodeHoverChanged;
        MapManager.UpdatedPlayerNode -= OnPlayerNodeUpdated;
        PointerHandler.PointerAltDown -= OnRightClick;
    }

    private void OnNodeHoverChanged(Node hoveredNode)
    {
        previewNode = hoveredNode;
        UpdatePreview();
    }
    
    private void OnRightClick(IClickableObject rightClickedObject)
    {
        switch (rightClickedObject)
        {
            case null:
                RemoveAllRouteSegments();
                break;
            
            case Node rightClickedNode:
                OnNodeRightClicked(rightClickedNode);
                break;
        }
    }
    
    private void OnNodeRightClicked(Node rightClickedNode)
    {
        // click on last stopover to remove
        if (RoutePlanned && rightClickedNode == FurthestStopoverNode)
        {
            RemoveLastPlannedRouteSegment();
            return;
        }
        
        // click on new waypoint
        if(previewRoute == null) return; // you can't have a previewRouteSegment, when hovering over the player node. no need to worry
        routeSegments.Add(previewRoute);
        UpdatePreview();
    }

    private void RemoveAllRouteSegments()
    {
        routeSegments.Clear();
        UpdatePreview();
    }
    
    private void RemoveLastPlannedRouteSegment()
    {
        routeSegments.RemoveAt(routeSegments.Count - 1);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if(previewNode && previewNode != NodeToCheckPreviewFrom)
            ConstructPreviewRouteSegment();
        else
            previewRoute = null;

        Route = CreateCompleteRoute();
        EvaluateRoute(Route);
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
            && connectorToNeighbor.MoveCost <= FuelToCheckPreviewFrom // can I cover the direct movement cost?
            && connectorToNeighbor.MoveCost + previewNode.CostToLeave <= FuelToCheckPreviewFrom; // can you still leave from there?

        List<Waypoint> waypointsToDestination = canMoveDirectlyToDestination
            ? new List<Waypoint> { new(previewNode, connectorToNeighbor.MoveCost, NodeToCheckPreviewFrom) }
            : DijkstraUtility.GetRouteBetweenTwoPointsOnMap(GetLatestPlannedMapStage(), NodeToCheckPreviewFrom, previewNode);
        
        Dictionary<Node, Waypoint> dijkstraMapFromDestination = DijkstraUtility.CalculateDijkstra(Node.s_Nodes, previewNode);
        
        // Write dijkstra route as list
        previewRoute = new Route(FuelToCheckPreviewFrom, waypointsToDestination, dijkstraMapFromDestination);
    }
    
    private Route CreateCompleteRoute()
    {
        // the list that will be filled with the complete route
        List<Waypoint> combinedRoute = new ();
        
        // collect complete route (potentially with preview)
        List<Route> segmentedRoute = GetAllRouteSegments();
        
        // when no route, don't evaluate
        if(segmentedRoute.Count < 1)
        {
            return new Route(MapManager.CurrentFuel, new List<Waypoint>(), startMap);
        }
        
        int costOffset = 0;
        // construct route segments into one big waypoint list
        
        for (int i = 0; i < segmentedRoute.Count; i++)
        {
            Route current = segmentedRoute[i];
            Route prev = i > 0 ? segmentedRoute[i-1] : null;
            if (prev != null) costOffset += prev.SegmentFuelCost;
            
            // evaluate waypoints
            foreach (Waypoint waypointFromSegmentedRoute in current.Waypoints)
            {
                combinedRoute.Add(new Waypoint(waypointFromSegmentedRoute.Node, waypointFromSegmentedRoute.LowestMoveCost + costOffset, waypointFromSegmentedRoute.PreviousNode));
            }
        }

        Dictionary<Node, Waypoint> dijkstraMapFromDestination = DijkstraUtility.CalculateDijkstra(Node.s_Nodes, combinedRoute[^1].Node);

        return new Route(MapManager.CurrentFuel, combinedRoute, dijkstraMapFromDestination);
    }

    private void EvaluateRoute(Route route)
    {
        foreach (Waypoint waypoint in route.Waypoints)
        {
            if (waypoint == route.NextWaypoint)
            {
                TargetedNode = waypoint.Node;
                TargetedNode.CurrentTargetState = TargetState.Targeted;
                TargetedNode.CurrentRoutingState = RoutingState.Marked;
                waypoint.Connection.SetConnectorRouteState(ConnectorState.NextRoute, waypoint.LowestMoveCost);
                continue;
            }

            if (waypoint == route.FurthestLegalWaypoint)
            {
                waypoint.Node.CurrentTargetState = TargetState.Targeted;
                waypoint.Connection.SetConnectorRouteState(ConnectorState.LastRoute, waypoint.LowestMoveCost);
                continue;
            }

            if (SegmentStopoverNodes.Contains(waypoint.Node))
            {
                waypoint.Node.CurrentTargetState = TargetState.Waypoint;
            }
            else
                waypoint.Node.CurrentTargetState = TargetState.NotTargeted;
            
            waypoint.Node.CurrentRoutingState = RoutingState.Marked;
            
            waypoint.Connection.SetConnectorRouteState(ConnectorState.OnRoute, waypoint.LowestMoveCost);
        }

        foreach (Waypoint waypointFromDestination in route.DijkstraMapFromDestination.Values)
        {
            waypointFromDestination.Node.PreviewingOutOfReach =
                waypointFromDestination.LowestMoveCost + waypointFromDestination.Node.CostToLeave > route.FuelAtDestination;
        }
        
        // unmark all nodes not on route
        foreach (Node node in Node.AllNodesExcept(route.RouteNodes))
        {
            node.CurrentRoutingState = RoutingState.NotOnRoute;
            node.CurrentTargetState = TargetState.NotTargeted;
        }
        
        // unmark all connectors not on route
        foreach (Connector connector in Connector.AllConnectorsExcept(route.RouteConnectors))
        {
            connector.SetConnectorRouteState(ConnectorState.Idle);
        }
    }
}

[Serializable]
public class Route
{
    [field: SerializeField, HideInInspector]
    public string RouteName { get; private set; }
    
    [field: SerializeField]
    public int StartFuel { get; private set; }
    [field: SerializeField]
    public List<Waypoint> Waypoints { get; private set; }
    public Dictionary<Node, Waypoint> DijkstraMapFromDestination { get; private set; }
    
    private bool ContainsRoute => Waypoints is {Count: > 0};
    public Node DestinationNode => ContainsRoute ? FurthestWaypoint.Node : null;
    public Node StartNode => ContainsRoute ? Waypoints[0].PreviousNode : null;
    public Waypoint NextWaypoint => ContainsRoute ? Waypoints[0] : null;
    public Waypoint FurthestWaypoint =>  ContainsRoute ? Waypoints[^1] : null;
    
    [field: SerializeField]
    public Waypoint FurthestLegalWaypoint { get; private set; } = null;
    
    private void DetermineFurthestLegalWaypoint()
    {
        if (!ContainsRoute) return;
        for (int i = Waypoints.Count - 1; i >= 0; i--)
        {
            if (Waypoints[i].LowestMoveCost + Waypoints[i].Node.CostToLeave > StartFuel) continue; // skip until one is legal
            FurthestLegalWaypoint = Waypoints[i];
            break;
        }
    }

    public List<Node> RouteNodes => Waypoints.Select(wp => wp.Node).ToList();
    public List<Connector> RouteConnectors => Waypoints.Select(c => c.Connection).ToList();
    
    public int FuelAtDestination => ContainsRoute ? StartFuel - SegmentFuelCost : StartFuel;
    public int SegmentFuelCost => ContainsRoute ? FurthestWaypoint.LowestMoveCost : 0;

    public Route(int startFuel, List<Waypoint> waypoints, Dictionary<Node, Waypoint> dijkstraMapFromDestination)
    {
        StartFuel = startFuel;
        Waypoints = waypoints;
        DijkstraMapFromDestination = dijkstraMapFromDestination;
        DetermineFurthestLegalWaypoint();

        RouteName = ContainsRoute ? $"Route from |{StartNode.name}| to |{DestinationNode.name}| for {SegmentFuelCost}" : "Incomplete Route";
    }
}