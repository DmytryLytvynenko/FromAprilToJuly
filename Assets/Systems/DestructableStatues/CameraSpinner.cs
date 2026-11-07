using UnityEngine;
public class CameraSpinner : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 30f;
    [SerializeField] bool _invertRotation = false;

    private Transform m_Transform;
    private float _YAngle;

    private void Start()
    {
        m_Transform = transform;
        _YAngle = m_Transform.eulerAngles.y;
    }
    private void Update()
    {
        float delta = _rotationSpeed * Time.deltaTime * (_invertRotation ? -1 : 1);
        m_Transform.Rotate(Vector3.up, delta);
        _YAngle = Mathf.Repeat(_YAngle + delta, 360f); 
    }
}