using System;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(TMP_Text))]
public class FuelCounter : MonoBehaviour
{
    private TMP_Text counter;

    private void Awake()
    {
        counter = GetComponent<TMP_Text>();
        MapManager.OnFuelChanged += ChangeCounter;
    }

    private void OnDestroy()
    {
        MapManager.OnFuelChanged -= ChangeCounter;
    }

    private void ChangeCounter(int newFuelValue)
    {
        counter.text = newFuelValue.ToString();
    }
}
