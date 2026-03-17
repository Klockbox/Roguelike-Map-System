using System.Collections.Generic;
using UnityEngine;

public class RandomEncounterNode : MonoBehaviour, IEncounterNode
{
    [SerializeField]
    private List<MapEncounter> randomEncounters = new();

    [SerializeField]
    private List<CombatEncounter> combatEncounters = new();
    
    public MapEncounter GetEncounter()
    {
        int rnd = Random.Range(1, 6);
        if (rnd == 5)
            return combatEncounters[Random.Range(0, combatEncounters.Count)];
        return randomEncounters[Random.Range(0, randomEncounters.Count)];
    }
}
