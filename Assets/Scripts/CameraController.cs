using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private Transform _moveableCameraAnchor;
    [SerializeField] private Transform _focusPoint;
    [SerializeField] private float _minXAngle;
    [SerializeField] private float _maxXAngle;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _positionLerpRate;
    [SerializeField] private float _cameraObstacleOffset;
    [SerializeField] private float _checkObstaclesTime = .2f;
    [SerializeField] private LayerMask _cameraRayIgnoreObjectsMask;

    private Quaternion _targetControllerRotation;
    private Quaternion _currentControllerRotation;
    private Transform _player;
    private Transform _currentCameraAnchor;
    private RaycastHit _hit;
    private float _debugHitSphereRadius = .2f;
    private float _checkObstaclesTimer = 0f;


    public void Initialize(Transform player)
    {
        _currentControllerRotation = transform.rotation;
        _currentCameraAnchor = _cameraAnchor;
        _player = player;
        gameObject.SetActive(true);
    }
    private void LateUpdate()
    {
        FollowPlayer();
        Rotate();
        CheckObstacles();
        MoveCameraToAnchor();
        RotateCamera();
    }
    private void OnEnable()
    {
        if (!InputManager.Instance) return;
        InputManager.Instance.OnLook.AddListener(HandleLook);
    }
    private void OnDisable()
    {
        if (!InputManager.Instance) return;
        InputManager.Instance.OnLook.RemoveListener(HandleLook);
    }
    private void HandleLook(InputAction.CallbackContext context)
    {
        Vector2 delta = context.ReadValue<Vector2>();
        Vector3 currentEuler = _currentControllerRotation.eulerAngles;

        float xRot = AngleClamp.ClampAngle(currentEuler.x - delta.y * _rotationSpeed, _minXAngle, _maxXAngle);
        float yRot = currentEuler.y + delta.x * _rotationSpeed;

        _targetControllerRotation = Quaternion.Euler(xRot, yRot, 0);
    }
    private void FollowPlayer()
    {
        transform.position = _player.position;
    }
    private void Rotate()
    {
        _currentControllerRotation = _targetControllerRotation;
        transform.rotation = _currentControllerRotation;
    }
    private void RotateCamera()
    {
        _camera.transform.rotation = Quaternion.LookRotation(_focusPoint.transform.position - _camera.transform.position);
    }
    private void MoveCameraToAnchor()
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _currentCameraAnchor.position, Time.deltaTime * _positionLerpRate);
    }
    private void CheckObstacles()
    {
        if (_checkObstaclesTimer < _checkObstaclesTime)
        {
            _checkObstaclesTimer += Time.deltaTime;
            return;
        }
        else
        {
            _checkObstaclesTimer = 0;
        }
        Vector3 dir = _cameraAnchor.transform.position - _focusPoint.transform.position;
        Debug.DrawRay(_focusPoint.transform.position, dir, Color.red, .1f);
        if (Physics.Raycast(_focusPoint.transform.position, dir, out _hit, dir.magnitude, ~_cameraRayIgnoreObjectsMask))
        {
            _moveableCameraAnchor.position = _hit.point - dir * _cameraObstacleOffset;
            _currentCameraAnchor = _moveableCameraAnchor;
        }
        else
        {
            _currentCameraAnchor = _cameraAnchor;
        }
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_hit.point, _debugHitSphereRadius);
    }
}
