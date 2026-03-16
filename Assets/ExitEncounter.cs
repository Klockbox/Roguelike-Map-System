using UnityEngine;

public class ExitEncounter : MapEncounter
{
    public void RestartMapDemo()
    {
        EndEncounter(MapManager.Instance.ReloadMapScene);
    }
    
    public void CloseMapDemo()
    {
        Application.Quit();
    }
}
