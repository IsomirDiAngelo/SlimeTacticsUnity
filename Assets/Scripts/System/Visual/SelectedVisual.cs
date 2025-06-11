using UnityEngine;

public class SelectedVisual : MonoBehaviour
{
    [SerializeField] private Transform interactableObjectTransform;
    private IInteractable interactableComponent;

    private void Start()
    {
        if (!interactableObjectTransform.TryGetComponent(out interactableComponent))
        {
            Debug.Log("No interactable object linked!");
        }
        else
        {
            InputManager.Instance.OnSelectedInteractableChanged += InputManager_OnSelectedInteractableChanged;          
        }

        Hide();
    }

    private void InputManager_OnSelectedInteractableChanged(IInteractable selectedInteractable)
    {
        if (selectedInteractable == interactableComponent)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
