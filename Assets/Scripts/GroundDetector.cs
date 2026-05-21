using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GroundDetector : MonoBehaviour
{
    public bool Grounded {get; private set;} = true;

    [SerializeField] private LayerMask _groundedLayers;
    [SerializeField] private float _jumpRayCheckDistance = 1.05f;

    private PlayerMovement _playerMovement;
    public void Initialize(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;
        gameObject.SetActive(true);
    }
    private void OnEnable()
    {
        _playerMovement.PlayerJumped += OnJump;
    }
    private void OnDisable()
    {
        _playerMovement.PlayerJumped -= OnJump;
    }
    private void OnJump()
    {
        Grounded = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsGroundLayer(other.gameObject))
        {
            Grounded = true;
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
        Debug.DrawRay(_playerMovement.transform.position, Vector3.down * _jumpRayCheckDistance, Color.blue, 0.5f);
        if (Physics.Raycast(_playerMovement.transform.position, Vector3.down, _jumpRayCheckDistance, _groundedLayers))
            return true;
        else
            return false;
    }
}
