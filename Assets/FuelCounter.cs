using System;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(TMP_Text))]
public class FuelCounter : MonoBehaviour
{
    private TMP_Text counter;
    private int currentValue;
    private void Awake()
    {
        counter = GetComponent<TMP_Text>();
        MapManager.OnFuelChanged += ChangeCounter;
        MapManager.MovePreviewChanged += OnMovePreviewChanged;
    }
    
    private void OnDestroy()
    {
        MapManager.OnFuelChanged -= ChangeCounter;
        MapManager.MovePreviewChanged -= OnMovePreviewChanged;
    }

    private void ChangeCounter(int newFuelValue)
    {
        currentValue = newFuelValue;
        counter.text = currentValue.ToString();
    }
    
    private void OnMovePreviewChanged(int previewMoveCosts)
    {
        counter.color = previewMoveCosts == 0 ? new Color(1, 1f, 1f) : new Color(1, 0.5f, 0.5f);
        counter.text = (currentValue - previewMoveCosts).ToString();
    }
}
