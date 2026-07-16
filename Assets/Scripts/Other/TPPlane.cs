using UnityEngine;

public class TPPlane : MonoBehaviour
{
    [SerializeField] private PlayZoneChecker _playZoneChecker;
    [SerializeField] private Transform _playZoneTPPoint;
    [SerializeField] private Transform[] _TPPoints;
    [SerializeField] private float angularTorgue = 5f;
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (_playZoneChecker.NotEnoughItems)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = _playZoneTPPoint.position;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = _TPPoints[Random.Range(0, _TPPoints.Length)].position;
            rb.angularVelocity = RandomNormalizedDirection() * angularTorgue;
        }
    }
    private Vector3 RandomNormalizedDirection()
    {
        return new Vector3(
                   UnityEngine.Random.Range(-1f, 1f),
                   UnityEngine.Random.Range(-1f, 1f),
                   UnityEngine.Random.Range(-1f, 1f));
    }
}
