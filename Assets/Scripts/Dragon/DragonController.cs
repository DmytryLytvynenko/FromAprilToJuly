using UnityEngine;
using System.Collections.Generic;

public class DragonController : MonoBehaviour
{
    [field:SerializeField] public bool Loop { get; set; } = false;
    [field:SerializeField] public bool CopyTail { get; set; } = false;
    [field:SerializeField] public bool CopyFirstPart { get; set; } = true;
    [field:SerializeField] public bool LookAtPlayer { get; set; } = true;
    [field:SerializeField] public bool Move { get; set; } = false;
    [field:SerializeField] public bool Rotate { get; set; } = false;
    [field:SerializeField] public bool RotationModeCopy { get; set; } = false;
    [field:SerializeField] public bool CopyRotationX { get; set; } = false;
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
    private bool _moveOnStart;
    private bool _rotateOnStart;

    private void Start()
    {
        Spawn(_testSpawnPosition.position);
    }
    private void OnEnable()
    {
        DragonWaypoint.WayPointReached += OnWayPointReached;
    }
    private void OnDisable()
    {
        DragonWaypoint.WayPointReached -= OnWayPointReached;
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
        SetWayPoints(_wayPoints);
        
        _moveOnStart = Move;
        _rotateOnStart = Rotate;
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
        Move = _moveOnStart;
        Rotate = _rotateOnStart;
    }
    public void SetHeadSpeed(float _moveSpeed)
    {
        _head.MoveSpeed = _moveSpeed;
    }
    public void Go()
    {
        Move = true;
        Rotate = true;
        RotationModeCopy = false;
        _firstPart.IgnorePreviousPart = false;
        _tail.IgnorePreviousPart = false;
    }
    public void SetFirstPartRotation(Vector3 Euler, bool RotateAroundX) 
    {
        _firstPart.SetTargetRotation(Euler);
        _firstPart.IgnorePreviousPart = true;
        RotationModeCopy = true;
        CopyRotationX = RotateAroundX;
        Move = false;
    }
    private void OnWayPointReached()
    {
        if (Loop) _currentWaypointIndex = (_currentWaypointIndex + 1) % _wayPoints.Count;
        else _currentWaypointIndex++;

        CurrentWaypoint = _wayPoints[_currentWaypointIndex];
        if (!Loop && LastWaypoint) SetHeadSpeed(_moveSpeedLastWaypoint);
    }

}
