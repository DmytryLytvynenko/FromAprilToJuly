using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    public bool Picked { get; private set; } = false;
    public static event Action ItemPicked;
    public static event Action ItemReleased;
    [SerializeField] protected float _followForce = 1f;
    [SerializeField] protected float _rotationForce = 5f;
    [SerializeField] protected float _dampingFactor = 10f;

    protected Transform _followTarget = null;
    protected Rigidbody _rigidbody;
    protected Material _material;
    protected Quaternion _currentRotation = Quaternion.identity;
    protected Vector3 _targetRotation = Vector3.zero;
    protected bool _rotate = false;

    protected virtual void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
        _rigidbody = GetComponent<Rigidbody>();
    }
    public virtual void PickUp(Transform target) 
    {
        Picked = true;
        _followTarget = target;
        Highlight();
    }
    public virtual void PickUp(Transform target, float followForce) 
    {
        Picked = true;
        _rotate = true;
        _followTarget = target;
        _followForce = followForce;
        ItemPicked?.Invoke();
        Highlight();
    }
    public virtual void Release() 
    {
        ItemReleased?.Invoke();
        _rotate = false;
        Picked = false;
    }
    public virtual void Release(Vector3 throwDir) 
    {
        ItemReleased?.Invoke();
        _rotate = false;
        Picked = false;
        _rigidbody.AddForce(throwDir, ForceMode.Impulse);
    }
    public virtual void Highlight() 
    {
        _material.color = Color.black;
    }
    public virtual void RemoveHighlight()
    {
        _material.color = Color.white;
    }
    protected virtual void Follow()
    {
        //_rigidbody.AddForce((_followTarget.position - transform.position) * _followForce, ForceMode.Impulse);
        _rigidbody.linearVelocity = (_followTarget.position - transform.position) * _followForce;
    }
    protected virtual void Rotate()
    {
        Quaternion targetRotation = Quaternion.Euler(_targetRotation);

        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(transform.rotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;

        if (Mathf.Abs(angle) < 0.5f)
        {
            _rigidbody.angularVelocity = Vector3.zero;
            //_rotate = false;
            return;
        }

        Vector3 targetAngularVelocity = axis * (angle * Mathf.Deg2Rad * _rotationForce);
        Vector3 correction = targetAngularVelocity - _rigidbody.angularVelocity;

        _rigidbody.angularVelocity += correction * _dampingFactor * Time.fixedDeltaTime;
    }
    public virtual void ChangeTargetRotation(float deltaX, float deltaY)
    {
        _targetRotation = new Vector3(_targetRotation.x + deltaX, _targetRotation.y + deltaY, 0);
        _rotate = true;
    }
    public virtual void Stabilize()
    {
        _targetRotation = Vector2.zero;
        _rotate = true;
    }
    protected virtual void FixedUpdate()
    {
        if (Picked)
        {
            Follow();
        }
        if (_rotate)
        {
            Rotate();
        }    
    }
}
