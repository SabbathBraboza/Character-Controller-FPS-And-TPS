using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Values:")]
    public float WalkAcceleration = 1f;
    public float WalkSpeed = 1.5f;
    public float RunAcceleration = 0.25f;
    public float RunSpeed = 4f;
    public float SprintAccelreation = 0.5f;
    public float SprintSpeed = 7f;  
    public float Drag = 0.1f;
    public float MovingThreshold = 0.01f;
    public float Gravity = 25f;
    public float JumpSpeed = 1f;
    public float InAirAccleration = 0.15f;
    public float TermaialVelocity = 2f;

    [Space(5f)]
    [Header("Camera Settings:")]
    public float LookSenseH = 0.1f;
    public float LookSensev = 0.1f;
    public float LookLimitV = 90f;

    [Space(5f)]
    [Header("Animation:")]
    public float PlayerModelRotationSpeed = 10f;
    public float RotationTargetTime = 0.25f;

    [Space(5f)]
    [Header("Environment Layer:")]
     public LayerMask GroundLayer;

    [Space(5f)]
    [Header("References:")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera PlayerCamera;

    public float RotationMisMatch { get; private set; } = 0f;
    public bool IsRotatingToTarget {  get; private set; } = false;

    private PlayerMovementState lastMovementState = PlayerMovementState.Falling;

    private PlayerInput playerInput;
    private PlayerState playerState;

    private Vector2 CameraRotation = Vector2.zero;
    private Vector2 PlayerRotation = Vector2.zero;

    private bool JumpLastframe = false;
    private bool IsRotatingClockWise = false;

    private float rotationToTargetTimer = 0f;
    private float VerticalVelocity = 0f;
    private float AntiBump;
    private float StepOffset;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerState = GetComponent<PlayerState>();

        AntiBump = WalkSpeed;
        StepOffset = characterController.stepOffset;
    }

    private void Update()
    {
        HandleMovement();
        HandleVerticalMovement();
        UpdateMovementState();
    }

    private void UpdateMovementState()
    {
        lastMovementState = playerState.CurrentPlayerMovementState;

        bool canRun = CanRun();
        bool isMovementInput = playerInput.Movement != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprintLaterally = playerInput.SprintToggleOn && isMovingLaterally;
        bool isWalking = (isMovingLaterally && !canRun) || playerInput.WalkToggleOn;
        bool isGrounded = IsGrounded();

        PlayerMovementState lateralState = isWalking ? PlayerMovementState.Walking :
            isSprintLaterally ? PlayerMovementState.Sprinting :
            isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;
        playerState.SetPlayerMovementState(lateralState);

        // Character Jumps
        if ((!isGrounded || JumpLastframe) && characterController.velocity.y > 0f)
        {
            playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            JumpLastframe = false;
            characterController.stepOffset = 0f;
        }
        else if ((!isGrounded || JumpLastframe) && characterController.velocity.y < 0f)
        {
            playerState.SetPlayerMovementState(PlayerMovementState.Falling);
            JumpLastframe = false;
            characterController.stepOffset = 0f;
        }
        else
        {
            characterController.stepOffset = StepOffset;
        }
    }


    private Vector3 HandlwSteepWalls(Vector3 Velocity)
    {
        Vector3 normal = CharacterUtility.GetNormalWithSphereCast(characterController, GroundLayer);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= characterController.slopeLimit;

        if (!validAngle && VerticalVelocity <0f)
        {
            Velocity = Vector3.ProjectOnPlane(Velocity, normal);
        }
        return Velocity;
    }

    private void HandleMovement()
    {
        bool isSprinting =  playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = playerState.IsGroundedState();
        bool isWalking = playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

        float LateralAcceleration = !isGrounded ? InAirAccleration : isWalking ? WalkAcceleration: isSprinting ? SprintAccelreation : RunAcceleration;
        float LateralClampMagnitude = !isGrounded ? SprintSpeed: isWalking ? WalkSpeed : isSprinting ? SprintSpeed : RunSpeed;

        Vector3 CameraForwardXZ = new Vector3(PlayerCamera.transform.forward.x, 0f, PlayerCamera.transform.forward.z).normalized;
        Vector3 CameraRightXZ = new Vector3(PlayerCamera.transform.right.x, 0f, PlayerCamera.transform.right.z).normalized;
        Vector3 movementDirection = CameraRightXZ * playerInput.Movement.x + CameraForwardXZ * playerInput.Movement.y;

        Vector3 movementDelta = movementDirection * LateralAcceleration;
        Vector3 newVelocity = characterController.velocity + movementDelta;

        // Player Drag
        Vector3 CurrentDrag = newVelocity.normalized * Drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > Drag * Time.deltaTime) ? newVelocity - CurrentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f,newVelocity.z), LateralClampMagnitude);
        newVelocity.y += VerticalVelocity;  
        newVelocity = !isGrounded? HandlwSteepWalls(newVelocity) : newVelocity;

        // Player Moves 
        characterController.Move(newVelocity * Time.deltaTime);
    }

    private void HandleVerticalMovement()
    {
        bool IsGrounded = playerState.IsGroundedState();

        VerticalVelocity -= Gravity * Time.deltaTime;

        if (IsGrounded && VerticalVelocity < 0) VerticalVelocity = -AntiBump;

        if (playerInput.JumpPressed && IsGrounded)
        {
            VerticalVelocity += AntiBump+ Mathf.Sqrt(JumpSpeed * 3 * Gravity);
            JumpLastframe = true;
        }

        if(playerState.IsStateGroundedState(lastMovementState) && !IsGrounded)
        {
            VerticalVelocity += AntiBump;
        }

        if(Mathf.Abs(VerticalVelocity) > Mathf.Abs(TermaialVelocity))
        {
            VerticalVelocity = -1f *Mathf.Abs(TermaialVelocity);
        }
    }

    private void LateUpdate() => UpdateCameraRotation();

     #region Camera 
    private void UpdateCameraRotation()
    {
        CameraRotation.x += LookSenseH * playerInput.Look.x;
        CameraRotation.y = Mathf.Clamp(CameraRotation.y - LookSensev * playerInput.Look.y, -LookSensev, LookLimitV);

        PlayerRotation.x += transform.eulerAngles.x + LookSenseH * playerInput.Look.x;

        float RotationTolerance = 90f;
        bool isIdling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        IsRotatingToTarget = rotationToTargetTimer > 0;

        if (!isIdling) RotatePlayerToTarget();
        else if (Mathf.Abs(RotationMisMatch) > RotationTolerance || IsRotatingToTarget)
        {
            UpdateIdleRotation(RotationTolerance);
        }
        PlayerCamera.transform.rotation = Quaternion.Euler(CameraRotation.y, CameraRotation.x, 0f);

        //Get angle between Camera And Player
        Vector3 CamForwardProjectedXZ = new Vector3(PlayerCamera.transform.forward.x, 0, PlayerCamera.transform.forward.z).normalized;
        Vector3 CrossProduct = Vector3.Cross(transform.forward, CamForwardProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(CrossProduct,transform.up));
        RotationMisMatch = sign * Vector3.Angle(transform.forward, CamForwardProjectedXZ);
    }
    #endregion

    private void UpdateIdleRotation(float RotationTolerance)
    {
        if (Mathf.Abs(RotationMisMatch) > RotationTolerance)
        {
            rotationToTargetTimer = RotationTargetTime;
            IsRotatingClockWise = RotationMisMatch > RotationTolerance;
        }

        rotationToTargetTimer -= Time.deltaTime;

        if (IsRotatingClockWise && RotationMisMatch > 0f || !IsRotatingClockWise && RotationMisMatch < 0f)
        {
            RotatePlayerToTarget();
        }
    }

    private void RotatePlayerToTarget()
    {
        Quaternion TargetRotationX = Quaternion.Euler(0f, PlayerRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotationX, PlayerModelRotationSpeed * Time.deltaTime);
    }

    private bool IsMovingLaterally()
    {
        Vector3 LateralVelocity =  new Vector3(characterController.velocity.x,characterController.velocity.y,characterController.velocity.z);

        return LateralVelocity.magnitude > MovingThreshold; 
    }

    private bool IsGrounded()
    {
        bool grounded = playerState.IsGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();  
        return characterController.isGrounded;
    }

    private bool IsGroundedWhileGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x,transform.position.y - characterController.radius, transform.position.z);

        bool grounded = Physics.CheckSphere(spherePosition, characterController.radius, GroundLayer, QueryTriggerInteraction.Ignore);
        return grounded;
    }

    private bool IsGroundedWhileAirborne()
    {
        Vector3 normal = CharacterUtility.GetNormalWithSphereCast(characterController, GroundLayer);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= characterController.slopeLimit;


        return characterController.isGrounded && validAngle;
    }

    private bool CanRun()
    {
        return playerInput.Movement.y >= Mathf.Abs(playerInput.Movement.x);
    }
}
