using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private MainInput _mainInput;

    [System.Serializable]
    public class InputEvent : UnityEvent<InputAction.CallbackContext> { }

    [HideInInspector] public InputEvent OnMove = new InputEvent();
    [HideInInspector] public InputEvent OnJump = new InputEvent();
    [HideInInspector] public InputEvent OnInteract = new InputEvent();
    [HideInInspector] public InputEvent OnLook = new InputEvent();
    [HideInInspector] public InputEvent OnAim = new InputEvent();
    [HideInInspector] public InputEvent OnRotateHorizontal = new InputEvent();
    [HideInInspector] public InputEvent OnScroll = new InputEvent();
    [HideInInspector] public InputEvent OnStabilizeRotation = new InputEvent();
    [HideInInspector] public InputEvent OnToggleScroll= new InputEvent();
    [HideInInspector] public InputEvent OnMousePosition= new InputEvent();
    [HideInInspector] public InputEvent OnMenu= new InputEvent();
    [HideInInspector] public event Action OnDebug;
    [HideInInspector] public event Action OnPauseEditor;

    public Vector2 CurrentMoveInput => _currentMoveInput;
    public bool JumpPressed => _jumpPressed;
    public bool InteractPressed => _interactPressed;
    public Vector2 CurrentLookInput => _currentLookInput;

    private Vector2 _currentMoveInput = Vector2.zero;
    private bool _jumpPressed = false;
    private bool _interactPressed = false;
    private bool _aimPressed = false;
    private Vector2 _currentLookInput = Vector2.zero;

    public void Initialize()
    {
        InitializeInputActions();
    }
    public void HandleDisable()
    {
        DisableInputActions();
    }
    private void InitializeInputActions()
    {
        _mainInput = new MainInput();

        _mainInput.Player.Enable();
        _mainInput.UI.Enable();

        var moveAction = _mainInput.Player.Move;
        var jumpAction = _mainInput.Player.Jump;
        var interactAction = _mainInput.Player.Interact;
        var lookAction = _mainInput.Player.Look;
        var aimAction = _mainInput.Player.Aim;
        var rotateHorizontal = _mainInput.Player.RotateHorizontal;
        var scroll = _mainInput.Player.Scroll;
        var stabilizeRotation = _mainInput.Player.StabilizeRotation;
        var toggleScroll = _mainInput.Player.ToggleScroll;
        var debugAction = _mainInput.Player.Debug;
        var pauseEditor = _mainInput.Player.PauseEditor;
        var mousePosition = _mainInput.Player.MousePosition;
        var menu = _mainInput.UI.Menu;


        moveAction.performed += OnMoveInput;
        moveAction.canceled += OnMoveInput;

        jumpAction.performed += OnJumpInput;
        jumpAction.canceled += OnJumpInput;

        interactAction.performed += OnInteractInput;
        interactAction.canceled += OnInteractInput;

        lookAction.performed += OnLookInput;
        lookAction.canceled += OnLookInput;

        aimAction.performed += OnAimInput;
        aimAction.canceled += OnAimInput;

        rotateHorizontal.performed += OnRotateHorizontalInput;

        scroll.performed += OnScrollInput;

        stabilizeRotation.performed += OnStabilizeRotationInput;

        toggleScroll.performed += OnToggleScrollInput;
        toggleScroll.canceled += OnToggleScrollInput;

        debugAction.performed += OnDebugInput;
        //pauseEditor.performed += OnPauseEditorInput;
        mousePosition.performed += OnMousePositionInput;
        menu.performed += OnMenuInput;
    }
    private void DisableInputActions()
    {
        var moveAction = _mainInput.Player.Move;
        var jumpAction = _mainInput.Player.Jump;
        var interactAction = _mainInput.Player.Interact;
        var lookAction = _mainInput.Player.Look;
        var aimAction = _mainInput.Player.Aim;
        var rotateHorizontal = _mainInput.Player.RotateHorizontal;
        var scroll = _mainInput.Player.Scroll;
        var stabilizeRotation = _mainInput.Player.StabilizeRotation;
        var toggleScroll = _mainInput.Player.ToggleScroll;
        var debugAction = _mainInput.Player.Debug;
        var pauseEditor = _mainInput.Player.PauseEditor;
        var mousePosition = _mainInput.Player.MousePosition;
        var menu = _mainInput.UI.Menu;

        moveAction.performed -= OnMoveInput;
        moveAction.canceled -= OnMoveInput;

        jumpAction.performed -= OnJumpInput;
        jumpAction.canceled -= OnJumpInput;

        interactAction.performed -= OnInteractInput;
        interactAction.canceled -= OnInteractInput;

        lookAction.performed -= OnLookInput;
        lookAction.canceled -= OnLookInput;

        aimAction.performed -= OnAimInput;
        aimAction.canceled -= OnAimInput;

        rotateHorizontal.performed -= OnRotateHorizontalInput;

        scroll.performed -= OnScrollInput;

        stabilizeRotation.performed -= OnStabilizeRotationInput;

        toggleScroll.performed -= OnToggleScrollInput;
        toggleScroll.canceled -= OnToggleScrollInput;

        debugAction.performed -= OnDebugInput;
        //pauseEditor.performed -= OnPauseEditorInput;
        mousePosition.performed -= OnMousePositionInput;
        menu.performed -= OnMenuInput;

        _mainInput.Player.Disable();
        _mainInput.UI.Disable();
    }
    public void CreateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void DisableMovement()
    {
        if (_mainInput == null) return;

        _mainInput.Player.Move.Disable();
        _mainInput.Player.Jump.Disable();

        _currentMoveInput = Vector2.zero;
        _jumpPressed = false;
    }
    public void EnableMovement()
    {
        if (_mainInput == null) return;

        _mainInput.Player.Move.Enable();
        _mainInput.Player.Jump.Enable();
    }
    private void OnMoveInput(InputAction.CallbackContext ctx)
    {
        _currentMoveInput = ctx.ReadValue<Vector2>();
        if (ctx.canceled) _currentMoveInput = Vector2.zero;
        OnMove?.Invoke(ctx);
    }
    public void DisableCamera()
    {
        if (_mainInput == null) return;

        _mainInput.Player.Look.Disable();
        _mainInput.Player.Aim.Disable();
        _mainInput.Player.RotateHorizontal.Disable();
        _mainInput.Player.Scroll.Disable();
        _mainInput.Player.StabilizeRotation.Disable();

        _currentLookInput = Vector2.zero;
        _aimPressed = false;
    }
    public void EnableCamera()
    {
        if (_mainInput == null) return;

        _mainInput.Player.Look.Enable();
        _mainInput.Player.Aim.Enable();
        _mainInput.Player.RotateHorizontal.Enable();
        _mainInput.Player.Scroll.Enable();
        _mainInput.Player.StabilizeRotation.Enable();
    }

    private void OnJumpInput(InputAction.CallbackContext ctx)
    {
        _jumpPressed = ctx.performed;
        OnJump?.Invoke(ctx);
    }

    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        _interactPressed = ctx.performed;
        OnInteract?.Invoke(ctx);
    }
    private void OnLookInput(InputAction.CallbackContext ctx)
    {
        _currentLookInput = ctx.ReadValue<Vector2>();
        if (ctx.canceled) _currentLookInput = Vector2.zero;
        OnLook?.Invoke(ctx);
    }
    private void OnAimInput(InputAction.CallbackContext ctx)
    {
        _aimPressed = ctx.performed;
        OnAim?.Invoke(ctx);
    }
    private void OnRotateHorizontalInput(InputAction.CallbackContext ctx)
    {
        OnRotateHorizontal?.Invoke(ctx);
    }
    private void OnScrollInput(InputAction.CallbackContext ctx)
    {
        OnScroll?.Invoke(ctx);
    }
    private void OnStabilizeRotationInput(InputAction.CallbackContext ctx)
    {
        OnStabilizeRotation?.Invoke(ctx);
    }
    private void OnToggleScrollInput(InputAction.CallbackContext ctx)
    {
        OnToggleScroll?.Invoke(ctx);
    }
    private void OnDebugInput(InputAction.CallbackContext ctx)
    {
        OnDebug?.Invoke();
    }
    private void OnPauseEditorInput(InputAction.CallbackContext ctx)
    {
        OnPauseEditor?.Invoke();
    }
    private void OnMousePositionInput(InputAction.CallbackContext ctx)
    {
        OnMousePosition?.Invoke(ctx);
    }
    private void OnMenuInput(InputAction.CallbackContext ctx)
    {
        OnMenu?.Invoke(ctx);
    }
}

