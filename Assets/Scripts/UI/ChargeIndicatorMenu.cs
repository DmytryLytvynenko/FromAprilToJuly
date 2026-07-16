using UnityEngine;
using UnityEngine.InputSystem;

public class ChargeIndicatorMenu : MonoBehaviour
{
    private Vector2 _mousePos;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
    private void Update()
    {
        _rectTransform.position = _mousePos;
    }
    private void OnEnable()
    {
        InputManager.Instance.OnMousePosition.AddListener(HandleMousePosition);
    }
    private void OnDisable()
    {
        InputManager.Instance.OnMousePosition.RemoveListener(HandleMousePosition);
    }
    private void HandleMousePosition(InputAction.CallbackContext ctx)
    {
        _mousePos = ctx.ReadValue<Vector2>();
    }
}
