using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
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
    private Transform meshTransform;
    [SerializeField]
    private MeshRenderer meshRenderer;
    
    [SerializeField] private TMP_Text consumptionText;
    [SerializeField] private Canvas costPreviewCanvas;
    
    [Header("Options")]
    private Color idleColor;
    private Color baseColor;
    [SerializeField] private Color illegalColor;
    [SerializeField] private Color highlightColor;

    private Sequence onHoverTween;
    private Sequence neighborTween;
    
    [SerializeField] private Vector3 scaleOffset_Visited = Vector3.one;
    [SerializeField] private Vector3 scaleOffset_HoverFeedback = Vector3.one;
    [SerializeField] private Vector3 scaleOffset_TargetState = Vector3.one;
    [SerializeField] private Vector3 scaleOffset_IsNeighbor = Vector3.one;
    [SerializeField] private Vector3 scaleOffset_IsNeighborTween = Vector3.one;
    
    private void Awake()
    {
        idleColor = meshRenderer.material.color;
        node.NodeStateChanged += OnNodeStateChanged;
        node.NeighboringChanged += OnNeighboringChanged;
    }

    private void OnNeighboringChanged()
    {
        neighborTween.Rewind();
        neighborTween.Kill();
        switch (node.IsNeighbor)
        {
            case true when node.CurrentNodeState == NodeState.InReach:
                scaleOffset_IsNeighbor = new Vector3(1.4f, 1.4f, 1.4f);
                neighborTween = DOTween.Sequence().SetLoops(-1, LoopType.Yoyo);
                neighborTween.Append(
                    DOTween.To(
                        () => scaleOffset_IsNeighborTween, 
                        x => scaleOffset_IsNeighborTween = x, 
                        new Vector3(1.1f, 1.1f, 1.1f), 
                        1f
                        ).SetEase(Ease.InOutQuad));
                break;
            default:
                scaleOffset_IsNeighbor = Vector3.one;
                break;
        }
    }

    private void OnNodeStateChanged()
    {
        UpdateDimensions();
        UpdateBaseColor();
        UpdateCostPreviewDisplay();
        
        if (node.PreviewingOutOfReach)
            StartCoroutine(PreviewOutOfReach());
    }

    private void UpdateDimensions()
    {
        scaleOffset_Visited = node.Visited switch
        {
            true => new Vector3(1, 0.2f, 1),
            false => new Vector3(1, 1, 1)
        };
        
        switch (node.CurrentTargetState)
        {
            default:
            case TargetState.NotTargeted:
                scaleOffset_TargetState = Vector3.one;
                break;
            case TargetState.RouteEnd:
                scaleOffset_TargetState = Vector3.one * 1.1f;
                break;
            case TargetState.Targeted:
                scaleOffset_TargetState = Vector3.one * 1.2f;
                break;
        }
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
            //onHoverTween.Append(meshTransform.transform.DOPunchScale(Vector3.one * 0.05f, 0.5f)).OnComplete(() => onHoverTween = null);
            TweenerCore<Vector3, Vector3[], Vector3ArrayOptions> punchTween = DOTween.Punch(() => scaleOffset_HoverFeedback, x => scaleOffset_HoverFeedback = x, Vector3.one * 0.05f, 0.5f);
            onHoverTween.Append(punchTween);
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
        
        
        
        meshRenderer.material.color = baseColor;
    }

    private void Update()
    {
        meshOrigin.localScale = new Vector3(
            scaleOffset_Visited.x * scaleOffset_HoverFeedback.x * scaleOffset_TargetState.x * scaleOffset_IsNeighbor.x * scaleOffset_IsNeighborTween.x,
            scaleOffset_Visited.y * scaleOffset_HoverFeedback.y * scaleOffset_TargetState.y * scaleOffset_IsNeighbor.y * scaleOffset_IsNeighborTween.y,
            scaleOffset_Visited.z * scaleOffset_HoverFeedback.z * scaleOffset_TargetState.z * scaleOffset_IsNeighbor.z * scaleOffset_IsNeighborTween.z
        );
    }

    private void UpdateCostPreviewDisplay()
    {
        if(!costPreviewCanvas || !consumptionText) return;
        
        //update cost preview
        consumptionText.text = $"{node.PreviewCost}";

        if (node.CurrentNodeState is NodeState.OutOfReach)
        {
            consumptionText.enabled = false;
            return;
        }
        
        
        
        costPreviewCanvas.gameObject.SetActive(node.CurrentTargetState is not TargetState.NotTargeted);
        costPreviewCanvas.transform.localScale = node.CurrentTargetState switch
        {
            TargetState.Targeted => Vector3.one * 1.2f,
            _ => Vector3.one
        };
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
