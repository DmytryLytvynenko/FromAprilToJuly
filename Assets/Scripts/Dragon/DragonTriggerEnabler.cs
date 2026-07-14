using UnityEngine;

public class DragonTriggerEnabler : DragonTrigger
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        base.OnTriggerEnter(other);

        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            OnTriggerEnterRoutine();
        }
    }

    private async void OnTriggerEnterRoutine()
    {
        await _dragonController.Spawn();
        _dragonController.Go();
        _dragonController.DragonReachedLastWayPoint += OnDragonReachedLastWayPoint;
    }

    private void OnDragonReachedLastWayPoint()
    {
        _dragonController.DragonReachedLastWayPoint -= OnDragonReachedLastWayPoint;
        _dragonController.Move = false;
    }
    protected override void OnTriggerExit(Collider other) { }
}
