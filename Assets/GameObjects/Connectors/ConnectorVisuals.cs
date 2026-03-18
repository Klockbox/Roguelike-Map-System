using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Connector))]
public class ConnectorVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private MeshRenderer connectionMeshRenderer;

    [SerializeField] private Canvas indicatorCanvas;
    [SerializeField] private ConnectionCostIndicator oneCostPrefab;
    [SerializeField] private ConnectionCostIndicator extraCostPrefab;

    [SerializeField] private Color oneCostRoadColor;
    [SerializeField] private Color twoCostRoadColor;
    [SerializeField] private Color threeCostRoadColor;
    
    private ConnectionCostIndicator connectedIndicator;
    
    [Header("Options")]
    private Color baseColor;
    public Color HighlightColor;

    private void Awake()
    {
        baseColor = connectionMeshRenderer.material.color;
        GetComponent<Connector>().HighlightStateChanged += SetHighlight;  // connector nulls listeners on destroy
    }

    public void SetUp(int cost)
    {
        connectedIndicator = cost switch
        {
            1 => Instantiate(oneCostPrefab, indicatorCanvas.transform),
            _ => Instantiate(extraCostPrefab, indicatorCanvas.transform)
        };
        connectedIndicator.SetCost(cost);

        baseColor = cost switch
        {
            2 => twoCostRoadColor,
            3 => threeCostRoadColor,
            _ => oneCostRoadColor
        };
    }

    private void SetHighlight(HighlightState state)
    {
        // notify indicator
        connectedIndicator.HighlightState = state;
        
        // change road color
        switch (state)
        {
            default:
            case HighlightState.Idle:
                connectionMeshRenderer.material.color = baseColor;
                break;
            case HighlightState.Marked:
                connectionMeshRenderer.material.color = ColorUtility.NegativeMultiplyBlend(baseColor, HighlightColor, 0.3f);
                break;
            case HighlightState.Targeted:
                connectionMeshRenderer.material.color = HighlightColor;
                break;
        }
    }
}
