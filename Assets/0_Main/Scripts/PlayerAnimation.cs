using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anime;
    [SerializeField] private float LocomotionBlendSpeed = 0.2f;

    private Vector3 CurrentBlendInput = Vector3.zero;

    private static readonly int InputXHash = Animator.StringToHash("InputX");
    private static readonly int InputYHash = Animator.StringToHash("InputY");
    private static readonly int IsIdlingHash = Animator.StringToHash("IsIdling");
    private static readonly int IsTargetRotatHash = Animator.StringToHash("IsTargetRotation"); 
    private static readonly int InputMagnitudeHash = Animator.StringToHash("InputMagnitude");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");
    private static readonly int RotationMisMatchHash = Animator.StringToHash("RotationMisMatch");

    private PlayerInput playerInput;
    private PlayerState playerState;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerState = GetComponent<PlayerState>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update() => UpdateAnimationState();
    
    private void UpdateAnimationState()
    {
        bool IsIdling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        bool IsWalking = playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;
        bool IsFalling = playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
        bool IsJumping = playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
        bool IsSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool IsRunning = playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
        bool IsGrounded = playerState.IsGroundedState();

        Vector2 InputTarget = IsSprinting ? playerInput.Movement * 1.5f : IsRunning ? playerInput.Movement *1f : playerInput.Movement * 0.5f;
        CurrentBlendInput = Vector3.Lerp(CurrentBlendInput, InputTarget, LocomotionBlendSpeed * Time.deltaTime);

        anime.SetBool(IsGroundedHash, IsGrounded);
        anime.SetBool(IsFallingHash,IsFalling);
        anime.SetBool(IsJumpingHash, IsJumping);
        anime.SetBool(IsIdlingHash, IsIdling);
        anime.SetBool(IsTargetRotatHash, playerMovement.IsRotatingToTarget);

        anime.SetFloat(RotationMisMatchHash, playerMovement.RotationMisMatch);
        anime.SetFloat(InputXHash, CurrentBlendInput.x);
        anime.SetFloat (InputYHash, CurrentBlendInput.y);
        anime.SetFloat(InputMagnitudeHash,CurrentBlendInput.magnitude);
    }
}
