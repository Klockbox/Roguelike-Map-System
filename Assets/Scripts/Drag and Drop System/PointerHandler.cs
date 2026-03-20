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
    [SerializeField] private string altClickActionReference;
    private InputAction point;
    private InputAction click;
    private InputAction altClick;
    
    [SerializeField]
    private LayerMask checkingLayers;
    
    private readonly RaycastHit[] rayCastHits = new RaycastHit[5];

    [SerializeField, ReadOnly, Foldout("Info")] 
    private bool isHoveringSomething;
    private IClickableObject hoveredObject;
    
    private IClickableObject clickedObject;


    public static event Action<IClickableObject> PointerMainDown;
    public static event Action<IClickableObject> PointerMainUp;
    
    public static event Action<IClickableObject> PointerAltDown;
    public static event Action<IClickableObject> PointerAltUp;
    
    private void Start()
    {
        pointerCam = Camera.main;
        
        point = InputSystem.actions.FindAction(pointActionReference);
        
        click = InputSystem.actions.FindAction(clickActionReference);
        click.started += OnMainClick;
        click.performed += OnMainClick;
        click.canceled += OnMainClick;
        
        altClick = InputSystem.actions.FindAction(altClickActionReference);
        altClick.started += OnAltClick;
        altClick.performed += OnAltClick;
        altClick.canceled += OnAltClick;
    }

    private void OnDestroy()
    {
        click.started -= OnMainClick;
        click.performed -= OnMainClick;
        click.canceled -= OnMainClick;
        
        altClick.started -= OnAltClick;
        altClick.performed -= OnAltClick;
        altClick.canceled -= OnAltClick;
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

        hoveredObject?.OnHoverEnd();
        hoveredObject = value;
        hoveredObject?.OnHoverStart();
    }
    
    
    //#################| Click |#################
    #region MainClick
    public void OnMainClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PointerMainDown?.Invoke(hoveredObject);
            OnMainClickStart(context);
            return;
        }
        
        if (context.performed)
        {
            // nothing
            return;
        }

        if (context.canceled)
        {
            PointerMainUp?.Invoke(hoveredObject);
            OnMainClickEnd(context);
            return;
        }
    }

    private void OnMainClickStart(InputAction.CallbackContext context)
    {
        if (hoveredObject == null || Mouse.current == null) return;
        
        hoveredObject.OnPointerDown();
        
        clickedObject = hoveredObject;
    }
    
    private void OnMainClickEnd(InputAction.CallbackContext context)
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
    
    #region AltClick
    public void OnAltClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PointerAltDown?.Invoke(hoveredObject);
            OnAltClickStart(context);
            return;
        }
        
        if (context.performed)
        {
            // nothing
            return;
        }

        if (context.canceled)
        {
            PointerAltUp?.Invoke(hoveredObject);
            OnAltClickEnd(context);
            return;
        }
    }

    private void OnAltClickStart(InputAction.CallbackContext context)
    {
        if (Mouse.current == null) return;
        hoveredObject?.OnPointerDown();
    }
    
    private void OnAltClickEnd(InputAction.CallbackContext context)
    {
        hoveredObject?.OnAltClickUp();
    }
    #endregion
}
