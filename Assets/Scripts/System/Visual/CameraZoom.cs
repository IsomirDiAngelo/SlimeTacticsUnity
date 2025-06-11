using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private CinemachineCamera vcamera;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;

    private float targetZoom;

    private void Awake()
    {
        vcamera = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        InputManager.Instance.OnZoom += InputManager_OnZoom;
        
        if (vcamera != null)
        {
            targetZoom = vcamera.Lens.OrthographicSize;
        }
    }

    private void Update()
    {
        if (vcamera != null && targetZoom != vcamera.Lens.OrthographicSize)
        {
            vcamera.Lens.OrthographicSize = Mathf.Lerp(vcamera.Lens.OrthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        }
    }

    private void InputManager_OnZoom(float zoomValue)
    {
        targetZoom = Mathf.Clamp(vcamera.Lens.OrthographicSize - zoomValue, minZoom, maxZoom);
    }
}
