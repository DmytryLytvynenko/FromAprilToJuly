using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    public event Action ItemPicked;
    public event Action ChargeTimerStarted;
    public event Action ItemThrown;

    [Header("Interact")]
    [SerializeField] protected Transform _followPoint;
    [SerializeField] protected float _pickUpDistance = 4f;
    [SerializeField] protected float _followSpeed = 10f;
    [SerializeField] protected float _throwForce = 50f;
    [SerializeField] protected LayerMask _pickUpObjects;
    [SerializeField] protected float _rotateStep = 45f;
    [SerializeField] protected float _moveFollowPointStep = .5f;
    [SerializeField] protected float _followPointAimPositionZ = 3.75f;
    [SerializeField] protected Vector2 _followPointClamp;
    [SerializeField] protected float _interactScanRate = .2f;
    [Header("Charge")]
    [SerializeField] protected float _throwChargeTime = 3f;
    [SerializeField] protected float _startChargeTime = .5f;
    [Header("Aditional")]
    [SerializeField] protected float _maxCarryWeight = 200f;
    [SerializeField] protected float _minimalMultiplierValue = .05f;

    protected Camera _camera;
    protected Transform _cameraTransform;
    protected Item _currentItem = null;
    protected Item _currentHighlightedItem = null;
    protected Vector2 _currentFollowPointClamp;
    protected Vector3 _followPointDefaultPosition;
    protected bool _rotateItemOnScroll = true;
    protected bool _justPickedUp = true;
    protected bool _chargeForThrow = false;
    protected float _interactScanTimer;
    protected float _throwTimer = 0f;
    protected float _rawChargeTime;
    protected virtual void Update()
    {
        ScanForItem();
        ChargeForThrow();
    }
    public virtual void Initialize(Camera camera)
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
    public virtual void HandleDisable()
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
    protected virtual void HandleInteract(InputAction.CallbackContext ctx)
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
    protected float CalculateFollowSpeed()
    {
        float maxCoefficient = 0.001f * _maxCarryWeight * _maxCarryWeight;
        float currentCoefficient = 0.001f * _currentItem.Mass * _currentItem.Mass;
        float weightRatio = currentCoefficient / maxCoefficient;
        float multiplier = 1f - weightRatio;
        multiplier = Mathf.Clamp(multiplier, _minimalMultiplierValue, 1f);
        return multiplier * _followSpeed;
    }
    protected virtual void ScanForItem()
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
    protected void ChargeForThrow()
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
    protected void HandleRotateHorizontal(InputAction.CallbackContext ctx)
    {
        sbyte sign = (sbyte)Mathf.Sign(ctx.ReadValue<float>());
        if (_currentItem)
        {
            _currentItem.ChangeTargetRotation(0, _rotateStep * sign);
        }
    }
    protected void HandleScroll(InputAction.CallbackContext ctx)
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
    protected void HandleStabilizeRotation(InputAction.CallbackContext ctx)
    {
        if (_currentItem)
        {
            _currentItem.Stabilize();
        }
    }
    protected void HandleToggleScroll(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) _rotateItemOnScroll = false;
        else _rotateItemOnScroll = true;
    }
    protected void OnCameraAimModeEntered()
    {
        _followPoint.localPosition = new Vector3(_followPoint.localPosition.x, _followPoint.localPosition.y, _followPoint.localPosition.z - _followPointAimPositionZ);
        _currentFollowPointClamp = _followPointClamp + Vector2.one * (_followPointDefaultPosition.z - _followPointAimPositionZ);
    }
    protected void OnCameraDefaultModeEntered()
    {
        _followPoint.localPosition = new Vector3(_followPoint.localPosition.x, _followPoint.localPosition.y, _followPoint.localPosition.z + _followPointAimPositionZ);
        _currentFollowPointClamp = _followPointClamp + Vector2.one * _followPointDefaultPosition.z;
    }
    protected virtual void MoveFollowPoint(float step)
    {
        float newPos = _followPoint.localPosition.z + step;
        newPos = Mathf.Clamp(newPos, _currentFollowPointClamp.x, _currentFollowPointClamp.y);
        _followPoint.localPosition = new Vector3(0, 0, newPos);
    }
    protected void ItemPickedEventForwarder()
    {
        ItemPicked?.Invoke();
    }
    protected void ItemThrownForwarder()
    {
        ItemThrown?.Invoke();
    }
    protected void ChargeTimerStartedEventForwarder()
    {
        ChargeTimerStarted?.Invoke();
    }
}
