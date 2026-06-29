using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PlayerMovement : MonoBehaviour
{
    [field: SerializeField] public float Speed { get; set; }

    public event Action PlayerJumped;

    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _gravity;
    [SerializeField] private float _horizontalDamping = 0.3f;
    [SerializeField] private float _slopeHorizontalDamping = 0.9f;
    [SerializeField] private LayerMask _groundedLayers;
    [SerializeField] private Transform _visual;
    [SerializeField] private PhysicsMaterial _materialHighFriction;
    [SerializeField] private PhysicsMaterial _materialZeroFriction;
    [SerializeField] private Collider _collider;
    
    private GroundDetector _groundedDetector;
    private NormalProjector _normalProjector;
    private Camera _camera;
    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    private Vector3 _lastMoveDirection;
    private CancellationTokenSource _source;
    public void Initialize(Camera camera, GroundDetector groundDetector, NormalProjector normalProjector, Rigidbody rigidbody)
    {
/*        _collider.material = _materialZeroFriction;
        _collider.material = _materialHighFriction; 
        _collider.material = _materialZeroFriction;*/
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
    private void LateUpdate()
    {
        RotateTowardsMoveDirection();
    }
    public void RotateTowardsMoveDirection()
    {

        float currentAngle = _visual.eulerAngles.y;
        float targetAngle;
        float smoothedAngle;

        if (CameraController.CameraMode == CameraMode.Aim)
        {
             targetAngle = _camera.transform.eulerAngles.y;
             smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, _rotationSpeed * Time.deltaTime);
        }
        else
        {
            if (_lastMoveDirection.magnitude < 0.01f) return;
            targetAngle = Mathf.Atan2(_lastMoveDirection.x, _lastMoveDirection.z) * Mathf.Rad2Deg;
            smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, _rotationSpeed * Time.deltaTime);
        }

        _visual.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
    }

    private void HandleMove()
    {
        if (_camera == null) { Debug.LogWarning("PlayerMovement camera ref is null"); return; };

        Vector2 input = InputManager.Instance.CurrentMoveInput;

        if (input.magnitude == 0f)
        {
            Vector3 desiredVelocity = _rigidbody.linearVelocity * _horizontalDamping;
            desiredVelocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = desiredVelocity;

            HandleGravity();

            return;
        }

        Vector3 forward = _camera.transform.forward;
        Vector3 right = _camera.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        _moveDirection = (forward * input.y + right * input.x).normalized;
        _lastMoveDirection = _moveDirection;
        _moveDirection = _normalProjector.Project(_moveDirection, out bool canMove);

        if (canMove)
        {
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            Vector3 desiredVelocity = _moveDirection * Speed;
            desiredVelocity.y = currentVelocity.y;
            _rigidbody.linearVelocity = desiredVelocity;
        }
        else
        {
            float speed = new Vector2(_rigidbody.linearVelocity.x, _rigidbody.linearVelocity.z).magnitude;
            Vector3 desiredVelocity = new Vector3(_lastMoveDirection.x,0,_lastMoveDirection.z) * speed * _slopeHorizontalDamping;
            desiredVelocity.y = _rigidbody.linearVelocity.y;
            _rigidbody.linearVelocity = desiredVelocity;
        }
        HandleGravity();
    }
    private void HandleMoveInput(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
        {
            _source?.Cancel();
            _source?.Dispose();

            _source = new CancellationTokenSource();
            WaitThenChangePhysicMaterial(_source.Token).Forget();
        }
        if (ctx.performed)
        {
            _source?.Cancel();
            _source?.Dispose();
            _source = null;
            _collider.material = _materialZeroFriction;
        }
    }
    private async UniTaskVoid WaitThenChangePhysicMaterial(CancellationToken ct)
    {
        await UniTask.Delay(300, cancellationToken: ct);
        _collider.material = _materialHighFriction;
    }
    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (_groundedDetector.Grounded)
        {
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
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
