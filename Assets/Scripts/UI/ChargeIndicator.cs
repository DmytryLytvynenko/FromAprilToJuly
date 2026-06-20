using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ChargeIndicator : MonoBehaviour
{
    [SerializeField] private Image _filling;
    [SerializeField] private float _shakeRange = .5f;
    [SerializeField] private float _shakeRate = 0.1f;
    [SerializeField] private float _indicatorShowTime;
    [SerializeField] private float _waitBeforeShow = .2f;
    [SerializeField] private AnimationCurve _shakeRateUponTime;
    [SerializeField] private AnimationCurve _indicatorShowCurve;

    private Transform _indicator;
    private Interact _interact;
    private Vector3 _startPos;
    private float _shakeTimer = 0f;
    private float _currentPercentage = 0f;
    private bool _fillPercentage = false;
    private CancellationTokenSource _indicatorShowSource;
    public void Initialize(Interact interact)
    {
        _indicator = transform;
        _startPos = transform.localPosition;
        _interact = interact;
        _interact.ItemThrown += OnItemThrown;
        _interact.ChargeTimerStarted += OnChargeTimerStarted;
    }
    public void HandleDisable()
    {
        _interact.ItemThrown -= OnItemThrown;
        _interact.ChargeTimerStarted -= OnChargeTimerStarted;
    }
    private void Update()
    {
        if (!_fillPercentage) return;

        SetFillingPercentage();
        Shake();
    }

    private void SetFillingPercentage()
    {
        _currentPercentage = _interact.GetThrowChargePercentage();
        _filling.fillAmount = _currentPercentage;
    }

    private void Shake()
    {
        _shakeTimer += Time.deltaTime;
        if (_shakeTimer > _shakeRate)
        {
            float shakeRange = _shakeRateUponTime.Evaluate(_currentPercentage) * _shakeRange;
            _shakeTimer = 0;
            transform.localPosition = _startPos + new Vector3(Random.Range(-shakeRange, shakeRange), Random.Range(-shakeRange, shakeRange), 0);
        }
    }
    private void OnItemThrown()
    {
        _fillPercentage = false;
        transform.localPosition = _startPos;
        _indicatorShowSource?.Cancel();
        _indicatorShowSource?.Dispose();

        _indicatorShowSource = new CancellationTokenSource();
        HideIndicator(_indicatorShowSource.Token).Forget();
    }
    private void OnChargeTimerStarted()
    {
        _fillPercentage = true;
        _indicatorShowSource?.Cancel();
        _indicatorShowSource?.Dispose();

        _indicatorShowSource = new CancellationTokenSource();
        ShowIndicator(_indicatorShowSource.Token).Forget();
    }
    private async UniTaskVoid ShowIndicator(CancellationToken ct)
    {
        await UniTask.Delay((int)(_waitBeforeShow * 1000));

        float expiredTime = 0f;
        float progress = 0f;
        while (progress < 1f) 
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = expiredTime / _indicatorShowTime;
            _indicator.localScale = Vector3.one * _indicatorShowCurve.Evaluate(progress);

            await UniTask.NextFrame(ct);
        }
    }
    private async UniTaskVoid HideIndicator(CancellationToken ct)
    {
        float progress = _indicator.localScale.x / 1f;
        float expiredTime = _indicatorShowTime - _indicatorShowTime * progress;
        while (progress > 0f) 
        {
            if (ct.IsCancellationRequested) return;
            expiredTime += Time.deltaTime;
            progress = 1 - (expiredTime / _indicatorShowTime);
            _indicator.localScale = Vector3.one * _indicatorShowCurve.Evaluate(progress);

            await UniTask.NextFrame(ct);
        }
    }
}
