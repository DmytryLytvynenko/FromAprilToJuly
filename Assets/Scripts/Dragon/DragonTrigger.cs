using UnityEngine;

public class DragonTrigger : MonoBehaviour
{
    [field: SerializeField] public bool Loop { get; private set; }
    [SerializeField] private DragonController _dragonController;
    [SerializeField] private Vector2 _testRotation;
    [SerializeField] private LayerMask _interactMask;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)

        _dragonController.SetFirstPartRotation(_testRotation, Mathf.Abs(_testRotation.x) > 0);
    }
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)

        _dragonController.Go();
    }
}
