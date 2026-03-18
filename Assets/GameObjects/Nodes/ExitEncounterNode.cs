using UnityEngine;

public class ExitEncounterNode : MonoBehaviour, IEncounterNode
{
    [SerializeField] private ExitEncounter exitEncounter;
    public MapEncounter GetEncounter()
    {
        return exitEncounter;
    }

    public bool IsExit() => true;
}
