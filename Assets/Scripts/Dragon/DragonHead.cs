using UnityEngine;

public class DragonHead : DragonPart
{
    [field: SerializeField] public MeshCollider MeshCollider { get; protected set; }
    private Quaternion _targetRotation;
    public void SetUp(float followSpeed, float rotationSpeed, float followOffset, DragonController controller)
    {
        if (TryGetComponent(out MeshCollider meshCollider))
        {
            MeshCollider = meshCollider;
        }
        Item = GetComponent<Item>();
        Rigidbody = GetComponent<Rigidbody>();
        BoxCollider = GetComponent<BoxCollider>();
        thisTransform = transform;
        MoveSpeed = followSpeed;
        RotationSpeed = rotationSpeed;
        this.FollowOffset = followOffset;
        _controller = controller;
    }
    public override void DisableCollider() => MeshCollider.enabled = false;
    public override void EnableCollider() => MeshCollider.enabled = true;
    public override void ResetColliderSize() { }
    protected override void Move()
    {
        if (!_controller.Move) return;
        if (!_controller.Loop && _controller.LastWaypoint)
        {
            Vector3 _movePosition = _controller.CurrentWaypoint.transform.position;
            thisTransform.position = Vector3.Lerp(thisTransform.position, _movePosition, Time.fixedDeltaTime * MoveSpeed);
        }
        else
        {
            Vector3 _movePosition = thisTransform.position + thisTransform.forward * Time.fixedDeltaTime * MoveSpeed;
            thisTransform.position = _movePosition;
        }
    }
    protected override void Rotate()
    {
        _targetRotation = Quaternion.LookRotation(_controller.CurrentWaypoint.transform.position - thisTransform.position);
        Quaternion currentRotation = thisTransform.rotation;
        Quaternion rot = Quaternion.Lerp(currentRotation, _targetRotation, RotationSpeed * Time.fixedDeltaTime);
        thisTransform.rotation = rot;
    }
    protected override void FixedUpdate()
    {
        if (!_controller) return;
        Move();
        Rotate();
    }
}
