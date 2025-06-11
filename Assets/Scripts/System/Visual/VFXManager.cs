using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [SerializeField] private Transform clickEffectUiPrefab;

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
        InputManager.Instance.OnMove += InputManager_OnMove;
    }

    private void InputManager_OnMove(Vector3 mouseWorldPosition, SlimeAI actor)
    {
        float verticalOffset = .1f;
        Instantiate(clickEffectUiPrefab, mouseWorldPosition + Vector3.up * verticalOffset, Quaternion.identity);
    }
}