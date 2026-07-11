using UnityEngine;

public class HingeJointHealthyDisable : MonoBehaviour
{
    private Quaternion _startRotation;
    private Vector3 _startPosition;
    void Start()
    {
        _startRotation = transform.rotation;
        _startPosition = transform.position;
    }
    private void OnDisable()
    {
        transform.rotation = _startRotation;
        transform.position = _startPosition;
    }
}
