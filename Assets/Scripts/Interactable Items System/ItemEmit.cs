using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class ItemEmit : Item
{
    private enum ShaderProperty
    {
        _PatternTransparancy
    }

    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material patternMaterial;
    [SerializeField] private float _patternShowTime = 1f;
    [SerializeField] private AnimationCurve _patternAnimCurve;

    private Renderer m_renderer;
    private CancellationTokenSource _cancellationTokenSource;

    protected override void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        base.Start();
        m_renderer = GetComponent<Renderer>();
    }
    private async UniTaskVoid ShowPattern(CancellationToken ct)
    {
        m_renderer.sharedMaterial = patternMaterial;
        string shaderProperty = ShaderProperty._PatternTransparancy.ToString();
        float expiredTime = 0f;
        float progress = 0f;
        while (progress < 1f)
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = expiredTime / _patternShowTime;
            patternMaterial.SetFloat(shaderProperty, _patternAnimCurve.Evaluate(progress));

            await UniTask.NextFrame(ct);
        }
        patternMaterial.SetFloat(shaderProperty, 1f);
    }
    private async UniTaskVoid HidePattern(CancellationToken ct)
    {
        string shaderProperty = ShaderProperty._PatternTransparancy.ToString();
        float expiredTime = 0f;
        float progress = 0f;
        while (progress < 1f)
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = expiredTime / _patternShowTime;
            patternMaterial.SetFloat(shaderProperty, _patternAnimCurve.Evaluate(1f - progress));

            await UniTask.NextFrame(ct);
        }
        patternMaterial.SetFloat(shaderProperty, 0f);
        m_renderer.sharedMaterial = baseMaterial;
    }
    public override void Highlight()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        ShowPattern(_cancellationTokenSource.Token).Forget();
    }
    public override void RemoveHighlight()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        HidePattern(_cancellationTokenSource.Token).Forget();
    }
}
