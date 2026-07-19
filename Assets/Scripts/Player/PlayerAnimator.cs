using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private PlayerMovement _playerMovement;
    private GroundDetector _groundDetector;
    public void Initialize(PlayerMovement playerMovement, GroundDetector groundDetector)
    {
        _playerMovement = playerMovement;
        _groundDetector = groundDetector;

        _playerMovement.PlayerJumped += OnPlayerJumped;
        _playerMovement.MovementStarted += OnMovementStarted;
        _playerMovement.MovementEnded += OnMovementEnded;

        _groundDetector.GroundedChanged += OnGroundedChanged;
    }
    public void HandleDisable()
    {
        _playerMovement.PlayerJumped -= OnPlayerJumped;
        _playerMovement.MovementStarted -= OnMovementStarted;
        _playerMovement.MovementEnded -= OnMovementEnded;

        _groundDetector.GroundedChanged -= OnGroundedChanged;
    }
    private void OnPlayerJumped() 
    {
        _animator.SetTrigger(PlayerAnimatorParams.Jump.ToString());
    }
    private void OnMovementStarted() 
    {
        _animator.SetBool(PlayerAnimatorParams.Run.ToString(), true);
    }
    private void OnMovementEnded() 
    {
        _animator.SetBool(PlayerAnimatorParams.Run.ToString(), false);
    }
    private void OnGroundedChanged(bool value) 
    {
        _animator.SetBool(PlayerAnimatorParams.Grounded.ToString(), value);
    }
}
public enum PlayerAnimatorParams
{
    Run,
    Jump,
    Grounded
}
public enum AnimatorParams
{
    SceneLoaded
}

