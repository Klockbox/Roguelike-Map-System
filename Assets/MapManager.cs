using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : SimpleMonoBehaviorSingleton<MapManager>
{
    [SerializeField] private PlayerMini playerMini;

    [SerializeField] private bool debug_SkipEncounter;
    
    [SerializeField, ReadOnly, BoxGroup("Move"), AllowNesting]
    private MapMove nextMove = new ();
    
    [SerializeField, BoxGroup("Move")]
    private Node _playerNode;
    public Node PlayerNode
    {
        get => _playerNode;
        set
        {
            _playerNode = value;
            UpdatedPlayerNode?.Invoke(_playerNode);
        }
    }
    public static event Action<Node> UpdatedPlayerNode;

    private static MapState s_mapState => Instance ? Instance.CurrentMapState : MapState.Travel;
    
    [SerializeField, ReadOnly, BoxGroup("Game State")] 
    private MapState _currentMapState = MapState.Travel;
    public MapState CurrentMapState
    {
        get => _currentMapState;
        set
        {
            _currentMapState = value;
            MapManager.UpdateMapInteractability();
        }
    }

    #region Map Interactability

    public static bool MapIsInteractable { get; private set; } = true;
    public static void UpdateMapInteractability()
    {
        bool prevValue = MapIsInteractable;
        MapIsInteractable = s_mapState is MapState.Travel && !PlayerMini.IsMoving;
        if(MapIsInteractable != prevValue)
            MapInteractabilityChanged?.Invoke(MapIsInteractable);
    }
    public static event Action<bool> MapInteractabilityChanged;

    #endregion
    
    

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
    
    public static event Action<int> OnFuelChanged;

    
    
    [SerializeField, BoxGroup("Fuel")] private int startingFuel;
    [SerializeField, ReadOnly, BoxGroup("Fuel")] private int currentFuel;
    public static int CurrentFuel
    {
        get => Instance ? Instance.currentFuel : int.MaxValue;
        set
        {
            if(!Instance) return;
            Instance.currentFuel = value;
            OnFuelChanged?.Invoke(value);
        }
    }
    
    private void OnEnable()
    {
        Node.NodeClicked += OnNodeClicked;

        UpdatedPlayerNode += Node.UpdateCostToReach;
        MapInteractabilityChanged += Node.UpdateNodeInteractability;
        MapInteractabilityChanged += Connector.SetInteractable;
        
        RouteManager.DeterminedNewMove += OnMoveUpdated;
        
        PlayerMini.PlayerMiniReachedCurrentNode += OnPlayerMiniReachedCurrentNode;
    }

    private void OnMoveUpdated(MapMove newMove)
    {
        nextMove = newMove;
    }

    private void OnDisable()
    {
        Node.NodeClicked -= OnNodeClicked;
        MapInteractabilityChanged = null;
        PlayerMini.PlayerMiniReachedCurrentNode -= OnPlayerMiniReachedCurrentNode;
    }

    [Button]
    public void ReloadMapScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    

    private void Start()
    {
        CurrentFuel = startingFuel;
        Node.UpdateCostToLeave();
        
        Money = 0;
        PointScore = 0;
        
        
        PlayerNode.Visited = true;
        TeleportPlayer(PlayerNode);
    }


    private void OnPlayerMiniReachedCurrentNode()
    {
        if (!PlayerNode.Visited && !debug_SkipEncounter)
            EncounterManager.Instance.StartEncounter(PlayerNode.GetEncounter());
        
        PlayerNode.Visited = true;
        
        MarkNeighbors(true);
    }
    
    private void OnNodeClicked(Node clickedNode)
    {
        MovePlayer();
    }

    public void MovePlayer()
    {
        if (!nextMove.IsValid || !MapIsInteractable) return;
        
        MarkNeighbors(false);

        CurrentFuel -= nextMove.MoveCost;
        PlayerNode = nextMove.TargetedNode;
        
        PlayerMini.Instance.MoveToNode(PlayerNode);
    }
    
    private void TeleportPlayer(Node toNode)
    {
        MarkNeighbors(false);
        
        PlayerNode = toNode;
        PlayerMini.Instance.SetMiniToNode(PlayerNode);
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

[Serializable]
public class MapMove
{
    public bool IsValid => TargetedNode && StartNode && IsLegal;
    public bool IsLegal;
    public Node TargetedNode;
    public Node StartNode;
    public int MoveCost;
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
