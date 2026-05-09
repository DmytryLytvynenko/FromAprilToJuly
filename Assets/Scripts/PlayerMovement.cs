using UnityEngine;
using SimpleDependencyManagement;

[RequireComponent(typeof(Rigidbody))] 
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationSpeed;
    private Rigidbody _rigidbody;
    private Camera _camera;
    public void Initialize(Camera camera)
    {
        _camera = camera;
        _rigidbody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (_camera == null) return;

        Vector2 input = InputManager.Instance.CurrentMoveInput;

        Vector3 forward = _camera.transform.forward;
        Vector3 right = _camera.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * input.y + right * input.x).normalized;

        Vector3 currentVelocity = _rigidbody.linearVelocity;
        Vector3 desiredVelocity = moveDirection * _speed;
        desiredVelocity.y = currentVelocity.y;
        _rigidbody.linearVelocity = desiredVelocity;
        RotateTowardsMoveDirection(moveDirection);
    }

    private void RotateTowardsMoveDirection(Vector3 moveDirection)
    {
        if (moveDirection.magnitude < 0.01f) return; 

        float currentAngle = transform.eulerAngles.y;
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        float smoothedAngle = Mathf.LerpAngle(currentAngle, targetAngle, _rotationSpeed * Time.fixedDeltaTime);

        transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
    }
}
