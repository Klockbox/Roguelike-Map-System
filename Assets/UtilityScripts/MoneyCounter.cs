using UnityEngine;

public class MoneyCounter : Counter
{
    private protected override void Subscribe()
    {
        MapManager.MoneyChanged += ChangeCounter;
    }

    private protected override void Unsubscribe()
    {
        MapManager.MoneyChanged -= ChangeCounter;
    }
}
