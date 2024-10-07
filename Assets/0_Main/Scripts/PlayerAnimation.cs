using System.Reflection;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator anime;
    [SerializeField] private float LocomotionBlendSpeed = 0.2f;

    private PlayerInput playerInput;
    private PlayerState playerState;
    private Vector3 CurrentBlendInput = Vector3.zero;

    private static int InputXHash = Animator.StringToHash("InputX");
    private static int InputYHash = Animator.StringToHash("InputY");
    private static int InputMagnitudeHash = Animator.StringToHash("InputMagnitude");

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool IsSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

        Vector2 InputTarget = IsSprinting ? playerInput.Movement * 1.5f : playerInput.Movement;
        CurrentBlendInput = Vector3.Lerp(CurrentBlendInput, InputTarget, LocomotionBlendSpeed * Time.deltaTime);
  
        anime.SetFloat(InputXHash, CurrentBlendInput.x);
        anime.SetFloat (InputYHash, CurrentBlendInput.y);
        anime.SetFloat(InputMagnitudeHash,CurrentBlendInput.magnitude);
    }
}
