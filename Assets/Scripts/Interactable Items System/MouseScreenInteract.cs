using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseScreenInteract : Interact
{
    [SerializeField] private float _distanceFromCamera = 5f;
    private Vector2 _mousePos;
    protected override void Update()
    {
        base.Update();
        SetFollowPointPos();
    }
    public override void Initialize(Camera camera)
    {
        base.Initialize(camera);
        InputManager.Instance.OnMousePosition.AddListener(HandleMousePosition);
    }
    public override void HandleDisable()
    {
        base.HandleDisable();
        InputManager.Instance.OnMousePosition.RemoveListener(HandleMousePosition);
    }
    protected override void HandleInteract(InputAction.CallbackContext ctx)
    {
        Ray ray = _camera.ScreenPointToRay(_mousePos);
        if (ctx.performed)
        {
            if (_currentItem)
            {
                _chargeForThrow = true;
                _justPickedUp = false;
                ChargeTimerStartedEventForwarder();
                return;
            }
            if (Physics.Raycast(ray, out RaycastHit hitInfo, _pickUpDistance, _pickUpObjects))
            {
                if (hitInfo.rigidbody.TryGetComponent(out Item item))
                {
                    _currentItem = item;
                    _justPickedUp = true;
                    item.PickUp(_followPoint, CalculateFollowSpeed());
                    PlayPickupSound();
                    ItemPickedEventForwarder();
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
                    _currentItem.Release((_throwTimer - _startChargeTime) / _rawChargeTime * _throwForce * ray.direction.normalized);               
                    _throwTimer = 0;
                    _chargeForThrow = false;
                    _currentItem = null;
                    PlayReleaseSound();
                    ItemThrownForwarder();
                }
            }
        }
    }
    protected override void ScanForItem()
    {
        if (_currentItem) return;
        _interactScanTimer += Time.deltaTime;
        if (_interactScanTimer < _interactScanRate) return;

        _interactScanTimer = 0;
        Ray ray = _camera.ScreenPointToRay(_mousePos);
        ScenForItemDebugRay(ray);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, _pickUpDistance, _pickUpObjects))
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

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private static void ScenForItemDebugRay(Ray ray)
    {
        UnityEngine.Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, .2f);
    }

    private void HandleMousePosition(InputAction.CallbackContext ctx)
    {
        _mousePos = ctx.ReadValue<Vector2>();
    }
    protected override void MoveFollowPoint(float step)
    {
        float newPos = _distanceFromCamera + step;
        _distanceFromCamera = Mathf.Clamp(newPos, _followPointClamp.x, _followPointClamp.y);
    }
    private void SetFollowPointPos()
    {
        Ray ray = _camera.ScreenPointToRay(_mousePos);
        _followPoint.position = ray.GetPoint(_distanceFromCamera);
    }
}
