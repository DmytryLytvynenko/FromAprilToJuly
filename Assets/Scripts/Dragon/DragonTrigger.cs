using UnityEngine;

public class DragonTrigger : MonoBehaviour
{
    [field: SerializeField] public bool Loop { get; private set; }
    [SerializeField] protected DragonController _dragonController;
    [SerializeField] protected Vector2 _newRotation;
    [SerializeField] protected Vector3 _newScale;
    [SerializeField] protected LayerMask _interactMask;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            //_dragonController.SetFirstPartRotationX(_testRotation.x);
            _dragonController.SetFirstPartRotation(_newRotation);
            _dragonController.SetFirstPartScale(_newScale);
            _dragonController.EnableColliders(3000).Forget();
        }
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & _interactMask.value) != 0)
        {
            _dragonController.Go();
        }
    }
}
