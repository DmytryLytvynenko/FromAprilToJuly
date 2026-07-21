using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI _fpsText;

    [Header("Settings")]
    [Tooltip("How often the FPS display updates (in seconds)")]
    [SerializeField] private float _updateInterval = 0.5f;

    private float _accumulatedTime = 0f;
    private int _framesCount = 0;
    private float _timeleft;

    private void Start()
    {
        if (_fpsText == null)
        {
            _fpsText = GetComponent<TextMeshProUGUI>();
        }

        _timeleft = _updateInterval;
    }

    private void Update()
    {
        _timeleft -= Time.unscaledDeltaTime;
        _accumulatedTime += Time.unscaledDeltaTime;
        _framesCount++;

        if (_timeleft <= 0f)
        {
            float fps = _framesCount / _accumulatedTime;
            UpdateFPSText(fps);

            _timeleft = _updateInterval;
            _accumulatedTime = 0f;
            _framesCount = 0;
        }
    }

    private void UpdateFPSText(float fps)
    {
        if (_fpsText == null) return;

        int roundedFps = Mathf.RoundToInt(fps);
        _fpsText.text = $"FPS: {roundedFps}";

        if (roundedFps >= 60)
        {
            _fpsText.color = Color.green;
        }
        else if (roundedFps >= 30)
        {
            _fpsText.color = Color.yellow;
        }
        else
        {
            _fpsText.color = Color.red;
        }
    }
}
