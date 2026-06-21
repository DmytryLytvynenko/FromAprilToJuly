using UnityEngine;

public class DragonPart : MonoBehaviour
{
    public float MoveSpeed;
    public float RotationSpeed;
    public float FollowOffset;
    [SerializeField] protected Transform _visual;

    protected DragonController _controller;
    protected DragonPart _previousPart;
    protected Vector3 _followPosition;
    protected Quaternion _targetRotation;
    protected bool copyPreviousVisualZRotation = true;
    protected bool _customTargetRotation = false;
    protected Transform thisTransform;

    public virtual void SetUp(float followSpeed, float rotationSpeed, float followOffset, DragonPart previousPart, DragonController controller)
    {
        thisTransform = transform;
        MoveSpeed = followSpeed;
        RotationSpeed = rotationSpeed;
        this.FollowOffset = followOffset;
        _controller = controller;
        _previousPart = previousPart;
    }
    protected virtual void Move()
    {
        if (_controller.Move)
        {
            _followPosition = -_previousPart._visual.forward * FollowOffset + _previousPart._visual.position;
            Vector3 currentPos = thisTransform.position;
            thisTransform.position = Vector3.Lerp(currentPos, _followPosition, Time.fixedDeltaTime * MoveSpeed);
        }
        else
        {
            Vector3 currentPos = thisTransform.position;
            thisTransform.position = Vector3.Lerp(currentPos, _followPosition, Time.fixedDeltaTime * MoveSpeed);
        }
    }
    protected virtual void Rotate()
    {
        if (_controller.Move)
        {
            _targetRotation = Quaternion.LookRotation(_previousPart._visual.position - thisTransform.position);
            Quaternion currentRotation = thisTransform.rotation;
            Quaternion rot = Quaternion.Lerp(currentRotation, _targetRotation, RotationSpeed * Time.fixedDeltaTime);
            thisTransform.rotation = rot;
        }
        else
        {

            Quaternion currentRotation = thisTransform.rotation;
            Quaternion rot = Quaternion.Lerp(currentRotation, _targetRotation, RotationSpeed * Time.fixedDeltaTime);
            thisTransform.rotation = rot;
        }
    }
    protected virtual void RotateLocalZ()
    {
        if (copyPreviousVisualZRotation)
        {
            Vector3 targetRotation = _previousPart._visual.localEulerAngles;
            Vector3 currentRotation = _visual.localEulerAngles;
            currentRotation.z = Mathf.LerpAngle(_visual.localEulerAngles.z, targetRotation.z, Time.fixedDeltaTime * RotationSpeed);
            _visual.localRotation = Quaternion.Euler(currentRotation);
        }
    }
    protected void SetTargetRotation(Vector3 Euler)
    {
        //_customTargetRotation = true;
        _targetRotation = Quaternion.Euler(Euler);
    }
    protected virtual void FixedUpdate()
    {
        if (!_controller) return;
        Move();
        Rotate();
        RotateLocalZ();
    }
}
