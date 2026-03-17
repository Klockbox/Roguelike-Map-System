using UnityEngine;

public class PointCounter : Counter
{
    private protected override void Subscribe()
    {
        MapManager.PointScoreChanged += ChangeCounter;
    }

    private protected override void Unsubscribe()
    {
        MapManager.PointScoreChanged -= ChangeCounter;
    }
}
