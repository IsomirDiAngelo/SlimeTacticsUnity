using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public event Action<float> OnZoom;
    public event Action<Vector3, SlimeAI> OnMove;
    public event Action<Transform> OnInteract;
    public event Action<IInteractable> OnHoveredInteractableChanged;
    public event Action<IInteractable> OnSelectedInteractableChanged;
    public event Action<Vector3> OnSpawn;

    private PlayerInputActions playerInputActions;

    private IInteractable hoveredInteractable;
    private IInteractable selectedInteractable;

    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private PlayerAI player;

    private bool isHoveringUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Zoom.performed += Zoom_performed;
        playerInputActions.Player.MoveInteract.performed += MoveInteract_performed;
        playerInputActions.Player.Select.performed += Select_performed;
    }

    private void Update()
    {
        TryGetInteractableOnCursor(out Transform interactableTransform);
    }

    public void SetHoveringUI(bool hoveringUI)
    {
        isHoveringUI = hoveringUI;
    }

    private SlimeAI GetActor()
    {
        SlimeAI actor = player;
        if (selectedInteractable != null && selectedInteractable is SlimeAI interactableSlimeAI)
        {
            actor = interactableSlimeAI;
        }
        return actor;
    }

    private void Select_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (TryGetInteractableOnCursor(out Transform interactableTransform))
        {
            if (interactableTransform.TryGetComponent(out IInteractable interactableComponent))
            {
                selectedInteractable = interactableComponent;
                hoveredInteractable = null;
            }
        }
        else if (TryGetMouseTerrainWorldPosition(out Vector3 mouseWorldPosition))
        {
            OnSpawn?.Invoke(mouseWorldPosition);
            selectedInteractable = null;
        }
        else
        {
            selectedInteractable = null;
        }

        OnSelectedInteractableChanged?.Invoke(selectedInteractable);
    }

    private void MoveInteract_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (TryGetInteractableOnCursor(out Transform interactableTransform))
        {
            if (interactableTransform.TryGetComponent(out IInteractable interactableComponent))
            {
                interactableComponent.Interact(GetActor());
            }
        } 
        else if (TryGetMouseTerrainWorldPosition(out Vector3 mouseWorldPosition))
        {
            OnMove?.Invoke(mouseWorldPosition, GetActor());
        }
    }

    private void Zoom_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Vector2 scrollInput = obj.ReadValue<Vector2>();
        
        if (scrollInput != Vector2.zero)
        {
            float zoomValue = obj.ReadValue<Vector2>().y;
            OnZoom?.Invoke(zoomValue);
        }
    }

    private bool TryGetInteractableOnCursor(out Transform interactableTransform)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!isHoveringUI && Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactableComponent))
            {
                hoveredInteractable = interactableComponent;
                OnHoveredInteractableChanged?.Invoke(hoveredInteractable);
                interactableTransform = hit.transform;
                return true;
            }
        }

        hoveredInteractable = null;
        OnHoveredInteractableChanged?.Invoke(hoveredInteractable);
        interactableTransform = null;
        return false;
    }

    private bool TryGetMouseTerrainWorldPosition(out Vector3 mouseWorldPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!isHoveringUI && Physics.Raycast(ray, out RaycastHit hit, terrainLayerMask))
        {
            mouseWorldPosition = hit.point;
            return true;
        }

        mouseWorldPosition = Vector3.zero;
        return false;
    }
}