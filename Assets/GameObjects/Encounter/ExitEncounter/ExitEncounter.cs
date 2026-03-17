using System;
using UnityEngine;

public class ExitEncounter : MapEncounter
{
    private void Start()
    {
        MapManager.Instance.PointScore = MapManager.Instance.PointScore; // just to update the counter. lazy method
    }

    public void RestartMapDemo()
    {
        EndEncounter(MapManager.Instance.ReloadMapScene);
    }
    
    public void CloseMapDemo()
    {
        Application.Quit();
    }
}
