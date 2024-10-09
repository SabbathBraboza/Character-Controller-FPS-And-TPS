using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerController.PlayerControls;

public class ThridPersonInputs : MonoBehaviour, IThirdPersonMapActions
{

    public Vector2 ScrollInput {  get; private set; }

    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float CameraZoomSpeed;
    [SerializeField] private float CameraMinZoom;
    [SerializeField] private float CameraMaxZoom;

    private Cinemachine3rdPersonFollow ThridPersonFollow;

    private void Awake()
    {
        ThridPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    private void OnEnable()
    {
        if (PlayerInputManagers.Instance?.PlayerControls == null)
        {
            Debug.Log("Player Controls Are Not Initialized - cannot enable");
            return;
        }
        PlayerInputManagers.Instance.PlayerControls.ThirdPersonMap.Enable();
        PlayerInputManagers.Instance.PlayerControls.ThirdPersonMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManagers.Instance?.PlayerControls == null)
        {
            Debug.Log("Player Controls Are Not Initialized - cannot disable");
            return;
        }
        PlayerInputManagers.Instance.PlayerControls.ThirdPersonMap.Disable();
        PlayerInputManagers.Instance.PlayerControls.ThirdPersonMap.RemoveCallbacks(this);
    }

    private void Update() => ThridPersonFollow.CameraDistance = Mathf.Clamp(ThridPersonFollow.CameraDistance + ScrollInput.y, CameraMinZoom, CameraMaxZoom);
    
    private void LateUpdate() => ScrollInput = Vector3.zero;
    
    public void OnScrollCamera(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 scrollInput = context.ReadValue<Vector2>();
        ScrollInput = -1f * scrollInput.normalized * CameraZoomSpeed;
    }
}
