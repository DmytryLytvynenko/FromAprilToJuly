using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    [SerializeField] private float _pickUpDistance = 4f;
    [SerializeField] private float _pickUpForce = 10f;
    [SerializeField] private LayerMask _pickUpObjects;
    [SerializeField] private Transform _followPoint;
    [SerializeField] private float _interactScanRate = .2f;

    private Camera _camera;
    private Transform _cameraTransform;
    private Item _currentItem = null;
    private Item _currentHighlightedItem = null;
    private float _interactScanTimer;
    private void Update()
    {
        ScanForItem();
    }
    public void Initialize(Camera camera)
    {
        _camera = camera;
        _cameraTransform = camera.transform;
        InputManager.Instance.OnInteract.AddListener(HandleInteract);
        InputManager.Instance.OnRotateRight.AddListener(HandleRotateRight);
        InputManager.Instance.OnRotateLeft.AddListener(HandleRotateLeft);
    }
    public void HandleDisable()
    {
        InputManager.Instance.OnInteract.RemoveListener(HandleInteract);
        InputManager.Instance.OnRotateRight.RemoveListener(HandleRotateRight);
        InputManager.Instance.OnRotateLeft.RemoveListener(HandleRotateLeft);
    }
    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (_currentItem)
        {
            _currentItem.Release();
            _currentItem = null;
        }
        else
        {
            Vector3 dir = _camera.transform.forward;
            Debug.DrawRay(_cameraTransform.position, dir * _pickUpDistance, Color.yellow, 5f);
            if (Physics.Raycast(_cameraTransform.position, dir,out RaycastHit hitInfo, _pickUpDistance, _pickUpObjects))
            {
                if (hitInfo.rigidbody.TryGetComponent(out Item item))
                {
                    item.PickUp(_followPoint, _pickUpForce);
                    _currentItem = item;
                }
            }
        }
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
    private void HandleRotateRight(InputAction.CallbackContext ctx)
    {
        if (_currentItem)
        {
            _currentItem.AdjustTargetRotation(45, 0);
        }
    }
    private void HandleRotateLeft(InputAction.CallbackContext ctx)
    {

    }
}
