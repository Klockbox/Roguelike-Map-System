using System;
using UnityEngine;
[RequireComponent(typeof(UIElementTweener))]
public class EncounterPanel : MonoBehaviour
{
    private UIElementTweener tweener;
    private void Awake()
    {
        tweener = GetComponent<UIElementTweener>();
    }

    private void Start()
    {
        
    }
}
