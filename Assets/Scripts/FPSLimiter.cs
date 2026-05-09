using UnityEngine;

public class FpsLimiter : MonoBehaviour
{
    [SerializeField] private int _targetFps = 60;

    private void Start()
    {
        Application.targetFrameRate = _targetFps;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            Application.targetFrameRate = _targetFps;
        }
    }
}