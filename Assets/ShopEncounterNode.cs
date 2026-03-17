using UnityEngine;

public class ShopEncounterNode : MonoBehaviour, IEncounterNode
{
    [SerializeField] private ShopEncounter shopEncounter;
    public MapEncounter GetEncounter()
    {
        return shopEncounter;
    }
}
