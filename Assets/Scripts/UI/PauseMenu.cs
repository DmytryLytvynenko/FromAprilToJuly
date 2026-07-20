using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _menuContainer;
    [SerializeField] private SFXActor _menuSFX;

    public void Initialize()
    {
        InputManager.Instance.OnMenu.AddListener(OnMenu);
    }
    public void HandleDisable()
    {
        InputManager.Instance.OnMenu.RemoveListener(OnMenu);
    }
    private void OnMenu(InputAction.CallbackContext ctx)
    {
        if (_menuContainer.activeSelf)
        {
            _menuContainer.SetActive(false);
            EnableInput();
            LockCursor();
        }
        else
        {
            _menuContainer.SetActive(true);
            DisableInput();
            UnlockCursor();
        }
        _menuSFX.PlaySound(SFX.Submit);
    }

    public void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void CloseGame()
    {
        Application.Quit();
    }
    public void EnableInput()
    {
        InputManager.Instance.EnableMovement();
        InputManager.Instance.EnableCamera();
    }
    public void DisableInput()
    {
        InputManager.Instance.DisableMovement();
        InputManager.Instance.DisableCamera();
    }
}
