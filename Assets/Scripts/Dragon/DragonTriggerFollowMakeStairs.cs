using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DragonTriggerFollowMakeStairs : DragonTrigger
{
    [SerializeField] List<DragonWaypoint> _newWayPoints = new();
    [SerializeField] float _newStep;
    protected override void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            _dragonController.SetBodyPartStep(_newStep);
            _dragonController.SetWayPoints(_newWayPoints);
            _dragonController.Go();
        }
    }
    private void OnEnable()
    {
        _dragonController.DragonReachedLastWayPoint += OnDragonReachedLastWayPoint;
    }
    private void OnDisable()
    {

        _dragonController.DragonReachedLastWayPoint -= OnDragonReachedLastWayPoint;
    }
    private void OnDragonReachedLastWayPoint()
    {
        _dragonController.SetFirstPartRotation(_newRotation);
        _dragonController.SetFirstPartScale(_newScale);
        _dragonController.EnableColliders(3000).Forget();
    }
    protected override void OnTriggerExit(Collider other) { }
}
