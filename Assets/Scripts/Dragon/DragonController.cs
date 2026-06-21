using UnityEngine;
using System.Collections.Generic;

public class DragonController : MonoBehaviour
{
    [field:SerializeField] bool Loop { get; set; } = false;
    [field:SerializeField] bool CopyTail { get; set; } = false;
    [field:SerializeField] bool CopyFirstPart { get; set; } = true;
    [field:SerializeField] bool LookAtPlayer { get; set; } = true;
    [field:SerializeField] bool Move { get; set; } = false;
    [SerializeField] DragonPart _head;
    [SerializeField] DragonPart _firstPart;
    [SerializeField] DragonPart _tail;
    [SerializeField] float _bodyPartsCount;
    [SerializeField] float _bodyPartsStep;
    [SerializeField] List<Transform> _wayPoints;

    private List<DragonPart> _dragonParts = new List<DragonPart>();

    private void SetWayPoints(List<Transform> wayPoints)
    {
        _wayPoints = wayPoints;
    }
    private void Spawn(Vector3 headPosition)
    {

    }
    private void Go()
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
