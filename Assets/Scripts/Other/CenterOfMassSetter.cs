using UnityEngine;

public class CenterOfMassSetter : MonoBehaviour
{
    [SerializeField] private Transform _centerOfMass;
    private Rigidbody _rb;
    private void OnValidate()
    {
        if (_rb)
        {
            _rb.centerOfMass = _centerOfMass.localPosition;
        }
    }
    void Start()
    {
        if (TryGetComponent(out Rigidbody rb))
        {
            _rb = rb;
            _rb.centerOfMass = _centerOfMass.localPosition;
        }
        else
        {
            #if UNITY_EDITOR
                Debug.LogError("Cannot set centerOfMass, Rigidbody is null");
            #endif
        }
    }
    private void OnDrawGizmos()
    {
        if (_rb)
        {
            Gizmos.DrawSphere(transform.TransformPoint(_rb.centerOfMass), .2f);
        }
    }
}
