using UnityEngine;
using System.Collections.Generic;

public class DragonController : MonoBehaviour
{
    [field:SerializeField] public bool Loop { get; set; } = false;
    [field:SerializeField] public bool CopyTail { get; set; } = false;
    [field:SerializeField] public bool CopyFirstPart { get; set; } = true;
    [field:SerializeField] public bool LookAtPlayer { get; set; } = true;
    [field:SerializeField] public bool Move { get; set; } = false;

    [SerializeField] private DragonHead _head;
    [SerializeField] private DragonPart _firstPart;
    [SerializeField] private DragonPart _tail;
    [SerializeField] private GameObject _headPrefab;
    [SerializeField] private GameObject _partPrefab;
    [SerializeField] private Transform _testSpawnPosition;
    [SerializeField] private float _bodyPartsCount;
    [SerializeField] private float _bodyPartsStep;
    [SerializeField] private float _followSpeed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private List<DragonWaypoint> _wayPoints;

    private List<DragonPart> _dragonParts = new List<DragonPart>();

    private void Start()
    {
        Spawn(_testSpawnPosition.position);
    }
    private void OnValidate()
    {
        foreach (DragonPart DragonPart in _dragonParts)
        {
            DragonPart.FollowOffset = _bodyPartsStep;
            DragonPart.FollowSpeed = _followSpeed;
            DragonPart.RotationSpeed = _rotationSpeed;
        }
    }
    private void SetWayPoints(List<DragonWaypoint> wayPoints)
    {
        _wayPoints = wayPoints;
    }
    private void Spawn(Vector3 headPosition)
    {
        _head =  Instantiate(_headPrefab, headPosition, Quaternion.identity, transform).GetComponent<DragonHead>();
        _dragonParts.Add(_head);
        float step = _bodyPartsStep;
        for (int i = 0; i < _bodyPartsCount; i++)
        {
            Vector3 newPartPos = -_head.transform.forward * step * (i + 1) + _head.transform.position;
            DragonPart part = Instantiate(_partPrefab, newPartPos, Quaternion.identity, transform).GetComponent<DragonPart>();
            part.SetUp(_followSpeed, _rotationSpeed, _bodyPartsStep, _dragonParts[i], this);
            _dragonParts.Add(part);
        }
    }
    public void Go()
    {
        Move = true;
    }
    private void FollowWay()
    {
        //Head -> go forward
    }
    private void Rotate()
    {
        //Head -> rotate towards next waypoint
    }
    private void FixedUpdate()
    {
        if (!Move) return;

        FollowWay();
        Rotate();
    }
}
