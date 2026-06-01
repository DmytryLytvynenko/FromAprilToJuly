using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; set; }

    public event Action PlayerJumped;

    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _gravity;
    [SerializeField] private float _maxVelocity = 5f;
    [SerializeField] private LayerMask _groundedLayers;
    [SerializeField] private Transform _visual;
    
    private GroundDetector _groundedDetector;
    private NormalProjector _normalProjector;
    private Camera _camera;
    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    public void Initialize(Camera camera, GroundDetector groundDetector, NormalProjector normalProjector, Rigidbody rigidbody)
    {
        _camera = camera;
        _groundedDetector = groundDetector;
        _normalProjector = normalProjector;
        _rigidbody = rigidbody;
        InputManager.Instance.OnJump.AddListener(HandleJump);
        InputManager.Instance.OnMove.AddListener(HandleMoveInput);
    }
    public void HandleDisable()
    {
        InputManager.Instance.OnJump.RemoveListener(HandleJump);
        InputManager.Instance.OnMove.RemoveListener(HandleMoveInput);
    }
    private void FixedUpdate()
    {
        HandleMove();
    }
    public void RotateTowardsMoveDirection()
    {
        if (_moveDirection.magnitude < 0.01f) return; 

        float currentAngle = _visual.eulerAngles.y;
        float targetAngle = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg;
        float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, _rotationSpeed * Time.deltaTime);

        //_rigidbody.MoveRotation(Quaternion.Euler(0f, smoothedAngle, 0f));
        _visual.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
    }
    private void HandleMove()
    {
        if (_camera == null) { Debug.LogWarning("PlayerMovement camera ref is null"); return; };

        Vector2 input = InputManager.Instance.CurrentMoveInput;

        Vector3 forward = _camera.transform.forward;
        Vector3 right = _camera.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        _moveDirection = (forward * input.y + right * input.x).normalized;
        //RotateTowardsMoveDirection();
        _moveDirection = _normalProjector.Project(_moveDirection);

        Vector3 currentVelocity = _rigidbody.linearVelocity;
        Vector3 desiredVelocity = _moveDirection * Speed;
        desiredVelocity.y = currentVelocity.y;
        _rigidbody.linearVelocity = desiredVelocity;
        HandleGravity();
    }
    private void HandleMoveInput(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled && _groundedDetector.Grounded)
        {
            _rigidbody.linearVelocity = new Vector3();
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (_groundedDetector.Grounded)
        {
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            PlayerJumped?.Invoke();
        }
    }
    private void HandleGravity()
    {
        float currentVerticalSpeed = _rigidbody.linearVelocity.y;

        currentVerticalSpeed -= _gravity * Time.fixedDeltaTime;

        _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, currentVerticalSpeed, _rigidbody.linearVelocity.z);
    }
}
