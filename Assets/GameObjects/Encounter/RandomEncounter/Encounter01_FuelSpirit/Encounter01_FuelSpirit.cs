using UnityEngine;

public class Encounter01_Spirit : MapEncounter
{
    public void Decline()
    {
        EndEncounter();
    }
    
    public void Accept()
    {
        MapManager.CurrentFuel += 5;
        MapManager.Instance.PointScore -= 10;
        EndEncounter();
    }
}
