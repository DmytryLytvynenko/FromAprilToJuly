using UnityEngine;

public class DragonPart : MonoBehaviour
{
    public float MoveSpeed;
    public float RotationSpeed;
    public float FollowOffset;
    public bool IgnorePreviousPart = false;
    [SerializeField] protected Transform _visual;

    protected DragonController _controller;
    protected DragonPart _previousPart;
    protected Vector3 _followPosition;
    protected bool copyPreviousVisualZRotation = true;
    protected Transform thisTransform;

    protected Quaternion _baseRotation;     
    public float CurrentXAngle = 0f;        
    public float CurrentYAngle = 0f;        
    public float CurrentZAngle = 0f;

    protected float _targetXAngle = 0f;     
    protected float _targetYAngle = 0f;
    protected float _targetZAngle = 0f;

    public virtual void SetUp(float followSpeed, float rotationSpeed, float followOffset, DragonPart previousPart, DragonController controller)
    {
        thisTransform = transform;
        MoveSpeed = followSpeed;
        RotationSpeed = rotationSpeed;
        this.FollowOffset = followOffset;
        _controller = controller;
        _previousPart = previousPart;

        _baseRotation = thisTransform.rotation; 
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
        if (_controller.Rotate && !IgnorePreviousPart)
        {
            float t = RotationSpeed * Time.fixedDeltaTime;

            if (_controller.RotationModeCopy)
            {
                if (_controller.CopyRotationX)
                {
                    CurrentXAngle = Mathf.LerpAngle(CurrentXAngle, _previousPart.CurrentXAngle, t);
                }
                else
                {
                    CurrentYAngle = Mathf.LerpAngle(CurrentYAngle, _previousPart.CurrentYAngle, t);
                }
            }
            else
            {
                Quaternion lookRot = Quaternion.LookRotation(_previousPart._visual.position - thisTransform.position);
                Quaternion delta = Quaternion.Inverse(_baseRotation) * lookRot;
                Vector3 deltaEuler = delta.eulerAngles; 
                _targetXAngle = NormalizeAngle(deltaEuler.x);
                _targetYAngle = NormalizeAngle(deltaEuler.y);

                CurrentXAngle = Mathf.LerpAngle(CurrentXAngle, _targetXAngle, t);
                CurrentYAngle = Mathf.LerpAngle(CurrentYAngle, _targetYAngle, t);
            }

            ApplyRotation();
            return;
        }
        else
        {
            float t = RotationSpeed * Time.fixedDeltaTime;
            CurrentXAngle = Mathf.LerpAngle(CurrentXAngle, _targetXAngle, t);
            CurrentYAngle = Mathf.LerpAngle(CurrentYAngle, _targetYAngle, t);
            CurrentZAngle = Mathf.LerpAngle(CurrentZAngle, _targetZAngle, t);
            ApplyRotation();
        }
    }

    protected void ApplyRotation()
    {
        thisTransform.rotation = _baseRotation
                                * Quaternion.AngleAxis(CurrentYAngle, Vector3.up)
                                * Quaternion.AngleAxis(CurrentXAngle, Vector3.right)
                                * Quaternion.AngleAxis(CurrentZAngle, Vector3.forward);
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

    public void SetTargetRotation(Vector3 Euler)
    {
        _targetXAngle += Euler.x;
        _targetYAngle += Euler.y;
        _targetZAngle += Euler.z;
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    protected virtual void FixedUpdate()
    {
        if (!_controller) return;
        Move();
        Rotate();
        RotateLocalZ();
    }
}