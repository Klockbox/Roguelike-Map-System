using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointerHandler : SimpleMonoBehaviorSingleton<PointerHandler>
{
    [SerializeField, ReadOnly, Foldout("Info")]
    private Camera pointerCam;
    [SerializeField, ReadOnly, Foldout("Info")]
    private Vector2 pointerPos;

    [SerializeField] private string pointActionReference;
    [SerializeField] private string clickActionReference;
    private InputAction point;
    private InputAction click;
    
    
    [SerializeField]
    private LayerMask checkingLayers;
    
    private readonly RaycastHit[] rayCastHits = new RaycastHit[5];

    [SerializeField, ReadOnly, Foldout("Info")] 
    private bool isHoveringSomething;
    private IClickableObject hoveredObject;
    
    private IClickableObject clickedObject;

    private void Start()
    {
        pointerCam = Camera.main;
        
        point = InputSystem.actions.FindAction("Point");
        
        click = InputSystem.actions.FindAction("Click");
        click.started += OnClick;
        click.performed += OnClick;
        click.canceled += OnClick;
    }

    private void OnDestroy()
    {
        click.started -= OnClick;
        click.performed -= OnClick;
        click.canceled -= OnClick;
    }

    private void Update()
    {
        pointerPos = point.ReadValue<Vector2>();
        
        HandleHovering();

        clickedObject?.OnPointerHold();
    }

    private void HandleHovering()
    {
        if (!pointerCam)
        {
            SetHoveredObject(null);
            return;
        }
        
        Ray ray = pointerCam.ScreenPointToRay(pointerPos);
        
        int hits = Physics.RaycastNonAlloc(ray, rayCastHits, pointerCam.farClipPlane, checkingLayers, QueryTriggerInteraction.Ignore);
        if (hits <= 0)
        {
            SetHoveredObject(null);
            return;
        }

        string dbs = "Hovering:";
        for (int i = 0; i < hits; i++)
        {
            RaycastHit hit = rayCastHits[i];
            dbs += $"\n- {hit.transform.gameObject.name} ";
        }
        //Debug.Log(dbs);
        
        for (int i = hits - 1; i >= 0; i--)
        {
            RaycastHit hit = rayCastHits[i];
            
            IClickableObject clickableObject = hit.collider.GetComponent<IClickableObject>();
            if (clickableObject == null) continue;
            
            SetHoveredObject(clickableObject);
            break;
        }
    }
    
    private void SetHoveredObject(IClickableObject value)
    {
        if(value == hoveredObject) return; // if same as last frame, nothing happens.

        isHoveringSomething = value != null;
        
        if (!isHoveringSomething)
            hoveredObject.OnHoverEnd();
        
        hoveredObject = value;
        
        if (isHoveringSomething)
            hoveredObject.OnHoverStart();
    }
    
    
    //#################| Click |#################
    #region Click
    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnClickStart(context);
            return;
        }
        
        if (context.performed)
        {
            // nothing
            return;
        }

        if (context.canceled)
        {
            OnClickEnd(context);
            return;
        }
    }

    private void OnClickStart(InputAction.CallbackContext context)
    {
        if (hoveredObject == null || Mouse.current == null) return;
        
        hoveredObject.OnPointerDown();
        
        clickedObject = hoveredObject;
    }
    
    private void OnClickEnd(InputAction.CallbackContext context)
    {
        if(hoveredObject != null)
        {
            if (hoveredObject == clickedObject)
                clickedObject.OnPointerUpAsClick();
            else
                hoveredObject.OnPointerUp();
        }
        
        clickedObject = null;
    }
    #endregion
}
