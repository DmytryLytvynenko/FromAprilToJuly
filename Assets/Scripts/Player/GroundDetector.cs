using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GroundDetector : MonoBehaviour
{
    public event Action<bool> GroundedChanged;
    public event Action GroundedPositive;
    public bool Grounded {get; private set;} = true;
    public Transform GroundedOn {get; private set;} = null;

    [SerializeField] private LayerMask _groundedLayers;
    [SerializeField] private float _jumpRayCheckDistance = 1.05f;
    [SerializeField] private float _disableCheckOnJumpDelay = .2f;

    private PlayerMovement _playerMovement;
    private NormalProjector _normalProjector;
    private CancellationTokenSource _disableCheckSource;
    private bool _checkOnStay = true;
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
/*        _disableCheckSource?.Cancel();
        _disableCheckSource?.Dispose();
        _disableCheckSource = new CancellationTokenSource();*/
        DisableCheckOnJump(/*_disableCheckSource.Token*/).Forget();

        Grounded = false;
        GroundedChanged?.Invoke(Grounded);
    }
    private void OnTriggerEnter(Collider other)
    {
        bool GroundedBefore = Grounded;
/*        _disableCheckSource?.Cancel();
        _disableCheckSource?.Dispose();*/
        if (!IsGroundLayer(other.gameObject)) return;
        
        if(_normalProjector.SlopeIsWalkable)
        {
            Grounded = true;
            GroundedChanged?.Invoke(Grounded);
            if (GroundedBefore != Grounded) GroundedPositive?.Invoke();
            GroundedOn = other.transform;
        }
        else
        {
            Grounded = CheckGround();
            GroundedChanged?.Invoke(Grounded);
            if (GroundedBefore != Grounded) GroundedPositive?.Invoke();
            GroundedOn = Grounded ? other.transform : null;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!Grounded && IsGroundLayer(other.gameObject))
        {
            Grounded = false;
            GroundedChanged?.Invoke(Grounded);
            GroundedOn = null;
            return;
        }
        else if(!CheckGround())
        {
            Grounded = false;
            GroundedChanged?.Invoke(Grounded);
            GroundedOn = null;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!_checkOnStay) return;

        if (!IsGroundLayer(other.gameObject)) return;
        if (_normalProjector.SlopeIsWalkable)
        {
            Grounded = true;
            GroundedChanged?.Invoke(Grounded);
            GroundedOn = other.transform;
        }
        else
        {
            Grounded = CheckGround();
            GroundedChanged?.Invoke(Grounded);
            GroundedOn = Grounded ? other.transform : null;
        }
    }
    private bool IsGroundLayer(GameObject obj)
    {
        return ((1 << obj.layer) & _groundedLayers.value) != 0;
    }
    private bool CheckGround()
    {
        CheckGroundRay();
        if (Physics.Raycast(_playerMovement.transform.position, Vector3.down, out RaycastHit hit, _jumpRayCheckDistance, _groundedLayers))
        {
            return true;

        }
        else
        {
            return false;
        }
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private void CheckGroundRay()
    {
        UnityEngine.Debug.DrawRay(_playerMovement.transform.position, Vector3.down * _jumpRayCheckDistance, Color.blue, 5f);
    }

    private async UniTaskVoid DisableCheckOnJump(/*CancellationToken token*/)
    {
        _checkOnStay = false;
        float time = 0;
        while (time < _disableCheckOnJumpDelay)
        {
/*            if (token.IsCancellationRequested)
            {
                _checkOnStay = true;
                return;
            }*/
            time += Time.deltaTime;
            await UniTask.NextFrame();
        }
        _checkOnStay = true;
    }
}
