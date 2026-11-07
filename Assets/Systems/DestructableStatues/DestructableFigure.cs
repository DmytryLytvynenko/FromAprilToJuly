using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class DestructableFigure : MonoBehaviour
{
    [SerializeField] private Transform _center;
    [SerializeField] private DestructablePart[] parts;

    [Header("Destruction")]
    [SerializeField] private float _explosionForce = 50f;
    [SerializeField] private float _explosionRadius = 10f;
    [SerializeField] private float _upwardsModifier = 0f;

    [Header("Assembly")]
    [SerializeField] private float _delayBetweenParts = 0.1f;
    [SerializeField] private bool _sequentialAssembling = true;

    public bool _destructed;
    public bool _isAssembling;

    public bool IsAssembled => parts.All(p => p.IsAssembled);

    private void Awake()
    {
        foreach (var part in parts)
            part.Initialize(transform);
    }
    private void Start()
    {
        if (!_center) Debug.LogError("Center is null in DestructableFigure: " + gameObject.name);
    }

    private void OnEnable() => InputManager.Instance.OnJump.AddListener(HandleSpacebarInput);
    private void OnDisable() => InputManager.Instance.OnJump.RemoveListener(HandleSpacebarInput);

    private void HandleSpacebarInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || _isAssembling) return;

        if (!_destructed) Explode();
        else AssembleAsync(destroyCancellationToken).Forget();
    }

    private void Explode()
    {
        _destructed = true;

        foreach (var part in parts)
        {
            part.Release();
            part.Rigidbody.AddExplosionForce(_explosionForce, _center.position, _explosionRadius, _upwardsModifier);
        }
    }

    private async UniTask AssembleAsync(CancellationToken ct)
    {
        _isAssembling = true;
        try
        {
            if (_sequentialAssembling) await AssembleSequential(ct);
            else await AssembleStaggered(ct);

            _destructed = false;
        }
        finally
        {
            _isAssembling = false;
        }
    }

    private async UniTask AssembleSequential(CancellationToken ct)
    {
        foreach (var part in parts)
            await part.Assemble(ct);
    }

    private async UniTask AssembleStaggered(CancellationToken ct)
    {
        var tasks = new UniTask[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            tasks[i] = parts[i].Assemble(ct);
            await UniTask.Delay(Mathf.RoundToInt(_delayBetweenParts * 1000), cancellationToken: ct);
        }

        await UniTask.WhenAll(tasks);
    }
}