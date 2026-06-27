using UnityEngine;

public class DragonTrigger : MonoBehaviour
{
    [field: SerializeField] public bool Loop { get; private set; }
    [SerializeField] private DragonController _dragonController;
    [SerializeField] private Vector2 _testRotation;
    [SerializeField] private Vector3 _testScale;
    [SerializeField] private LayerMask _interactMask;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            //_dragonController.SetFirstPartRotationX(_testRotation.x);
            _dragonController.SetFirstPartRotation(_testRotation);
            _dragonController.SetFirstPartScale(_testScale);
            _dragonController.EnableColliders(3000).Forget();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            _dragonController.Go();
        }
    }
}
