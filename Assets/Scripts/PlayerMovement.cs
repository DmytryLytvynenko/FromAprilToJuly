using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(NormalComparer))] 
public class PlayerMovement : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; set; }

    public event Action PlayerJumped;

    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _gravity;
    [SerializeField] private float _maxElevationAngle;
    [SerializeField] private LayerMask _groundedLayers;
    
    private GroundDetector _groundedDetector;
    private NormalComparer _normalComparer;
    private Rigidbody _rigidbody;
    private Camera _camera;
    public void Initialize(Camera camera, GroundDetector groundDetector, NormalComparer normalComparer)
    {
        _camera = camera;
        _rigidbody = GetComponent<Rigidbody>();
        _groundedDetector = groundDetector;
        _normalComparer = normalComparer;
        InputManager.Instance.OnJump.AddListener(HandleJump);
    }
    private void FixedUpdate()
    {
        HandleMove();
    }

    private void RotateTowardsMoveDirection(Vector3 moveDirection)
    {
        if (moveDirection.magnitude < 0.01f) return; 

        float currentAngle = transform.eulerAngles.y;
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, _rotationSpeed * Time.fixedDeltaTime);

        transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
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

        Vector3 moveDirection = (forward * input.y + right * input.x).normalized;
        moveDirection = _normalComparer.Project(moveDirection);

        Vector3 currentVelocity = _rigidbody.linearVelocity;
        Vector3 desiredVelocity = moveDirection * Speed;
        desiredVelocity.y = currentVelocity.y;
        _rigidbody.linearVelocity = desiredVelocity;
        HandleGravity();
        RotateTowardsMoveDirection(moveDirection);
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
