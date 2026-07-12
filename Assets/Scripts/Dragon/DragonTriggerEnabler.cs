using UnityEngine;

public class DragonTriggerEnabler : DragonTrigger
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            _dragonController.Go();
        }
    }
    protected override void OnTriggerExit(Collider other) { }
}
