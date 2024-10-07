using PlayerController;
using UnityEngine;
using UnityEngine.InputSystem;


[DefaultExecutionOrder(-2)]
public class PlayerInput : MonoBehaviour,PlayerControls.IPlayerMovementActions
{

    [SerializeField] private bool HoldToSprint = true;

    public PlayerControls PlayerControls { get; private set; }
    public Vector2 Movement { get; private set; }
    public Vector2 Look { get; private set; }
    public bool SprintToggleOn { get; private set; }


    private void OnEnable()
    {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.PlayerMovement.Enable();
        PlayerControls.PlayerMovement.SetCallbacks(this);
    }

    private void OnDisable()
    {
        PlayerControls.PlayerMovement.Disable();
        PlayerControls.PlayerMovement.RemoveCallbacks(this);
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
       Movement = context.ReadValue<Vector2>();
        print(Movement);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look = context.ReadValue<Vector2>();
    }

    public void OnToggleSprint(InputAction.CallbackContext context)
    {
       if(context.performed)
        {
            SprintToggleOn = HoldToSprint || !SprintToggleOn;
        }
       else if(context.canceled)
        {
            SprintToggleOn = !HoldToSprint && SprintToggleOn;
        }
    }
}
