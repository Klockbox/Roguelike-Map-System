using System.Collections.Generic;
using UnityEngine;

public class CombatEncounterNode : MonoBehaviour, IEncounterNode
{
    [SerializeField]
    private List<CombatEncounter> combatEncounters = new();
    
    public MapEncounter GetEncounter()
    {
        int index = Random.Range(0, combatEncounters.Count);
        return combatEncounters[index];
    }

    public bool IsExit() => false;
    public bool IsRepeatable() => false;
}

public interface IEncounterNode
{
    MapEncounter GetEncounter();
    bool IsExit();
    bool IsRepeatable();
}
