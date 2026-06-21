using System;
using UnityEngine;

public class DragonWaypoint : MonoBehaviour
{
    public static event Action WayPointReached;
    public static event Action LastWayPointReached;
    public DragonController Controller { get; set; }
    [SerializeField] private LayerMask _interactMask;
    [SerializeField] private Collider _collider;

    private void OnTriggerEnter(Collider other)
    {
        if (!Controller.CurrentWaypoint == this) return;

        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            if (!Controller.Loop)
            {
                _collider.enabled = false;
                if (Controller.LastWaypoint)
                {
                    LastWayPointReached?.Invoke();
                    return;
                }
            }
            WayPointReached?.Invoke();
        }
    }
}
