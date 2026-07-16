using UnityEngine;

public class PlayZoneChecker : MonoBehaviour
{
    [field: SerializeField] public bool NotEnoughItems { get { return _currentItemCount < _itemsCap; } }
    [SerializeField] private int _currentItemCount = 5;
    [SerializeField] private int _itemsCap = 5;
    [SerializeField] private LayerMask _interactMask;
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            _currentItemCount++;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            _currentItemCount--;
        }
    }
}
