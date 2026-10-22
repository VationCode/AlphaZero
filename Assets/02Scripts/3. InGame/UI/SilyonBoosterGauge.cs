using System.Collections;
using alpha;
using alpha.ingame.player;
using UnityEngine;
using UnityEngine.UI;

/// <summary>실리온 부스터 잔량을 8칸의 이미지로 표시한다.</summary>
public sealed class SilyonBoosterGauge : MonoBehaviour
{
    [SerializeField] private PlayerCore _playerCore;
    [SerializeField] private LocomotionModule _locomotion;
    [SerializeField] private Image[] _segments;
    [SerializeField, Min(0f)] private float _visibleAfterBoost = 1.5f;
    [SerializeField, Min(0f)] private float _fadeDuration = 0.5f;

    private float[] _originalAlphas;
    private Coroutine _hideRoutine;
    private GameObject _gaugeObject;

    private void Awake()
    {
        // 현재 배치된 각 칸의 알파값을 부스터 재시작 시 복원한다.
        _originalAlphas = new float[_segments?.Length ?? 0];
        for (int i = 0; i < _originalAlphas.Length; i++)
            if (_segments[i] != null) _originalAlphas[i] = _segments[i].color.a;

        // 첫 번째 칸의 두 단계 부모가 현재 배치의 Gauge 객체다.
        if (_originalAlphas.Length > 0 && _segments[0] != null)
        {
            Transform mobility = _segments[0].transform.parent;
            if (mobility != null && mobility.parent != null)
                _gaugeObject = mobility.parent.gameObject;
        }
    }

    private void OnEnable()
    {
        if (_playerCore != null) _playerCore.OnPlayerSetup += HandlePlayerSetup;
        if (_locomotion != null)
        {
            _locomotion.OnBoostRatioChanged += Refresh;
            _locomotion.OnBoostStateChanged += HandleBoostStateChanged;
            Refresh(_locomotion.BoostRatio);
        }

        if (_playerCore != null && _playerCore.CurrentArete != null)
            HandlePlayerSetup(_playerCore.CurrentAreteType);
        else HideGauge();
    }

    private void OnDisable()
    {
        StopHideRoutine();
        HideGauge();
        if (_playerCore != null) _playerCore.OnPlayerSetup -= HandlePlayerSetup;
        if (_locomotion != null)
        {
            _locomotion.OnBoostRatioChanged -= Refresh;
            _locomotion.OnBoostStateChanged -= HandleBoostStateChanged;
        }
    }

    private void HandlePlayerSetup(EAreteType type)
    {
        StopHideRoutine();
        if (type == EAreteType.Silyon && _locomotion != null && _locomotion.IsBoosting)
            ShowGauge();
        else
            HideGauge();
    }

    private void HandleBoostStateChanged(bool isBoosting)
    {
        StopHideRoutine();
        if (_playerCore == null || _playerCore.CurrentAreteType != EAreteType.Silyon)
        {
            HideGauge();
            return;
        }

        if (isBoosting) ShowGauge();
        else if (_gaugeObject != null && _gaugeObject.activeSelf)
            _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        if (_visibleAfterBoost > 0f)
            yield return new WaitForSeconds(_visibleAfterBoost);

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / _fadeDuration));
            yield return null;
        }

        HideGauge();
        _hideRoutine = null;
    }

    private void ShowGauge()
    {
        SetAlpha(1f);
        if (_gaugeObject != null) _gaugeObject.SetActive(true);
    }

    private void HideGauge()
    {
        if (_gaugeObject != null) _gaugeObject.SetActive(false);
    }

    private void SetAlpha(float fraction)
    {
        for (int i = 0; i < _originalAlphas.Length; i++)
        {
            Image image = _segments[i];
            if (image == null) continue;
            Color color = image.color;
            color.a = _originalAlphas[i] * fraction;
            image.color = color;
        }
    }

    private void StopHideRoutine()
    {
        if (_hideRoutine == null) return;
        StopCoroutine(_hideRoutine);
        _hideRoutine = null;
    }

    private void Refresh(float ratio)
    {
        if (_segments == null) return;

        // 1번부터 비우고, 충전할 때는 8번부터 다시 채운다.
        for (int i = 0; i < _segments.Length; i++)
        {
            if (_segments[i] != null)
                _segments[i].fillAmount = Mathf.Clamp01(ratio * _segments.Length - (_segments.Length - 1 - i));
        }
    }
}
