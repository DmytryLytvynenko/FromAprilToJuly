using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    public event Action ItemPicked;
    public event Action ChargeTimerStarted;
    public event Action ItemThrown;

    [Header("Interact")]
    [SerializeField] private Transform _followPoint;
    [SerializeField] private float _pickUpDistance = 4f;
    [SerializeField] private float _followSpeed = 10f;
    [SerializeField] private float _throwForce = 50f;
    [SerializeField] private LayerMask _pickUpObjects;
    [SerializeField] private float _rotateStep = 45f;
    [SerializeField] private float _moveFollowPointStep = .5f;
    [SerializeField] private float _followPointAimPositionZ = 3.75f;
    [SerializeField] private Vector2 _followPointClamp;
    [SerializeField] private float _interactScanRate = .2f;
    [Header("Charge")]
    [SerializeField] private float _throwChargeTime = 3f;
    [SerializeField] private float _startChargeTime = .5f;
    [Header("Aditional")]
    [SerializeField] private float _maxCarryWeight = 200f;
    [SerializeField] private float _minimalMultiplierValue = .05f;

    private Camera _camera;
    private Transform _cameraTransform;
    private Item _currentItem = null;
    private Item _currentHighlightedItem = null;
    private Vector2 _currentFollowPointClamp;
    private Vector3 _followPointDefaultPosition;
    private bool _rotateItemOnScroll = true;
    private bool _justPickedUp = true;
    private bool _chargeForThrow = false;
    private float _interactScanTimer;
    private float _throwTimer = 0f;
    private float _rawChargeTime;
    private void Update()
    {
        ScanForItem();
        ChargeForThrow();
    }
    public void Initialize(Camera camera)
    {
        _rawChargeTime = _throwChargeTime - _startChargeTime;
        _currentFollowPointClamp = _followPointClamp + Vector2.one * _followPoint.localPosition.z;
        _followPointDefaultPosition = _followPoint.localPosition;
        _camera = camera;
        _cameraTransform = camera.transform;
        InputManager.Instance.OnInteract.AddListener(HandleInteract);
        InputManager.Instance.OnRotateHorizontal.AddListener(HandleRotateHorizontal);
        InputManager.Instance.OnScroll.AddListener(HandleScroll);
        InputManager.Instance.OnStabilizeRotation.AddListener(HandleStabilizeRotation);
        InputManager.Instance.OnToggleScroll.AddListener(HandleToggleScroll);
        CameraController.AimModeEntered += OnCameraAimModeEntered;
        CameraController.DefaultModeEntered += OnCameraDefaultModeEntered;
    }
    public void HandleDisable()
    {
        InputManager.Instance.OnInteract.RemoveListener(HandleInteract);
        InputManager.Instance.OnRotateHorizontal.RemoveListener(HandleRotateHorizontal);
        InputManager.Instance.OnScroll.RemoveListener(HandleScroll);
        InputManager.Instance.OnStabilizeRotation.RemoveListener(HandleStabilizeRotation);
        InputManager.Instance.OnToggleScroll.RemoveListener(HandleToggleScroll);
        CameraController.AimModeEntered -= OnCameraAimModeEntered;
        CameraController.DefaultModeEntered -= OnCameraDefaultModeEntered;
    }
    public float GetThrowChargePercentage()
    {
        return Mathf.Clamp01((_throwTimer - _startChargeTime) / _rawChargeTime);
    }
    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        Vector3 dir = _camera.transform.forward;
        if (ctx.performed)
        {
            if (_currentItem) 
            {
                _chargeForThrow = true;
                _justPickedUp = false;
                ChargeTimerStarted?.Invoke();
                return;
            }

            Debug.DrawRay(_cameraTransform.position, dir * _pickUpDistance, Color.yellow, 5f);
            if (Physics.Raycast(_cameraTransform.position, dir, out RaycastHit hitInfo, _pickUpDistance, _pickUpObjects))
            {
                if (hitInfo.rigidbody.TryGetComponent(out Item item))
                {
                    _currentItem = item;
                    _justPickedUp = true;
                    item.PickUp(_followPoint, CalculateFollowSpeed());
                    ItemPicked?.Invoke();
                }
            }
        }
        else
        {
            if (!_justPickedUp)
            {
                if (_currentItem)
                {
                    _throwTimer = _throwTimer > _startChargeTime ? _throwTimer : _startChargeTime;
                    _currentItem.Release((_throwTimer - _startChargeTime) / _rawChargeTime * _throwForce * dir);
                    Debug.Log("Throwed with force:" + (_throwTimer - _startChargeTime) / _rawChargeTime * _throwForce);
                    _throwTimer = 0;
                    _chargeForThrow = false;
                    _currentItem = null;
                    ItemThrown?.Invoke();
                }
            }
        }
    }
    private float CalculateFollowSpeed()
    {
        float maxCoefficient = 0.001f * _maxCarryWeight * _maxCarryWeight;
        float currentCoefficient = 0.001f * _currentItem.Mass * _currentItem.Mass;
        float weightRatio = currentCoefficient / maxCoefficient;
        float multiplier = 1f - weightRatio;
        multiplier = Mathf.Clamp(multiplier, _minimalMultiplierValue, 1f);
        return multiplier * _followSpeed;
    }
    private void ScanForItem()
    {
        if (_currentItem) return;
        _interactScanTimer += Time.deltaTime;
        if (_interactScanTimer < _interactScanRate) return;


        _interactScanTimer = 0;
        Vector3 dir = _camera.transform.forward;
        if (Physics.Raycast(_cameraTransform.position, dir, out RaycastHit hitInfo, _pickUpDistance, _pickUpObjects))
        {
            if (hitInfo.rigidbody.TryGetComponent(out Item item))
            {
                if (item == _currentHighlightedItem) return;

                _currentHighlightedItem?.RemoveHighlight();
                item.Highlight();
                _currentHighlightedItem = item;
            }
            else
            {
                _currentHighlightedItem?.RemoveHighlight();
                _currentHighlightedItem = null;
            }
        }
        else
        {
            _currentHighlightedItem?.RemoveHighlight();
            _currentHighlightedItem = null;
        }

    }
    private void ChargeForThrow()
    {
        if (!_chargeForThrow) return;

        if (_throwTimer < _throwChargeTime)
        {
            _throwTimer += Time.deltaTime;
        }
        else
        {
            _throwTimer = _throwChargeTime;
            _chargeForThrow = false;
        }

    }
    private void HandleRotateHorizontal(InputAction.CallbackContext ctx)
    {
        sbyte sign = (sbyte)Mathf.Sign(ctx.ReadValue<float>());
        if (_currentItem)
        {
            _currentItem.ChangeTargetRotation(0, _rotateStep * sign);
        }
    }
    private void HandleScroll(InputAction.CallbackContext ctx)
    {
        if (!_currentItem) return;

        sbyte sign = (sbyte)Mathf.Sign(ctx.ReadValue<float>());
        if (_rotateItemOnScroll)
        {
            _currentItem.ChangeTargetRotation(_rotateStep * sign, 0);
        }
        else
        {
            MoveFollowPoint(sign * _moveFollowPointStep);
        }
    }
    private void HandleStabilizeRotation(InputAction.CallbackContext ctx)
    {
        if (_currentItem)
        {
            _currentItem.Stabilize();
        }
    }
    private void HandleToggleScroll(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) _rotateItemOnScroll = false;
        else _rotateItemOnScroll = true;
    }
    private void OnCameraAimModeEntered()
    {
        _followPoint.localPosition = new Vector3(_followPoint.localPosition.x, _followPoint.localPosition.y, _followPoint.localPosition.z - _followPointAimPositionZ);
        _currentFollowPointClamp = _followPointClamp + Vector2.one * (_followPointDefaultPosition.z - _followPointAimPositionZ);
    }
    private void OnCameraDefaultModeEntered()
    {
        _followPoint.localPosition = new Vector3(_followPoint.localPosition.x, _followPoint.localPosition.y, _followPoint.localPosition.z + _followPointAimPositionZ);
        _currentFollowPointClamp = _followPointClamp + Vector2.one * _followPointDefaultPosition.z;
    }
    private void MoveFollowPoint(float step)
    {
        float newPos = _followPoint.localPosition.z + step;
        newPos = Mathf.Clamp(newPos, _currentFollowPointClamp.x, _currentFollowPointClamp.y);
        _followPoint.localPosition = new Vector3(0, 0, newPos);
    }
}
