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
}

public interface IEncounterNode
{
    MapEncounter GetEncounter();
}
