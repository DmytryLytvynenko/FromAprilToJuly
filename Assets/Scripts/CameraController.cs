using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraMode CameraMode { get; private set; } = CameraMode.Default;

    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private Transform _moveableCameraAnchor;
    [SerializeField] private Transform _focusPoint;
    [SerializeField] private Transform _moveableAnchorFocusPoint;
    [SerializeField] private Transform _obstacleChecker;
    [SerializeField] private float _minXAngle;
    [SerializeField] private float _maxXAngle;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _positionLerpRate;
    [SerializeField] private float _positionLerpRateAimMode;
    [SerializeField] private float _cameraObstacleOffset;
    [SerializeField] private float _checkObstaclesTime = .2f;
    [SerializeField] private LayerMask _cameraRayIgnoreObjectsMask;
    [SerializeField] private Vector3 AimAnchorPos;

    private Quaternion _targetControllerRotation;
    private Vector2 _targetControllerRotationVector;
    private Quaternion _currentControllerRotation;
    private Transform _player;
    private Transform _currentCameraAnchor;
    private Transform _currentFucusPoint;
    private RaycastHit _hit;
    private float _debugHitSphereRadius = .2f;
    private float _checkObstaclesTimer = 0f;
    private float _defaultPositionLerpRate;
    private bool _invertCameraRotation = false;
    

    public void Initialize(Transform player)
    {
        _defaultPositionLerpRate = _positionLerpRate;
        _currentFucusPoint = _focusPoint;
        _currentControllerRotation = transform.rotation;
        _currentCameraAnchor = _cameraAnchor;
        _player = player;
        gameObject.SetActive(true);
        InputManager.Instance.OnLook.AddListener(HandleLook);
        InputManager.Instance.OnAim.AddListener(HandleAim);
    }
    public void HandleDisable()
    {
        InputManager.Instance.OnLook.RemoveListener(HandleLook);
        InputManager.Instance.OnAim.RemoveListener(HandleAim);
    }
    private void LateUpdate()
    {
        FollowPlayer();
        Rotate();
        CheckObstacles();
        MoveCameraToAnchor();
        RotateCamera();
        //_playerMovement.RotateTowardsMoveDirection();
    }
    private void HandleLook(InputAction.CallbackContext context)
    {
        Vector2 delta = context.ReadValue<Vector2>();
        Vector3 currentEuler = _currentControllerRotation.eulerAngles;

        sbyte inverceInput = (sbyte)(_invertCameraRotation ? -1 : 1);
        float xRot = AngleClamp.ClampAngle(currentEuler.x - delta.y * inverceInput * _rotationSpeed, _minXAngle, _maxXAngle);
        float yRot = currentEuler.y + delta.x * inverceInput * _rotationSpeed;

        _targetControllerRotationVector = new Vector2(xRot, yRot);
        _targetControllerRotation = Quaternion.Euler(xRot, yRot, 0);
    }
    private void HandleAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CameraMode = CameraMode.Aim;
            //_invertCameraRotation = true;
            _moveableCameraAnchor.localPosition = AimAnchorPos;
            _currentCameraAnchor = _moveableCameraAnchor;
            _currentFucusPoint = _moveableAnchorFocusPoint;
            _positionLerpRate = _positionLerpRateAimMode;
        }
        if (context.canceled)
        {
            CameraMode = CameraMode.Default;
            //_invertCameraRotation = false;
            _currentCameraAnchor = _cameraAnchor;
            _currentFucusPoint = _focusPoint;
            _positionLerpRate = _defaultPositionLerpRate;
        }
    }
    private void FollowPlayer()
    {
        transform.position = _player.position;
    }
    private void Rotate()
    {
        if (CameraMode == CameraMode.Default)
        {
            _currentControllerRotation = _targetControllerRotation;
            transform.rotation = _currentControllerRotation;
        }
        else
        {
            _currentControllerRotation = _targetControllerRotation;
            transform.rotation = Quaternion.Euler(0, _targetControllerRotationVector.y, 0);
            _moveableCameraAnchor.rotation = Quaternion.Euler(_targetControllerRotationVector.x, _targetControllerRotationVector.y, 0);
        }
    }
    private void RotateCamera()
    {
        _camera.transform.rotation = Quaternion.LookRotation(_currentFucusPoint.transform.position - _camera.transform.position);
    }
    private void MoveCameraToAnchor()
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _currentCameraAnchor.position, Time.deltaTime * _positionLerpRate);
    }
    private void CheckObstacles()
    {
        if (CameraMode == CameraMode.Aim) return;

        if (_checkObstaclesTimer < _checkObstaclesTime)
        {
            _checkObstaclesTimer += Time.deltaTime;
            return;
        }

        _checkObstaclesTimer = 0;
        Vector3 dir = _cameraAnchor.transform.position - _obstacleChecker.transform.position;
        Debug.DrawRay(_obstacleChecker.transform.position, dir, Color.red, .1f);
        if (Physics.Raycast(_obstacleChecker.transform.position, dir, out _hit, dir.magnitude, ~_cameraRayIgnoreObjectsMask))
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
public enum CameraMode { Default, Aim }
