using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    [Header("Options")]
    private Color idleColor;
    private Color baseColor;
    [SerializeField] private Color stopoverColor;
    [SerializeField] private Color illegalColor;
    [SerializeField] private Color highlightColor;

    private Sequence onHoverTween;
    private Sequence neighborTween;
    
    [SerializeField, ReadOnly] private Vector3 scaleOffset_Visited = Vector3.one;
    [SerializeField, ReadOnly] private Vector3 scaleOffset_HoverFeedback = Vector3.one;
    [SerializeField, ReadOnly] private Vector3 scaleOffset_TargetState = Vector3.one;
    [SerializeField, ReadOnly] private Vector3 scaleOffset_IsNeighbor = Vector3.one;
    [SerializeField, ReadOnly] private Vector3 scaleOffset_IsNeighborTween = Vector3.one;

    [SerializeField] private StopoverPin pinPrefab;
    [SerializeField] private StopoverPin spawnedPin;
    [SerializeField] private Transform stopoverPinSpawn;
    
    private void Awake()
    {
        idleColor = meshRenderer.material.color;
        node.NodeStateChanged += OnNodeStateChanged;
        node.NeighboringChanged += OnNeighboringChanged;
        node.StopoverStateChanged += OnStopoverStateChanged;
    }

    private void OnDestroy()
    {
        node.NodeStateChanged -= OnNodeStateChanged;
        node.NeighboringChanged -= OnNeighboringChanged;
        node.StopoverStateChanged -= OnStopoverStateChanged;
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

        scaleOffset_TargetState = node.CurrentTargetState switch
        {
            TargetState.Targeted when node.IsInteractable => Vector3.one * 1.6f,
            _ => Vector3.one
        };
    }

    private void OnStopoverStateChanged(bool isStopover)
    {
        if (isStopover)
            spawnedPin = Instantiate(pinPrefab, stopoverPinSpawn);
        else if (spawnedPin )
            Destroy(spawnedPin.gameObject);
    }

    private void UpdateBaseColor()
    {
        baseColor = node.IsStopover switch
        {
            false => idleColor,
            true => stopoverColor
        }; 
        
        
        baseColor = node.CurrentNodeState switch
        {
            NodeState.InReach => baseColor,
            NodeState.OutOfReach => ColorUtility.MultiplyBlend(ColorUtility.SimpleGrayConversion(idleColor), Color.black, 0.3f),
            _ => baseColor
        };

        if (node.IsInteractable)
            UpdateColorForInteractable();
        
        meshRenderer.material.color = baseColor;
    }

    private void UpdateColorForInteractable()
    {
        if (node.Hovered)
        {
            baseColor = ColorUtility.NegativeMultiplyBlend(baseColor, highlightColor, 0.4f);
            
            onHoverTween?.Complete();
            onHoverTween = DOTween.Sequence();
            //onHoverTween.Append(meshTransform.transform.DOPunchScale(Vector3.one * 0.05f, 0.5f)).OnComplete(() => onHoverTween = null);
            TweenerCore<Vector3, Vector3[], Vector3ArrayOptions> punchTween = DOTween.Punch(() => scaleOffset_HoverFeedback, x => scaleOffset_HoverFeedback = x, Vector3.one * 0.2f, 0.5f);
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
    }

    private void Update()
    {
        Vector3 neighborOrTargetOffset = node.CurrentTargetState is TargetState.Targeted
            ? scaleOffset_TargetState
            : scaleOffset_IsNeighbor;
        
        meshOrigin.localScale = new Vector3(
            scaleOffset_Visited.x * scaleOffset_HoverFeedback.x * neighborOrTargetOffset.x * scaleOffset_IsNeighborTween.x,
            scaleOffset_Visited.y * scaleOffset_HoverFeedback.y * neighborOrTargetOffset.y * scaleOffset_IsNeighborTween.y,
            scaleOffset_Visited.z * scaleOffset_HoverFeedback.z * neighborOrTargetOffset.z * scaleOffset_IsNeighborTween.z
        );
    }
    
    
    private IEnumerator PreviewOutOfReach()
    {
        while (node.PreviewingOutOfReach)
        {
            float delta = (Mathf.Sin(Time.time * 4) + 1) / 2;
            delta *= 0.5f;
            meshRenderer.material.color = ColorUtility.MultiplyBlend(baseColor, illegalColor, delta);
            yield return null;
        }

        meshRenderer.material.color = baseColor;
    }
}
