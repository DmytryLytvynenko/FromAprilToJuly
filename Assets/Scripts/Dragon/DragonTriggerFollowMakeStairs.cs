using System.Collections.Generic;
using UnityEngine;

public class DragonTriggerFollowMakeStairs : DragonTrigger
{
    [SerializeField] List<DragonWaypoint> _newWayPoints = new();
    [SerializeField] float _newStep;
    protected override void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        base.OnTriggerEnter(other);

        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            _dragonController.SetBodyPartStep(_newStep);
            _dragonController.SetWayPoints(_newWayPoints);
            _dragonController.Go();
            _dragonController.DragonReachedLastWayPoint += OnDragonReachedLastWayPoint;
        }
    }
    private void OnDragonReachedLastWayPoint()
    {
        _dragonController.DragonReachedLastWayPoint -= OnDragonReachedLastWayPoint;

        _dragonController.SetFirstPartRotation(_newRotation);
        _dragonController.SetFirstPartScale(_newScale);
        _dragonController.EnableColliders(3000).Forget();
    }
    protected override void OnTriggerExit(Collider other) { }
}
