using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraMode CameraMode { get; private set; } = CameraMode.Default;

    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private Transform _moveableCameraAnchor;
    [SerializeField] private Transform _moveableAnchorFocusPoint;
    [SerializeField] private Transform _focusPoint;
    [SerializeField] private Transform _obstacleChecker;
    [SerializeField] private float _minXAngle;
    [SerializeField] private float _maxXAngle;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _positionLerpRate;
    [SerializeField] private float _positionLerpRateAimMode;
    [SerializeField] private float _cameraObstacleOffset;
    [SerializeField] private float _aimFocusPointDistance;
    [SerializeField] private float _aimFocusPointDistanceChangeDuration = .5f;
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
    private CancellationTokenSource _cancellationTokenSource;
    

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
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            CameraMode = CameraMode.Aim;
            _moveableCameraAnchor.localPosition = AimAnchorPos;
            _currentCameraAnchor = _moveableCameraAnchor;
            _currentFucusPoint = _moveableAnchorFocusPoint;
            _moveableAnchorFocusPoint.parent = _moveableCameraAnchor;
            _moveableAnchorFocusPoint.localPosition = new Vector3(0f, 0f, 10);
            _positionLerpRate = _positionLerpRateAimMode;
        }
        if (context.canceled)
        {
            CameraMode = CameraMode.Default;
            _currentCameraAnchor = _cameraAnchor;
            _moveableAnchorFocusPoint.parent = transform;
            _moveableAnchorFocusPoint.position = new Vector3(1f,0.5f, 10);
            //_currentFucusPoint = _focusPoint;
            _positionLerpRate = _defaultPositionLerpRate;
            _cancellationTokenSource = new CancellationTokenSource();
            FocusPointToDefault(_cancellationTokenSource.Token);
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
    private async void FocusPointToDefault(CancellationToken ct)
    {
        float expiredTime = 0f;
        float progress = 1;
        Vector3 newFocusPosition = _focusPoint.localPosition;
        while (progress > 0f)
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = 1 - expiredTime / _aimFocusPointDistanceChangeDuration;
            _moveableAnchorFocusPoint.localPosition = new Vector3(newFocusPosition.x,
                                                    newFocusPosition.y,
                                                    (newFocusPosition.z + _aimFocusPointDistance) * progress);
            await Task.Yield();
        }
        _currentFucusPoint = _focusPoint;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_hit.point, _debugHitSphereRadius);
    }
}
public enum CameraMode { Default, Aim }
