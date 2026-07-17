using UnityEngine;

public class SFXActor_StepLanding : SFXActor
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private GroundDetector _groundDetector;
    protected override void Awake()
    {
        base.Awake();

    }
    private void OnEnable()
    {
        _playerMovement.PlayerJumped += OnPlayerJumped;
        _groundDetector.GroundedPositive += OnGrounded;
    }
    private void OnDisable()
    {
        _playerMovement.PlayerJumped -= OnPlayerJumped;
        _groundDetector.GroundedPositive -= OnGrounded;
    }
    private void OnPlayerJumped()
    {
        _source.pitch = 1f;
        PlaySound(SFX.Jump);
    }
    private void OnGrounded()
    {
        PlaySound(SFX.FootstepGrass);
    }
}
