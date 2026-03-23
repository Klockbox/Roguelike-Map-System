using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(UIElementTweener))]
public class MapEncounter : MonoBehaviour
{
    public event Action Close;
    
    private UIElementTweener tweener;
    public bool IsClosing => tweener.IsMoving;
    
    
    private protected virtual void Awake()
    {
        tweener = GetComponent<UIElementTweener>();
    }

    
    public void EndEncounter()
    {
        Close?.Invoke();
        tweener.Close();
    }
    public void EndEncounter(Action callback)
    {
        Close?.Invoke();
        tweener.Close(callback);
    }
}
