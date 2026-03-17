using System;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(TMP_Text))]
public class FuelCounter : Counter
{
    private protected override void Subscribe()
    {
        MapManager.OnFuelChanged += ChangeCounter;
        MapManager.MovePreviewChanged += OnMovePreviewChanged;
    }

    private protected override void Unsubscribe()
    {
        MapManager.OnFuelChanged -= ChangeCounter;
        MapManager.MovePreviewChanged -= OnMovePreviewChanged;
    }
    
    private void OnMovePreviewChanged(int previewMoveCosts)
    {
        TextElement.color = previewMoveCosts == 0 ? new Color(1, 1f, 1f) : new Color(1, 0.5f, 0.5f);
        TextElement.text = (CurrentValue - previewMoveCosts).ToString();
    }
}