using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class ConnectionCostIndicator : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private HighlightState _currentState;
    public HighlightState HighlightState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            UpdateIndicator();
        }
    }

    [SerializeField, BoxGroup("References")] private CanvasGroup indicatorGroup;
    [SerializeField, BoxGroup("References")] private GameObject previewCost;
    [SerializeField, BoxGroup("References")] private TMP_Text previewCostTxt;
    [SerializeField, BoxGroup("References")] private GameObject actualCost;
    [SerializeField, BoxGroup("References")] private TMP_Text actualCostTxt;
    
    [SerializeField, BoxGroup("State - Idle")] private float idleAlpha = 0.5f;
    [SerializeField, BoxGroup("State - Marked")] private float markedAlpha = 1;
    [SerializeField, BoxGroup("State - Targeted")] private float targetedAlpha = 1;
    
    [SerializeField, BoxGroup("State - Idle")] private float idleScaleFactor = 0.5f;
    private Vector3 IdleScale => new Vector3(idleScaleFactor, idleScaleFactor, idleScaleFactor);
    
    [SerializeField, BoxGroup("State - Marked")] private float markedScaleFactor = 1;
    private Vector3 MarkedScale => new Vector3(markedScaleFactor, markedScaleFactor, markedScaleFactor);
    
    [SerializeField, BoxGroup("State - Targeted")] private float targetedScaleFactor = 1.2f;
    private Vector3 TargetedScale => new Vector3(targetedScaleFactor, targetedScaleFactor, targetedScaleFactor);
    

    private void Awake()
    {
        if(previewCost)
            previewCost.SetActive(false);
        actualCost.SetActive(false);
    }

    public void SetCost(int cost)
    {
        actualCostTxt.text = cost.ToString();
        if(previewCostTxt)
            previewCostTxt.text = (cost-1).ToString();

        HighlightState = HighlightState.Idle; // initial check
    }
    
    private void UpdateIndicator()
    {
        if(previewCost)
            previewCost.SetActive(HighlightState is HighlightState.Idle);
        actualCost.SetActive(HighlightState is not HighlightState.Idle);

        indicatorGroup.alpha = HighlightState switch
        {
            HighlightState.Marked => markedAlpha,
            HighlightState.Targeted => targetedAlpha,
            _ => idleAlpha,
        };
        
        transform.localScale = HighlightState switch
        {
            HighlightState.Marked => MarkedScale,
            HighlightState.Targeted => TargetedScale,
            _ => IdleScale,
        };
    }
    
    
}
