using System;
using UnityEngine;

public class MoveButton : MonoBehaviour
{
    private void Awake()
    {
        RouteManager.RoutingModeChanged += RouteManagerOnRoutingModeChanged;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Hide();
    }
    
    private void RouteManagerOnRoutingModeChanged(ERoutingMode mode)
    {
        switch (mode)
        {
            default:
            case ERoutingMode.Freeform:
                Hide();
                break;
            case ERoutingMode.Planning:
                Show();
                break;
        }
    }
    
    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
}
