using UnityEngine;

public class Encounter01_Spirit : MapEncounter
{
    public void Decline()
    {
        EndEncounter();
    }
    
    public void Accept()
    {
        MapManager.Instance.CurrentFuel += 12;
        EndEncounter();
    }
}
