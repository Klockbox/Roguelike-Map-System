using System;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIElementTweener : MonoBehaviour
{
    private RectTransform _rectTransform;
    public RectTransform RectTransform 
    {
        get
        {
            if (_rectTransform) return _rectTransform;
            _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }
    
    [SerializeField, ReadOnly, BoxGroup("Info")]
    public bool IsActive;
    [field: SerializeField, ReadOnly, BoxGroup("Info")]
    public bool IsMoving { get; private set; }
    
    
    [SerializeField, BoxGroup("Settings")] private RectTransformData beginTransform;
    [SerializeField, BoxGroup("Settings")] private float easeInDuration = 1f;
    [SerializeField, BoxGroup("Settings")] private bool useCustomEaseMoveIn;
    [SerializeField, BoxGroup("Settings"), HideIf("useCustomEaseMoveIn")] private Ease easeMoveIn;
    [SerializeField, BoxGroup("Settings"), ShowIf("useCustomEaseMoveIn")] private AnimationCurve customEaseMoveIn;
    
    
    [SerializeField, BoxGroup("Settings")] private RectTransformData activeTransform;
    [SerializeField, BoxGroup("Settings")] private float easeOutDuration = 1f;
    [SerializeField, BoxGroup("Settings")] private bool useCustomEaseMoveOut;
    [SerializeField, BoxGroup("Settings"), HideIf("useCustomEaseMoveOut")] private Ease easeMoveOut;
    [SerializeField, BoxGroup("Settings"), ShowIf("useCustomEaseMoveOut")] private AnimationCurve customEaseMoveOut;
    
    [SerializeField, BoxGroup("Settings")] private bool differingEndPosition;
    [SerializeField, BoxGroup("Settings"), ShowIf("differingEndPosition")] private RectTransformData endTransform;
    
    public bool IsRunning { get; private set; } = false;

    [SerializeField] private bool autoStart = false;
    [SerializeField] private bool destroyOnExit = false;
    private void OnEnable()
    {
        if(!Application.isPlaying) return;
        IsActive = false;
        SetRectTransformToData(RectTransform, beginTransform);
    }

    private void Start()
    {
        if(autoStart)
            SetActive(true);
    }

    [Button]
    private void ChangeState()
    {
        IsActive = !IsActive;
        StartCoroutine(PlayTween());
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        StartCoroutine(PlayTween());
    }

    private IEnumerator PlayTween(Action callback = null)
    {
        IsMoving = true;
        if (IsActive)
        {
            yield return StartCoroutine(useCustomEaseMoveIn ? 
                Move(beginTransform,activeTransform, customEaseMoveIn, easeInDuration) : 
                Move(beginTransform,activeTransform, easeMoveIn, easeInDuration));
        }
        else
        {
            yield return StartCoroutine(useCustomEaseMoveOut ? 
                Move(activeTransform,differingEndPosition ? endTransform : beginTransform, customEaseMoveOut, easeOutDuration) : 
                Move(activeTransform, differingEndPosition ? endTransform : beginTransform, easeMoveOut, easeOutDuration));
            if(destroyOnExit)
                Destroy(gameObject);
        }
    }
    

    private IEnumerator Move(RectTransformData start, RectTransformData target, Ease ease, float duration)
    {
        SetRectTransformToData(RectTransform, start);
        yield return RectTransform.DOAnchorPos3D(target.AnchoredPosition3D, duration).SetEase(ease).WaitForCompletion();
        IsMoving = false;
    }
    private IEnumerator Move(RectTransformData start, RectTransformData target, AnimationCurve ease, float duration)
    {
        SetRectTransformToData(RectTransform, start);
        yield return RectTransform.DOAnchorPos3D(target.AnchoredPosition3D, duration).SetEase(ease).WaitForCompletion();
        IsMoving = false;
    }

    
    [Button]
    private void SetBeginTransform() => SetTransformData(ref beginTransform);
    [Button]
    private void SetActiveTransform() => SetTransformData(ref activeTransform);
    [Button, ShowIf("differingEndPosition")] 
    private void SetEndTransform() => SetTransformData(ref endTransform);
    private void SetTransformData(ref RectTransformData dataToSet) { dataToSet = new RectTransformData(RectTransform); }

    [Button]
    private void SetToBegin() => SetRectTransformToData(RectTransform, beginTransform);
    [Button]
    private void SetToActive() => SetRectTransformToData(RectTransform, activeTransform);
    [Button, ShowIf("differingEndPosition")] 
    private void SetToEnd() => SetRectTransformToData(RectTransform, endTransform);
    
    private static void SetRectTransformToData(RectTransform rectTransform, RectTransformData data)
    {
        rectTransform.anchoredPosition3D = data.AnchoredPosition3D;
        rectTransform.anchorMax = data.AnchorMax;
        rectTransform.anchorMin = data.AnchorMin;
        rectTransform.pivot = data.Pivot;
    }
    
    [Serializable]
    private struct RectTransformData
    {
        [field: SerializeField] public Vector3 AnchoredPosition3D { get; private set; }
        [field: SerializeField] public Vector2 AnchorMax { get; private set; }
        [field: SerializeField] public Vector2 AnchorMin { get; private set; }
        [field: SerializeField] public Vector2 Pivot { get; private set; }
        
        public RectTransformData(RectTransform transform)
        {
            AnchoredPosition3D = transform.anchoredPosition3D;
            AnchorMax = transform.anchorMax;
            AnchorMin = transform.anchorMin;
            Pivot = transform.pivot;
        }
    }
}
