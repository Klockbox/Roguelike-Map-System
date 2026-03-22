using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class StopoverPin : MonoBehaviour
{
    private static int pinIndex = 0;
    
    [SerializeField] private Vector2 stopoverPinEulerRangeX = new (0, 20);
    [SerializeField] private Vector2 stopoverPinEulerRangeZ = new (0, -20);
    [SerializeField] private TMP_Text pinNumberText;

    private void Awake()
    {
        pinIndex++;
        if(pinNumberText)
            pinNumberText.text = pinIndex.ToString();
        
        transform.localRotation = Quaternion.Euler(Random.Range(stopoverPinEulerRangeX.x, stopoverPinEulerRangeX.y), 0, Random.Range(stopoverPinEulerRangeZ.x, stopoverPinEulerRangeZ.y));

    }

    private void OnDestroy()
    {
        pinIndex--;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
