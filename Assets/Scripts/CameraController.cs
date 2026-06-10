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
    [SerializeField] private float _cameraYDamping = .5f;
    [SerializeField] private float _checkObstaclesTime = .2f;
    [SerializeField] private LayerMask _cameraRayIgnoreObjectsMask;
    [SerializeField] private Vector3 AimAnchorPos;

    private Quaternion _targetControllerRotation;
    private Vector2 _targetControllerRotationVector;
    private Quaternion _currentControllerRotation;
    private Transform _player;
    private Transform _currentCameraAnchor;
    private Transform _currentFucusPoint;
    private GroundDetector _groundDetector;
    private RaycastHit _hit;
    private float _debugHitSphereRadius = .2f;
    private float _aimModeLerpRateMultiplier = 2f;
    private float _checkObstaclesTimer = 0f;
    private float _defaultPositionLerpRate;
    private float _currentCameraY;
    private bool _invertCameraRotation = false;
    private CancellationTokenSource _aimRoutineCTS;
    

    public void Initialize(Transform player, GroundDetector groundDetector)
    {
        _currentCameraY = _camera.transform.position.y;
        _defaultPositionLerpRate = _positionLerpRate;
        _currentFucusPoint = _focusPoint;
        _currentControllerRotation = transform.rotation;
        _currentCameraAnchor = _cameraAnchor;
        _player = player;
        _groundDetector = groundDetector;
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
            _aimRoutineCTS?.Cancel();
            _aimRoutineCTS?.Dispose();

            CameraMode = CameraMode.Aim;
            _moveableCameraAnchor.localPosition = AimAnchorPos;
            _currentCameraAnchor = _moveableCameraAnchor;
            _positionLerpRate = _positionLerpRateAimMode;

            _aimRoutineCTS = new CancellationTokenSource();
            FocusPointToAim(_aimRoutineCTS.Token);
        }
        if (context.canceled)
        {
            _aimRoutineCTS?.Cancel();
            _aimRoutineCTS?.Dispose();

            CameraMode = CameraMode.Default;
            _currentCameraAnchor = _cameraAnchor;
            _positionLerpRate = _defaultPositionLerpRate;

            _aimRoutineCTS = new CancellationTokenSource();
            FocusPointToDefault(_aimRoutineCTS.Token);
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
        /*        if (!_groundDetector.Grounded && (transform.position.y - _camera.transform.position.y) < _cameraYDamping)
                {
                    Vector3 newPosition = new Vector3(_currentCameraAnchor.position.x, _camera.transform.position.y, _currentCameraAnchor.position.z);
                    _camera.transform.position = Vector3.Lerp(_camera.transform.position, newPosition, Time.deltaTime * _positionLerpRate);
                    return;
                }
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, _currentCameraAnchor.position, Time.deltaTime * _positionLerpRate);*/

        float anchorY = _currentCameraAnchor.position.y;

        // Dead zone: если игрок недалеко ушёл по Y — не двигаем камеру вертикально
        if (_groundDetector.Grounded || Mathf.Abs(transform.position.y - _currentCameraY) > _cameraYDamping)
        {
            _currentCameraY = _currentCameraAnchor.position.y;
        }
        //_currentCameraY = Mathf.Lerp(_currentCameraY, _currentCameraAnchor.position.y, Time.deltaTime * _positionLerpRate);

        Vector3 target = new Vector3(
            _currentCameraAnchor.position.x,
            _currentCameraY,
            _currentCameraAnchor.position.z
        );

        _camera.transform.position = Vector3.Lerp(_camera.transform.position, target, Time.deltaTime * _positionLerpRate);

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
        _moveableAnchorFocusPoint.parent = transform;
        _moveableAnchorFocusPoint.position = new Vector3(AimAnchorPos.x, AimAnchorPos.y, _aimFocusPointDistance);
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
    private async void FocusPointToAim(CancellationToken ct)
    {
        _currentFucusPoint = _moveableAnchorFocusPoint;
        _moveableAnchorFocusPoint.parent = _moveableCameraAnchor;
        _moveableAnchorFocusPoint.localPosition = Vector3.zero;
        float expiredTime = 0f;
        float progress = 0;
        float middlePoint = _aimFocusPointDistance / 2;
        Vector3 newFocusPosition = new Vector3(0, 0, _aimFocusPointDistance);
        while (progress < 1f)
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = expiredTime / _aimFocusPointDistanceChangeDuration;
            _moveableAnchorFocusPoint.localPosition = new Vector3(newFocusPosition.x,
                                                    newFocusPosition.y,
                                                    middlePoint + middlePoint * progress);
            await Task.Yield();
        }
        _positionLerpRate *= _aimModeLerpRateMultiplier;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_hit.point, _debugHitSphereRadius);
    }
}
public enum CameraMode { Default, Aim }
