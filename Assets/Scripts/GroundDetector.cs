using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GroundDetector : MonoBehaviour
{
    public bool Grounded {get; private set;} = true;

    [SerializeField] private LayerMask _groundedLayers;
    [SerializeField] private float _jumpRayCheckDistance = 1.05f;

    private PlayerMovement _playerMovement;
    private NormalProjector _normalProjector;
    public void Initialize(PlayerMovement playerMovement, NormalProjector normalProjector)
    {
        _normalProjector = normalProjector;
        _playerMovement = playerMovement;
        gameObject.SetActive(true);
        _playerMovement.PlayerJumped += OnJump;
    }
    public void HandleDisable()
    {
        _playerMovement.PlayerJumped -= OnJump;
    }
    private void OnJump()
    {
        Grounded = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsGroundLayer(other.gameObject)) return;
        
        if(_normalProjector.SlopeIsWalkable)
        {
            Grounded = true;
        }
        else
        {
            Grounded = CheckGround();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!Grounded && IsGroundLayer(other.gameObject))
        {
            Grounded = false;
            return;
        }
        else if(!CheckGround())
        {
            Grounded = false;
        }
    }
    private bool IsGroundLayer(GameObject obj)
    {
        return ((1 << obj.layer) & _groundedLayers.value) != 0;
    }
    private bool CheckGround()
    {
        Debug.DrawRay(_playerMovement.transform.position, Vector3.down * _jumpRayCheckDistance, Color.blue, 5f);
        if (Physics.Raycast(_playerMovement.transform.position, Vector3.down, out RaycastHit hit, _jumpRayCheckDistance, _groundedLayers))
        {
            return true;

        }
        else
        {
            return false;
        }
    }
}
