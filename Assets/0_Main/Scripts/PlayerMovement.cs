using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Values:")]
    public float RunAcceleration = 0.25f;
    public float RunSpeed = 4f;
    public float SprintAccelreation = 0.5f;
    public float SprintSpeed = 7f;  
    public float Drag = 0.1f;
    public float MovingThreshold = 0.01f;
    public float Gravity = 25f;
    public float JumpSpeed = 1f;
    
    [Header("Camera Settings:")]
    public float LookSenseH = 0.1f;
    public float LookSensev = 0.1f;
    public float LookLimitV = 90f;

    [Space(5f)]
    [Header("References:")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera PlayerCamera;

    private PlayerInput playerInput;
    private PlayerState playerState;

    private Vector2 CameraRotation = Vector2.zero;
    private Vector2 PlayerRotation = Vector2.zero;

    private float VerticalVelocity = 0f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        HandleMovement();
        HandleVerticalMovement();
        UpdateMovementState();
    }

    private void UpdateMovementState()
    {
        bool isMovementInput = playerInput.Movement != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprintLaterally = playerInput.SprintToggleOn && isMovingLaterally;
        bool isGrounded = IsGrounded();

        PlayerMovementState lateralState =  isSprintLaterally ? PlayerMovementState.Sprinting:
            isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;
        playerState.SetPlayerMovementState(lateralState);

        // Character Jumps
        if (!isGrounded && characterController.velocity.y > 0f) playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
        
        else if(!isGrounded && characterController.velocity.y < 0f) playerState.SetPlayerMovementState(PlayerMovementState.Falling);
    }

    private void HandleMovement()
    {
        bool isSprinting =  playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = playerState.IsGroundedState();        

        float LateralAcceleration =  isSprinting ? SprintAccelreation : RunAcceleration;
        float LateralClampMagnitude = isSprinting ? SprintSpeed : RunSpeed;

        Vector3 CameraForwardXZ = new Vector3(PlayerCamera.transform.forward.x, 0f, PlayerCamera.transform.forward.z).normalized;
        Vector3 CameraRightXZ = new Vector3(PlayerCamera.transform.right.x, 0f, PlayerCamera.transform.right.z).normalized;
        Vector3 movementDirection = CameraRightXZ * playerInput.Movement.x + CameraForwardXZ * playerInput.Movement.y;

        Vector3 movementDelta = movementDirection * LateralAcceleration;
        Vector3 newVelocity = characterController.velocity + movementDelta;

        // Player Drag
        Vector3 CurrentDrag = newVelocity.normalized * Drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > Drag * Time.deltaTime) ? newVelocity - CurrentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, LateralClampMagnitude);
        newVelocity.y += VerticalVelocity;  

        // Player Moves 
        characterController.Move(newVelocity * Time.deltaTime);
    }
     
    private void HandleVerticalMovement()
    {
        bool IsGrounded = playerState.IsGroundedState();

        if(IsGrounded && VerticalVelocity <0) VerticalVelocity = 0;

        VerticalVelocity -= Gravity * Time.deltaTime;

        if (playerInput.JumpPressed && IsGrounded) VerticalVelocity += Mathf.Sqrt(JumpSpeed * 3 * Gravity);
    }

    private void LateUpdate()
    {
        CameraRotation.x += LookSenseH * playerInput.Look.x;
        CameraRotation.y = Mathf.Clamp(CameraRotation.y - LookSensev * playerInput.Look.y, -LookSensev, LookLimitV);

        PlayerRotation.x += transform.eulerAngles.x + LookSenseH * playerInput.Look.x;
        transform.rotation = Quaternion.Euler(0f, PlayerRotation.x,0f);

        PlayerCamera.transform.rotation = Quaternion.Euler(CameraRotation.y, CameraRotation.x, 0f);
    }

    private bool IsMovingLaterally()
    {
        Vector3 LateralVelocity =  new Vector3(characterController.velocity.x,characterController.velocity.y,characterController.velocity.z);

        return LateralVelocity.magnitude > MovingThreshold; 
    }

    private bool IsGrounded()
    {
        return characterController.isGrounded;
    }
}
