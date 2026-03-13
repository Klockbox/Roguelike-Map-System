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
            case HighlightState.Idle:
                connectionMeshRenderer.material.color = baseColor;
                costText.color = new Color(0.75f, 0.75f, 0.75f, 1);
                costText.fontStyle = FontStyles.Normal;
                costText.fontSize = 0.3f;
                break;
            case HighlightState.Hovered:
                connectionMeshRenderer.material.color = ColorUtility.NegativeMultiplyBlend(baseColor, HighlightColor, 0.3f);
                
                costText.color = Color.white;
                costText.fontSize = 0.35f;
                costText.fontStyle = FontStyles.Bold;
                break;
            case HighlightState.Targeted:
                connectionMeshRenderer.material.color = HighlightColor;
                costText.color = Color.white;
                costText.fontSize = 0.35f;
                costText.fontStyle = FontStyles.Bold;
                
                break;
        }
    }
}
