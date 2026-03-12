using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Connector))]
public class ConnectorVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private MeshRenderer connectionMeshRenderer;
    [SerializeField]
    private TMP_Text costText;
    
    [Header("Options")]
    private Color baseColor;
    public Color HighlightColor;

    private void Awake()
    {
        baseColor = connectionMeshRenderer.material.color;
        GetComponent<Connector>().HighlightStateChanged += SetHighlight;  // connector nulls listeners on destroy
    }

    public void SetCostText(int costs)
    {
        costText.text = costs.ToString();
    }

    private void SetHighlight(HighlightState state)
    {
        switch (state)
        {
            default:
            case HighlightState.NotHighlighted:
                connectionMeshRenderer.material.color = baseColor;
                costText.fontSize = 0.3f;
                break;
            case HighlightState.LightlyHighlighted:
                connectionMeshRenderer.material.color = ColorUtility.NegativeMultiplyBlend(baseColor, HighlightColor, 0.3f);
                costText.fontSize = 0.5f;
                break;
            case HighlightState.Highlighted:
                connectionMeshRenderer.material.color = HighlightColor;
                costText.fontSize = 0.5f;
                break;
        }
    }
}
