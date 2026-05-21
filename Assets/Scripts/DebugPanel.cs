using TMPro;
using UnityEngine;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private GameObject _debugPanel;
    [SerializeField] private TextMeshProUGUI GroundedTB;
    private GroundDetector _groundDetector;
    
    public void Initialize(GroundDetector groundDetector)
    {
        _groundDetector = groundDetector;
        gameObject.SetActive(true);
        
    }
    private void OnEnable()
    {
        InputManager.Instance.OnDebug += OnDebug;
    }
    private void OnDisable()
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
    }

    private void UpdateText()
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
}
