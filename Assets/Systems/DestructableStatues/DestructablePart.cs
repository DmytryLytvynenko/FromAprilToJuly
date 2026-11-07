using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DestructablePart : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] private float positionSpring = 200f;
    [SerializeField] private float positionDamping = 28f;
    [SerializeField] private float maxAcceleration = 100f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpring = 150f;
    [SerializeField] private float rotationDamping = 24f;
    [SerializeField] private float maxAngularAcceleration = 100f;

    [Header("Snapping")]
    [SerializeField] private float positionTolerance = 0.02f;
    [SerializeField] private float angleTolerance = 2f;
    [SerializeField] private float speedTolerance = 0.1f;

    private Transform _root;
    private Vector3 _defaultLocalPos;
    private Quaternion _defaultLocalRot;

    public Rigidbody Rigidbody { get; private set; }
    [field: SerializeField] public bool IsAssembled { get; set; } = true;

    public void Initialize(Transform root)
    {
        Rigidbody = GetComponent<Rigidbody>();
        _root = root;

        _defaultLocalPos = _root.InverseTransformPoint(Rigidbody.position);
        _defaultLocalRot = Quaternion.Inverse(_root.rotation) * Rigidbody.rotation;

        enabled = false;
    }

    public async UniTask Assemble(CancellationToken ct = default)
    {
        Rigidbody.isKinematic = false; 
        IsAssembled = false;
        enabled = true;
        await UniTask.WaitUntil(() => IsAssembled, PlayerLoopTiming.FixedUpdate, ct);
    }
    public void Release()
    {
        Rigidbody.isKinematic = false;
        IsAssembled = false;
    }

    private void GetTarget(out Vector3 pos, out Quaternion rot)
    {
        pos = _root.TransformPoint(_defaultLocalPos);
        rot = _root.rotation * _defaultLocalRot;
    }

    private void FixedUpdate()
    {
        GetTarget(out var targetPos, out var targetRot);

        if ((targetPos - Rigidbody.position).magnitude < positionTolerance
            && Quaternion.Angle(Rigidbody.rotation, targetRot) < angleTolerance
            && Rigidbody.linearVelocity.magnitude < speedTolerance)
        {
            Snap(targetPos, targetRot);
            return;
        }

        Vector3 accel = (targetPos - Rigidbody.position) * positionSpring - Rigidbody.linearVelocity * positionDamping;
        Rigidbody.AddForce(Vector3.ClampMagnitude(accel, maxAcceleration), ForceMode.Acceleration);
        if (Rigidbody.linearVelocity.magnitude < speedTolerance)
        {
            Rigidbody.linearVelocity = accel.normalized * .5f;
        }

        (targetRot * Quaternion.Inverse(Rigidbody.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        if (float.IsFinite(axis.x))
        {
            Vector3 torque = axis * (angle * Mathf.Deg2Rad) * rotationSpring - Rigidbody.angularVelocity * rotationDamping;
            Rigidbody.AddTorque(Vector3.ClampMagnitude(torque, maxAngularAcceleration), ForceMode.Acceleration);
        }
    }

    private void Snap(Vector3 pos, Quaternion rot)
    {
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.isKinematic = true;
        Rigidbody.position = pos;
        Rigidbody.rotation = rot;

        IsAssembled = true;
        enabled = false;
    }
}