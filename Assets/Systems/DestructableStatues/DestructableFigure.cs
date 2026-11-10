using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DestructableFigures
{
    public class DestructableFigure : MonoBehaviour
    {
        [SerializeField] private Transform _center;
        [SerializeField] private DestructablePart[] parts;

        [Header("Destruction")]
        [SerializeField] private float _explosionForce = 50f;
        [SerializeField] private float _explosionRadius = 10f;
        [SerializeField] private float _upwardsModifier = 0f;
        [SerializeField] private FigureDestructionMethod destructionMethod = FigureDestructionMethod.Explode;

        [Header("Assembly")]
        [SerializeField] private float _delayBetweenParts = 0.1f;
        [SerializeField] private bool _sequentialAssembling = true;
        [SerializeField] private bool _topToBottom = true;
        [SerializeField] private bool _bottomToTop = false;
        [SerializeField] private bool _enableColliders = true;

        public bool _destructed;
        public bool _isAssembling;

        public bool IsAssembled => parts.All(p => p.IsAssembled);

        public void SetParts(DestructablePart[] _parts) => parts = _parts;

        private void Awake()
        {
            InitializeParts();
            if (_topToBottom) SortTopToBottom();
        }

        private void SortTopToBottom()
        {
            parts = parts
                .Select(p => (part: p, pos: transform.InverseTransformPoint(p.transform.position)))
                .OrderByDescending(t => Mathf.Round(t.pos.y * 100f))
                .ThenBy(t => Mathf.Round(t.pos.x * 100f))
                .ThenBy(t => Mathf.Round(t.pos.z * 100f))
                .Select(t => t.part)
                .ToArray();
        }


        public void InitializeParts()
        {
            foreach (var part in parts)
                part.Initialize(transform, _enableColliders);
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

            if (!_destructed) DestroyFigure();
            else AssembleAsync(destroyCancellationToken).Forget();
        }

        private void DestroyFigure()
        {
            _destructed = true;

            switch (destructionMethod)
            {
                case FigureDestructionMethod.Explode:
                    Explode();
                    break;
                case FigureDestructionMethod.Drop:
                    Drop();
                    break;
                case FigureDestructionMethod.Disassemble:
                    break;
                default:
                    break;
            }


        }

        private void Explode()
        {
            foreach (var part in parts)
            {
                part.Release();
                part.Rigidbody.AddExplosionForce(_explosionForce, _center.position, _explosionRadius, _upwardsModifier);
            }
        }
        private void Drop()
        {
            foreach (var part in parts)
            {
                part.Release();
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
    public enum FigureDestructionMethod
    {
        Explode,
        Drop,
        Disassemble
    }
}