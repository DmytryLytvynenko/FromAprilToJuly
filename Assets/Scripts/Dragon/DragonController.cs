using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    [field:SerializeField] public bool Loop { get; set; } = false;
    [field:SerializeField] public bool CopyFirstPart { get; set; } = true;
    [field:SerializeField] public bool LookAtPlayer { get; set; } = true;
    [field:SerializeField] public bool Move { get; set; } = false;
    [field:SerializeField] public bool Rotate { get; set; } = false;
    [field:SerializeField] public bool RotationModeCopy { get; set; } = false;
    [field:SerializeField] public bool CopyRotationX { get; set; } = false;
    [field:SerializeField] public bool CopyScale { get; set; } = false;
    public bool LastWaypoint { get { return _currentWaypointIndex == _wayPoints.Count - 1; } }

    [SerializeField] private DragonHead _head;
    [SerializeField] private DragonPart _firstPart;
    [SerializeField] private DragonPart _tail;
    [SerializeField] private GameObject _headPrefab;
    [SerializeField] private GameObject _partPrefab;
    [SerializeField] private Transform _testSpawnPosition;
    [SerializeField] private float _bodyPartsCount;
    [SerializeField] private float _bodyPartsStep;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _moveSpeedLastWaypoint;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private List<DragonWaypoint> _wayPoints = new List<DragonWaypoint>();

    private List<DragonPart> _dragonParts = new List<DragonPart>();
    public DragonWaypoint CurrentWaypoint { get; private set; }
    private int _currentWaypointIndex = 0;

    [Header("Ragdoll")]
    [SerializeField] private float _partRagdollDelay;
    [SerializeField] private float _partRagdollForce;
    [SerializeField] private LayerMask _newRagdollLayer;

    private void Start()
    {
        Spawn(_testSpawnPosition.position);
    }
    private void OnEnable()
    {
        DragonWaypoint.WayPointReached += OnWayPointReached;
        DragonWaypoint.LastWayPointReached += OnLastWayPointReached;
    }
    private void OnDisable()
    {
        DragonWaypoint.WayPointReached -= OnWayPointReached;
        DragonWaypoint.LastWayPointReached -= OnLastWayPointReached;
    }
    private void OnValidate()
    {
        foreach (DragonPart DragonPart in _dragonParts)
        {
            DragonPart.FollowOffset = _bodyPartsStep;
            DragonPart.MoveSpeed = _moveSpeed;
            DragonPart.RotationSpeed = _rotationSpeed;
        }
    }
    private void SetWayPoints(List<DragonWaypoint> wayPoints)
    {
        _wayPoints = wayPoints;
        if (_wayPoints.Count == 0)
            Move = false;
        else
            CurrentWaypoint = _wayPoints[_currentWaypointIndex];
        foreach (DragonWaypoint dragonWaypoint in _wayPoints)
        {
            dragonWaypoint.Controller = this;
        }
    }
    private void Spawn(Vector3 headPosition)
    {
        bool moveOnStart = Move;
        bool rotateOnStart = Rotate;
        Move = false;
        Rotate = false;
        _head =  Instantiate(_headPrefab, headPosition, Quaternion.identity, transform).GetComponent<DragonHead>();
        _head.SetUp(_moveSpeed, _rotationSpeed, _bodyPartsStep, this);
        _dragonParts.Add(_head);
        float step = _bodyPartsStep;
        for (int i = 0; i < _bodyPartsCount; i++)
        {
            Vector3 newPartPos = -_head.transform.forward * step * (i + 1) + _head.transform.position;
            DragonPart part = Instantiate(_partPrefab, newPartPos, Quaternion.identity, transform).GetComponent<DragonPart>();
            part.SetUp(_moveSpeed, _rotationSpeed, _bodyPartsStep, _dragonParts[i], this);
            _dragonParts.Add(part);
        }
        _firstPart = _dragonParts[1];
        _tail = _dragonParts[_dragonParts.Count - 1];
        _firstPart.IgnorePreviousPartScale = true;
        Move = moveOnStart;
        Rotate = rotateOnStart;

        SetWayPoints(_wayPoints);
    }
    public void SetHeadSpeed(float _moveSpeed)
    {
        _head.MoveSpeed = _moveSpeed;
    }
    public void Go()
    {
        Move = true;
        Rotate = true;
        CopyScale = true;
        RotationModeCopy = false;
        _firstPart.IgnorePreviousPart = false;
        _tail.IgnorePreviousPart = false;
        SetFirstPartDefaulScale();
    }
    public void SetFirstPartRotationX(float x) 
    {
        _firstPart.IgnorePreviousPart = true;
        RotationModeCopy = true;
        CopyRotationX = true;
        Move = false;
        _firstPart.SetTargetRotationX(x);
        
    }
    public void SetFirstPartRotation(Vector3 Euler) 
    {
        _firstPart.IgnorePreviousPart = true;
        RotationModeCopy = true;
        CopyRotationX = false;
        Move = false;
        _firstPart.SetTargetRotation(Euler);
        
    }
    public void SetFirstPartScale(Vector3 TargetScale) 
    {
        _firstPart.IgnorePreviousPart = true;
        Move = false;
        CopyScale = true;
        _firstPart.SetTargetScale(TargetScale);
    }
    public void SetFirstPartDefaulScale() 
    {
        _firstPart.SetDefaultScale();
    }
    public void EnableColliders()
    {
        foreach (DragonPart part in _dragonParts)
        {
            part.BoxCollider.enabled = true;
            part.ResetColliderSize();
        }
    }
    public void DisableColliders()
    {
        foreach (DragonPart part in _dragonParts)
        {
            part.BoxCollider.enabled = false;
        }
    }
    public async UniTaskVoid EnableColliders(int delayMS)
    {
        await UniTask.Delay(delayMS);
        foreach (DragonPart part in _dragonParts)
        {
            part.BoxCollider.enabled = true;
            part.ResetColliderSize();
        }
    }
    public async UniTaskVoid EnableRagdoll()
    {
        float timer = 0f;
        for (int i = _dragonParts.Count - 1; i >= 0; i--)
        {
            while (timer < _partRagdollDelay)
            {
                timer += Time.deltaTime;
                await UniTask.NextFrame();
            }

            timer = 0f;
            Vector3 dir = RandomNormalizedDirection();
            _dragonParts[i].enabled = false;
            _dragonParts[i].Rigidbody.constraints = RigidbodyConstraints.None;
            _dragonParts[i].BoxCollider.enabled = true;
            _dragonParts[i].ResetColliderSize();
            int layerIndex = (int)Mathf.Log(_newRagdollLayer.value, 2);
            _dragonParts[i].gameObject.layer = layerIndex;
            _dragonParts[i].Item.enabled = true;
            //_dragonParts[i].Rigidbody.useGravity = true;
            _dragonParts[i].Rigidbody.isKinematic = false;
            _dragonParts[i].Rigidbody.AddForce(dir * _partRagdollForce);
            _dragonParts[i].Rigidbody.AddTorque(dir * _partRagdollForce);
        }
    }
    private Vector3 RandomNormalizedDirection()
    {
        return new Vector3(
                   Random.Range(-1f, 1f),
                   Random.Range(-1f, 1f),
                   Random.Range(-1f, 1f));
    }
    private void OnWayPointReached()
    {
        if (Loop) _currentWaypointIndex = (_currentWaypointIndex + 1) % _wayPoints.Count;
        else _currentWaypointIndex++;

        CurrentWaypoint = _wayPoints[_currentWaypointIndex];
        if (!Loop && LastWaypoint) SetHeadSpeed(_moveSpeedLastWaypoint);
    }
    private void OnLastWayPointReached()
    {
        EnableRagdoll().Forget();
    }
}
