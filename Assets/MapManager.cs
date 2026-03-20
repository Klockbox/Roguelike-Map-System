using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : SimpleMonoBehaviorSingleton<MapManager>
{
    [SerializeField] private PlayerMini playerMini;

    [SerializeField, ReadOnly, BoxGroup("Game State")] 
    private MapState _currentMapState = MapState.Travel;
    public MapState CurrentMapState
    {
        get => _currentMapState;
        set
        {
            _currentMapState = value;
            UpdateHoverPreview();
        }
    }

    [SerializeField, ReadOnly, BoxGroup("Game State")]
    private int _pointScore;
    public int PointScore
    {
        get => _pointScore;
        set
        {
            _pointScore = value;
            PointScoreChanged?.Invoke(_pointScore);
        } 
    }
    public static event Action<int> PointScoreChanged;
    
    [SerializeField, ReadOnly, BoxGroup("Game State")]
    private int _money;
    public int Money
    {
        get => _money;
        set
        {
            _money = value;
            MoneyChanged?.Invoke(_money);
        } 
    }
    public static event Action<int> MoneyChanged;

    [SerializeField, BoxGroup("Move")]
    private Node _playerNode;
    public Node PlayerNode
    {
        get => _playerNode;
        set
        {
            _playerNode = value;
            UpdatedPlayerNode?.Invoke(_playerNode);
            CalculateCostsToReach();
        }
    }
    public static event Action<Node> UpdatedPlayerNode;
    
    [field: SerializeField, ReadOnly, BoxGroup("Move")] public Node HoveredNode { get; private set; }
    [field: SerializeField, ReadOnly, BoxGroup("Move")] public Node TargetedNode { get; private set; }
    [field: SerializeField, ReadOnly, BoxGroup("Move")] public bool LegalMove { get; private set; }
    [field: SerializeField, ReadOnly, BoxGroup("Move")] public int TargetMoveCost { get; private set; }
    public static event Action<int> OnFuelChanged;
    public static event Action<int> MovePreviewChanged;

    
    
    [SerializeField, BoxGroup("Fuel")] private int startingFuel;
    [SerializeField, ReadOnly, BoxGroup("Fuel")] private int currentFuel;
    public static int CurrentFuel
    {
        get => Instance ? Instance.currentFuel : int.MaxValue;
        set
        {
            if(!Instance) return;
            Instance.currentFuel = value;
            Instance.CalculateCostsToReach();
            OnFuelChanged?.Invoke(value);
        }
    }

    private Dictionary<Node, Waypoint> dijkstraRoutesFromPlayer = new();
    private Dictionary<Node, Waypoint> waypointsToLeave = new();
    
    private bool CurrentlyAcceptingInput()
    {
        return !PlayerMini.IsMoving;
    }
    
    private bool CurrentlyAllowingPreview()
    {
        return !PlayerMini.IsMoving && CurrentMapState is MapState.Travel;
    }
    
    private void OnEnable()
    {
        Node.NodeClicked += OnNodeClicked;
        Node.AnyNodeHoverChanged += OnHoveringChanged;
        
        PlayerMini.PlayerStartedMove += OnPlayerStartMoving;
        PlayerMini.PlayerReachedNode += OnPlayerReachedNode;
    }
    
    private void OnDisable()
    {
        Node.NodeClicked -= OnNodeClicked;
        Node.AnyNodeHoverChanged -= OnHoveringChanged;
        
        PlayerMini.PlayerStartedMove -= OnPlayerStartMoving;
        PlayerMini.PlayerReachedNode -= OnPlayerReachedNode;
    }

    [Button]
    public void ReloadMapScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    

    private void Start()
    {
        CurrentFuel = startingFuel;
        Money = 0;
        PointScore = 0;
        PlayerNode = PlayerNode;
        PlayerNode.Visited = true;
        playerMini.SetToNode(PlayerNode);
        CalculateCostsToLeave();
    }

    private void OnPlayerStartMoving()
    {
        UpdateHoverPreview();
    }

    private void OnPlayerReachedNode(Node node)
    {
        if (node.Visited)
        {
            // do nothing ?
        }
        else
        {
            node.Visited = true;
            EncounterManager.Instance.StartEncounter(node.GetEncounter());
        }
        MarkNeighbors(true);
        UpdateHoverPreview();
    }

    
    private static Dictionary<Node, Waypoint> CalculateDijkstra(Node startingNode)
    {
        // set up dict table
        Dictionary<Node, Waypoint> waypoints = Node.s_Nodes.ToDictionary(node => node, node => new Waypoint(node, int.MaxValue, null));

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
    
    private void CalculateCostsToReach()
    {
        dijkstraRoutesFromPlayer = CalculateDijkstra(PlayerNode);
        
        // write values to node objects
        foreach (Waypoint waypoint in dijkstraRoutesFromPlayer.Values)
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
        return;
    }

    private void OnNodeClicked(Node clickedNode)
    {
        if (!LegalMove
            || !CurrentlyAcceptingInput()
            || !Connector.TryGetConnector(PlayerNode, TargetedNode, out Connector _))
        {
            return;
        } 
        
        MarkNeighbors(false);
        
        PlayerNode = RouteManager.Instance.TargetedNode;
        CurrentFuel -= TargetMoveCost;
        PlayerMini.Instance.MoveToNode(TargetedNode);
    }

    private void MarkNeighbors(bool isNeighbor)
    {
        List<Node> neighbors = Connector.GetAllConnectedNodesOf(PlayerNode);
        foreach (Node neighbor in neighbors)
        {
            neighbor.IsNeighbor = isNeighbor;
        }
    }
}



public enum ECostType
{
    CostToReach,
    CostToLeave
}

public enum MapState
{
    Travel,
    Encounter
}
