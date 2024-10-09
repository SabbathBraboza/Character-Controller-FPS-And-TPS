using PlayerController;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputAction : MonoBehaviour, PlayerControls.IPlayerActionActions
{
    public bool Attack1Pressed { get; private set; }
    public bool Attack2Pressed { get; private set; }
    public bool GotHurt {  get; private set; }


    private void OnEnable()
    {
        if (PlayerInputManagers.Instance?.PlayerControls == null)
        {
            Debug.Log("Player Controls Are Not Initialized - cannot enable");
            return;
        }
        PlayerInputManagers.Instance.PlayerControls.PlayerAction.Enable();
        PlayerInputManagers.Instance.PlayerControls.PlayerAction.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManagers.Instance?.PlayerControls == null)
        {
            Debug.Log("Player Controls Are Not Initialized - cannot disable");
            return;
        }
        PlayerInputManagers.Instance.PlayerControls.PlayerAction.Disable();
        PlayerInputManagers.Instance.PlayerControls.PlayerAction.RemoveCallbacks(this);
    }

    private void LateUpdate()
    {
        Attack1Pressed = false;
        Attack2Pressed = false;
        GotHurt = false;
    }

    public void OnAttack1(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Attack1Pressed = true;
    }

    public void OnAttack2(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Attack2Pressed = true;
    }

    public void OnHurt(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        GotHurt = true;
    }
}
