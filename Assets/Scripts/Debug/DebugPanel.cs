using TMPro;
using UnityEngine;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private GameObject _debugPanel;
    [SerializeField] private TextMeshProUGUI GroundedTB;
    [SerializeField] private TextMeshProUGUI LinearVelocityTB;
    private GroundDetector _groundDetector;
    private Rigidbody _playerRigidbody;
    
    public void Initialize(GroundDetector groundDetector, Rigidbody playerRigidbody)
    {
        _playerRigidbody = playerRigidbody;
        _groundDetector = groundDetector;
        gameObject.SetActive(true);
        InputManager.Instance.OnDebug += OnDebug;
    }
    public void HandleDisable()
    {
        InputManager.Instance.OnDebug -= OnDebug;
    }
    private void Update()
    {
        UpdateText();
    }

    private void OnDebug()
    {
        _debugPanel.SetActive(!_debugPanel.activeSelf);
/*        #if UNITY_EDITOR
                _debugPanel.SetActive(!_debugPanel.activeSelf);
        #endif*/
    }

    private void UpdateText()
    {
        UpdateGroundedText();
        UpdateLinearVelocityTBText();
        /*        #if UNITY_EDITOR
                    UpdateGroundedText();
                    UpdateLinearVelocityTBText();
                #endif*/
    }

    private void UpdateGroundedText()
    {
        if (_groundDetector.Grounded)
        {
            GroundedTB.color = Color.green;
            GroundedTB.text = "True";
        }
        else
        {
            GroundedTB.color = Color.red;
            GroundedTB.text = "False";
        }
    }
    private void UpdateLinearVelocityTBText()
    {
        LinearVelocityTB.text = _playerRigidbody.linearVelocity.ToString();
    }
}
