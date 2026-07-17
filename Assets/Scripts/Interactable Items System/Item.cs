using System;
using UnityEditor.Sprites;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    public bool Picked { get; private set; } = false;
    public float Mass { get { return _rigidbody.mass; } }
    public static event Action ItemPicked;
    public static event Action ItemReleased;
    [SerializeField] protected float _followForce = 1f;
    [SerializeField] protected float _rotationForce = 5f;
    [SerializeField] protected float _dampingFactor = 10f;
    [SerializeField] protected float _gravity = -15f;
    [SerializeField] protected float _upMaxSpeed = 7f;
    [SerializeField] protected float _downMaxSpeed = -5f;
    [SerializeField] protected float _ySpeedMultiplier = 0.2f;
    [SerializeField] protected Rigidbody _rigidbody;
    [SerializeField] protected Material _material;
    [SerializeField] protected MeshRenderer _renderer;
    [SerializeField] protected SFXActor _SFXActor;

    protected Transform _followTarget = null;
    protected Quaternion _currentRotation = Quaternion.identity;
    protected Vector3 _targetRotation = Vector3.zero;
    protected bool _rotate = false;

    protected virtual void Start()
    {
        TryGetComponent(out MeshRenderer meshRenderer);
        TryGetComponent(out Rigidbody rigidbody);
        if (!_renderer) _renderer = meshRenderer;
        if (!_rigidbody) _rigidbody = rigidbody;

        _material = _renderer?.material;
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
        //PlayPickupSound();
        ItemPicked?.Invoke();
        //Highlight();
    }
    public virtual void Release() 
    {
        ItemReleased?.Invoke();
        _rotate = false;
        Picked = false;
    }
    public virtual void Release(Vector3 throwDir) 
    {
        //PlayReleaseSound();
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
        if (!Picked) return;

        Vector3 targetVel = (_followTarget.position - transform.position) * _followForce;
        Vector3 v = _rigidbody.linearVelocity;
        v.x = targetVel.x;
        v.z = targetVel.z;
        if (transform.position.y > _followTarget.position.y) 
        {
            v.y += v.y > _downMaxSpeed ? targetVel.y * _ySpeedMultiplier : 0;
        }
        else
        {
            v.y += v.y < _upMaxSpeed ? targetVel.y * _ySpeedMultiplier : 0;
        }

        _rigidbody.linearVelocity = v;
    }
    protected virtual void Rotate()
    {
        if (!_rotate) return;

        Quaternion targetRotation = Quaternion.Euler(_targetRotation);

        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(transform.rotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;

        if (Mathf.Abs(angle) < 0.5f)
        {
            _rigidbody.angularVelocity = Vector3.zero;
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
    protected virtual void Gravity()
    {
        //if (Picked && transform.position.y > _followTarget.position.y) return;

        Vector3 v = _rigidbody.linearVelocity;
        v.y = v.y + _gravity * Time.fixedDeltaTime;
        _rigidbody.linearVelocity = v;
    }
    protected virtual void PlayPickupSound()
    {
        _SFXActor.PlaySound(SFX.ItemPickup);
    }
    protected virtual void PlayReleaseSound()
    {
        _SFXActor.PlaySound(SFX.ItemRelease);
    }
    protected virtual void FixedUpdate()
    {
        Gravity();
        Follow();
        Rotate();
    }
}
