using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    public bool Picked { get; private set; } = false;
    public static event Action ItemPicked;
    public static event Action ItemReleased;
    [SerializeField] protected float _followForce = 1f;
    [SerializeField] protected float _rotationForce = 5f;

    protected Transform _followTarget = null;
    protected Rigidbody _rigidbody;
    protected Material _material;
    protected Quaternion _currentRotation = Quaternion.identity;
    protected Vector3 _targetRotation = Vector3.zero;

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
        _followTarget = target;
        _followForce = followForce;
        ItemPicked?.Invoke();
        Highlight();
    }
    public virtual void Release() 
    {
        ItemReleased?.Invoke();
        Picked = false;
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
        //_rigidbody.AddForce((_followTarget.position - transform.position) * _followForce, ForceMode.Impulse);
        Vector3 currentRotation = transform.rotation.eulerAngles;
        float deltaX = AngleClamp.MinAngleBetween(currentRotation.x, _targetRotation.x);
        float deltaY = AngleClamp.MinAngleBetween(currentRotation.y, _targetRotation.y);
        Vector3 desiredVelocity = new Vector3(deltaX * _rotationForce, deltaY * _rotationForce, 0);
        _rigidbody.angularVelocity = desiredVelocity;
    }
    public virtual void AdjustTargetRotation(float X, float Y)
    {
        _targetRotation = new Vector3(X, Y, 0);
    }
    protected virtual void FixedUpdate()
    {
        if (Picked)
        {
            Follow();
            Rotate();
        }
    }
}
