using UnityEngine;
using UnityEngine.InputSystem;

public class FpsLimiter : MonoBehaviour
{
    [SerializeField] private int _targetFps = 60;
    [SerializeField] private float _timeScale = 1f;

    private void Start()
    {
        Application.targetFrameRate = _targetFps;
    }
    private void OnEnable()
    {
        InputManager.Instance.OnPauseEditor += PauseEditor;
    }
    private void OnDisable()
    {
        InputManager.Instance.OnPauseEditor -= PauseEditor;
    }

    private void OnValidate()
    {
        Time.timeScale = _timeScale;
        if (Application.isPlaying)
        {
            Application.targetFrameRate = _targetFps;
        }
    }
    private void PauseEditor()
    {
        Debug.Break();
    }
}