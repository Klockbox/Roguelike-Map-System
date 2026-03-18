using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class ConnectionCostIndicator : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] private CanvasGroup indicatorGroup;
    [SerializeField, BoxGroup("References")] private GameObject previewCostObject;
    [SerializeField, BoxGroup("References")] private TMP_Text previewCostTxt;
    [SerializeField, BoxGroup("References")] private GameObject totalCostObject;
    [SerializeField, BoxGroup("References")] private TMP_Text totalCostTxt;
    
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
        if(previewCostObject)
            previewCostObject.SetActive(false);
        totalCostObject.SetActive(false);
    }

    public void SetBaseCost(int cost)
    {
        totalCostTxt.text = cost.ToString();
        if(previewCostTxt)
            previewCostTxt.text = (cost-1).ToString();

        UpdateIndicator(HighlightState.Idle) ; // initial check
    }
    
    public void UpdateIndicator(HighlightState newState, int routeCost = 0)
    {
        if(previewCostObject)
            previewCostObject.SetActive(newState is HighlightState.Idle);
        totalCostObject.SetActive(newState is not HighlightState.Idle);

        if (newState is not HighlightState.Idle)
        {
            totalCostTxt.text = routeCost.ToString();
        }
        
        indicatorGroup.alpha = newState switch
        {
            HighlightState.Marked => markedAlpha,
            HighlightState.Targeted => targetedAlpha,
            _ => idleAlpha,
        };
        
        transform.localScale = newState switch
        {
            HighlightState.Marked => MarkedScale,
            HighlightState.Targeted => TargetedScale,
            _ => IdleScale,
        };
    }
    
    
}
