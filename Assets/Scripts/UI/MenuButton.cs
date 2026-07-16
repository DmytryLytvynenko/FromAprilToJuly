using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private Image _underline;
    [SerializeField] private float _indicatorShowTime;
    [SerializeField] private AnimationCurve _underlineShowCurve;

    private CancellationTokenSource _underlineShowSource;

    public void OnHover()
    {
        _underlineShowSource?.Cancel();
        _underlineShowSource?.Dispose();

        _underlineShowSource = new CancellationTokenSource();
        ShowUnderline(_underlineShowSource.Token).Forget();
    }
    public void OnExit()
    {
        _underlineShowSource?.Cancel();
        _underlineShowSource?.Dispose();

        _underlineShowSource = new CancellationTokenSource();
        HideUnderline(_underlineShowSource.Token).Forget();
    }
    private async UniTaskVoid ShowUnderline(CancellationToken ct)
    {

        float expiredTime = 0f;
        float progress = 0f;
        while (progress < 1f)
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = expiredTime / _indicatorShowTime;
            _underline.fillAmount = _underlineShowCurve.Evaluate(progress);

            await UniTask.NextFrame(ct);
        }
    }
    private async UniTaskVoid HideUnderline(CancellationToken ct)
    {
        float progress = _underline.fillAmount / 1f;
        float expiredTime = _indicatorShowTime - _indicatorShowTime * progress;
        while (progress > 0f)
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = 1 - (expiredTime / _indicatorShowTime);
            _underline.fillAmount = _underlineShowCurve.Evaluate(progress);

            await UniTask.NextFrame(ct);
        }
    }
}
