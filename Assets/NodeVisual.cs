using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class NodeVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Node node;
    [SerializeField]
    private Transform meshOrigin;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private TMP_Text costPreviewText;
    
    [Header("Options")]
    private Color idleColor;
    private Color baseColor;
    [SerializeField] private Color illegalColor;
    [SerializeField] private Color highlightColor;

    private Sequence onHoverTween;
    private void Awake()
    {
        idleColor = meshRenderer.material.color;
        node.NodeStateChanged += OnNodeStateChanged;
    }

    private void OnNodeStateChanged()
    {
        UpdateBaseColor();
        
        if (node.PreviewingOutOfReach)
            StartCoroutine(PreviewOutOfReach());
    }

    private void UpdateBaseColor()
    {
        baseColor = node.CurrentNodeState switch
        {
            NodeState.InReach => idleColor,
            NodeState.OutOfReach => ColorUtility.MultiplyBlend(ColorUtility.SimpleGrayConversion(idleColor), Color.black, 0.3f),
            _ => idleColor
        };

        if (node.Hovered)
        {
            baseColor = ColorUtility.NegativeMultiplyBlend(baseColor, highlightColor, 0.4f);
            
            onHoverTween?.Complete();
            onHoverTween = DOTween.Sequence();
            onHoverTween.Append(meshOrigin.transform.DOPunchScale(Vector3.one * 0.05f, 0.5f)).OnComplete(() => onHoverTween = null);
        }
        
        if (node.CurrentRoutingState is RoutingState.Marked)
        {
            baseColor = node.CurrentNodeState switch
            {
                NodeState.InReach => ColorUtility.NegativeMultiplyBlend(baseColor, highlightColor, 0.4f),
                NodeState.OutOfReach => ColorUtility.MultiplyBlend(baseColor, illegalColor, 0.3f),
                _ => idleColor
            };
        }
        
        //update cost preview
        costPreviewText.text = $"- {node.PreviewCost} fuel";
        
        switch (node.CurrentTargetState)
        {
            default:
            case TargetState.NotTargeted:
                meshOrigin.localScale = Vector3.one;
                costPreviewText.enabled = false;
                break;
            case TargetState.RouteEnd:
                costPreviewText.enabled = true;
                costPreviewText.transform.localScale = Vector3.one;
                meshOrigin.localScale = Vector3.one * 1.2f;
                break;
            case TargetState.Targeted:
                costPreviewText.enabled = true;
                costPreviewText.transform.localScale = Vector3.one * 1.2f;
                meshOrigin.localScale = Vector3.one * 1.5f;
                break;
        }
        
        meshRenderer.material.color = baseColor;

        UpdateCostPreviewDisplay();
    }

    private void UpdateCostPreviewDisplay()
    {
        //update cost preview
        costPreviewText.text = $"- {node.PreviewCost} fuel";

        if (node.CurrentNodeState is NodeState.OutOfReach)
        {
            costPreviewText.enabled = false;
            return;
        }
        
        switch (node.CurrentTargetState)
        {
            default:
            case TargetState.NotTargeted:
                costPreviewText.enabled = false;
                break;
            case TargetState.RouteEnd:
                costPreviewText.enabled = true;
                costPreviewText.transform.localScale = Vector3.one;
                break;
            case TargetState.Targeted:
                costPreviewText.enabled = true;
                costPreviewText.transform.localScale = Vector3.one * 1.2f;
                break;
        }
    }
    
    
    
    private IEnumerator PreviewOutOfReach()
    {
        float timer = 0;
        
        while (node.PreviewingOutOfReach)
        {
            timer += Time.deltaTime * 4;
            float delta = (Mathf.Sin(timer) + 1) / 2;
            delta *= 0.5f;
            meshRenderer.material.color = ColorUtility.MultiplyBlend(baseColor, illegalColor, delta);
            yield return null;
        }

        meshRenderer.material.color = baseColor;
    }
}
