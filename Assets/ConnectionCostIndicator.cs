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
    [SerializeField, BoxGroup("State - Marked")] private float markedAlpha = 0.5f;
    [SerializeField, BoxGroup("State - Targeted")] private float targetedAlpha = 1;
    
    [SerializeField, BoxGroup("State - Idle")] private float idleScaleFactor = 0.5f;
    private Vector3 IdleScale => new Vector3(idleScaleFactor, idleScaleFactor, idleScaleFactor);

    [SerializeField, BoxGroup("State - Marked")] private float markedScaleFactor = 0.8f;
    private Vector3 MarkedScale => new Vector3(markedScaleFactor, markedScaleFactor, markedScaleFactor);
    
    [SerializeField, BoxGroup("State - Targeted")] private float targetedScaleFactor = 1.2f;
    private Vector3 TargetedScale => new Vector3(targetedScaleFactor, targetedScaleFactor, targetedScaleFactor);
    
    [SerializeField, BoxGroup("State - LastRoute")] private float endScaleFactor = 1f;
    private Vector3 EndScale => new Vector3(endScaleFactor, endScaleFactor, endScaleFactor);

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

        UpdateIndicator(ConnectorState.Idle) ; // initial check
    }
    
    public void UpdateIndicator(ConnectorState newState, int routeCost = 0)
    {
        if(previewCostObject)
            previewCostObject.SetActive(newState is ConnectorState.Idle);
        totalCostObject.SetActive(newState is not ConnectorState.Idle);

        if (newState is not ConnectorState.Idle)
        {
            totalCostTxt.text = routeCost.ToString();
        }
        
        indicatorGroup.alpha = newState switch
        {
            ConnectorState.OnRoute => markedAlpha,
            ConnectorState.NextRoute => targetedAlpha,
            ConnectorState.LastRoute => targetedAlpha,
            _ => idleAlpha,
        };
        
        transform.localScale = newState switch
        {
            ConnectorState.OnRoute => MarkedScale,
            ConnectorState.NextRoute => TargetedScale,
            ConnectorState.LastRoute => EndScale,
            _ => IdleScale,
        };
    }
    
    
}
