using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class ItemEmit : Item
{
    private static readonly int PatternTransparencyId = Shader.PropertyToID("_PatternTransparancy");

    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material patternMaterial;
    [SerializeField] private float _patternShowTime = 1f;
    [SerializeField] private AnimationCurve _patternAnimCurve;

    private MaterialPropertyBlock _propBlock;
    private CancellationTokenSource _cancellationTokenSource;
    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }
    protected override void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        base.Start();
    }

    private void SetPatternValue(float value)
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(PatternTransparencyId, value);
        _renderer.SetPropertyBlock(_propBlock);
    }

    private async UniTaskVoid ShowPattern(CancellationToken ct)
    {
        _renderer.sharedMaterial = patternMaterial;
        float expiredTime = 0f;
        float progress = 0f;
        while (progress < 1f)
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = expiredTime / _patternShowTime;
            SetPatternValue(_patternAnimCurve.Evaluate(progress));

            await UniTask.NextFrame(ct);
        }
        SetPatternValue(1f);
    }

    private async UniTaskVoid HidePattern(CancellationToken ct)
    {
        float expiredTime = 0f;
        float progress = 0f;
        while (progress < 1f)
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = expiredTime / _patternShowTime;
            SetPatternValue(_patternAnimCurve.Evaluate(1f - progress));

            await UniTask.NextFrame(ct);
        }
        SetPatternValue(0f);
        _renderer.sharedMaterial = baseMaterial;
    }

    public override void Highlight()
    {
        if (!enabled) return;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        ShowPattern(_cancellationTokenSource.Token).Forget();
    }
    public override void RemoveHighlight()
    {
        if (!enabled) return;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        HidePattern(_cancellationTokenSource.Token).Forget();
    }
}