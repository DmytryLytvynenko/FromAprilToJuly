using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

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
    [HideInInspector] public event Action OnDebug;

    public Vector2 CurrentMoveInput => _currentMoveInput;
    public bool JumpPressed => _jumpPressed;
    public bool InteractPressed => _interactPressed;
    public Vector2 CurrentLookInput => _currentLookInput;

    private Vector2 _currentMoveInput = Vector2.zero;
    private bool _jumpPressed = false;
    private bool _interactPressed = false;
    private Vector2 _currentLookInput = Vector2.zero;

    private void OnEnable()
    {
        InitializeInputActions();
    }
    private void OnDisable()
    {
        DisableInputActions();
    }
    private void InitializeInputActions()
    {
        _mainInput = new MainInput();

        _mainInput.Player.Enable();

        var moveAction = _mainInput.Player.Move;
        var jumpAction = _mainInput.Player.Jump;
        var interactAction = _mainInput.Player.Interact;
        var lookAction = _mainInput.Player.Look;
        var debugAction = _mainInput.Player.Debug;


        moveAction.performed += OnMoveInput;
        moveAction.canceled += OnMoveInput;

        jumpAction.performed += OnJumpInput;
        jumpAction.canceled += OnJumpInput;

        interactAction.performed += OnInteractInput;
        interactAction.canceled += OnInteractInput;

        lookAction.performed += OnLookInput;
        lookAction.canceled += OnLookInput;

        debugAction.performed += OnDebugInput;
    }
    private void DisableInputActions()
    {
        var moveAction = _mainInput.Player.Move;
        var jumpAction = _mainInput.Player.Jump;
        var interactAction = _mainInput.Player.Interact;
        var lookAction = _mainInput.Player.Look;
        var debugAction = _mainInput.Player.Debug;

        moveAction.performed -= OnMoveInput;
        moveAction.canceled -= OnMoveInput;

        jumpAction.performed -= OnJumpInput;
        jumpAction.canceled -= OnJumpInput;

        interactAction.performed -= OnInteractInput;
        interactAction.canceled -= OnInteractInput;

        lookAction.performed -= OnLookInput;
        lookAction.canceled -= OnLookInput;

        debugAction.performed -= OnDebugInput;

        _mainInput.Player.Disable();
    }
    public void CreateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void OnMoveInput(InputAction.CallbackContext ctx)
    {
        _currentMoveInput = ctx.ReadValue<Vector2>();
        if (ctx.canceled) _currentMoveInput = Vector2.zero;
        OnMove.Invoke(ctx);
    }

    private void OnJumpInput(InputAction.CallbackContext ctx)
    {
        _jumpPressed = ctx.performed;
        OnJump.Invoke(ctx);
    }

    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        _interactPressed = ctx.performed;
        OnInteract.Invoke(ctx);
    }
    private void OnLookInput(InputAction.CallbackContext ctx)
    {
        _currentLookInput = ctx.ReadValue<Vector2>();
        if (ctx.canceled) _currentLookInput = Vector2.zero;
        OnLook.Invoke(ctx);
    }
    private void OnDebugInput(InputAction.CallbackContext ctx)
    {
        OnDebug?.Invoke();
    }
}

