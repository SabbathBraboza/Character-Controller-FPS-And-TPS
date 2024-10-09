using PlayerController;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerInput : MonoBehaviour,PlayerControls.IPlayerMovementActions
{
    [SerializeField] private bool HoldToSprint = true;  // Player Sprint hold bool Checks

    public Vector2 Movement { get; private set; }  //Player Movement Vector 3
    public Vector2 Look { get; private set; } // Player Camera Move
    public bool SprintToggleOn { get; private set; }   // Player Sprint Toggle on bool Check
    public bool JumpPressed {  get; private set; } // Player Jump bool Check
    public bool WalkToggleOn { get; private set; } // Player Walk Toggle on bool Check


    private void OnEnable()
    {
        if(PlayerInputManagers.Instance?.PlayerControls == null)
        {
            Debug.Log("Player Controls Are Not Initialized - cannot enable");
            return;
        }
        PlayerInputManagers.Instance.PlayerControls.PlayerMovement.Enable();
        PlayerInputManagers.Instance.PlayerControls.PlayerMovement.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManagers.Instance?.PlayerControls == null)
        {
            Debug.Log("Player Controls Are Not Initialized - cannot disable");
            return;
        }
        PlayerInputManagers.Instance.PlayerControls.PlayerMovement.Disable();
        PlayerInputManagers.Instance.PlayerControls.PlayerMovement.RemoveCallbacks(this);
    }

    private void LateUpdate() => JumpPressed = false;
    
    // Player Movement WASD Input
    public void OnMovement(InputAction.CallbackContext context) => Movement = context.ReadValue<Vector2>();
     
    //Player Camera Input 
    public void OnLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();
    

    //Player Sprint Movement Input
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

    //Player Jump Input
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)return;

        JumpPressed = true;
    }

    public void OnTogglewalk(InputAction.CallbackContext context)
    {
        if(!context.performed)return;

        WalkToggleOn = !WalkToggleOn;
    }
}
