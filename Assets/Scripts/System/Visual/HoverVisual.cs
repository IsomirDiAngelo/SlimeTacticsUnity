using UnityEngine;

public class HoverVisual : MonoBehaviour
{
    [SerializeField] private Transform interactableObjectTransform;
    private IInteractable interactableComponent;

    private bool isSelected; // Used to prevent both selected and hover effects to appear at once

    private void Start()
    {
        if (!interactableObjectTransform.TryGetComponent(out interactableComponent))
        {
            Debug.Log("No interactable object linked!");
        }
        else
        {
            InputManager.Instance.OnHoveredInteractableChanged += InputManager_OnHoveredInteractableChanged;
            InputManager.Instance.OnSelectedInteractableChanged += InputManager_OnSelectedInteractableChanged;
        }

        Hide();
    }

    private void OnDestroy()
    {
        InputManager.Instance.OnHoveredInteractableChanged -= InputManager_OnHoveredInteractableChanged;
        InputManager.Instance.OnSelectedInteractableChanged -= InputManager_OnSelectedInteractableChanged;
    }

    private void InputManager_OnHoveredInteractableChanged(IInteractable hoveredInteractable)
    {
        if (hoveredInteractable == interactableComponent)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void InputManager_OnSelectedInteractableChanged(IInteractable selectedInteractable)
    {
        if (selectedInteractable == interactableComponent)
        {
            isSelected = true;
        }
        else
        {
            isSelected = false;
        }
    }

    private void Show()
    {
        gameObject.SetActive(!isSelected);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
