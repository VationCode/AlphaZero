using System.Collections;
using alpha.ingame.player;
using alpha.ingame.skill;
using UnityEngine;
using UnityEngine.UI;

/// <summary>스킬 슬롯의 아이콘과 남은 쿨다운을 기존 UI 이미지에 표시한다.</summary>
public sealed class SkillUI : MonoBehaviour
{
    [SerializeField] private PlayerCore _playerCore;
    [SerializeField] private RectTransform _slotsRoot;

    private readonly Image[] _icons = new Image[4];
    private readonly Image[] _fills = new Image[4];
    private readonly Coroutine[] _fillRoutines = new Coroutine[4];
    private readonly SkillSlotDisplayState[] _slotStates = new SkillSlotDisplayState[4];

    private void Awake()
    {
        if (_playerCore == null) _playerCore = FindFirstObjectByType<PlayerCore>();
        if (_slotsRoot == null) return;

        for (int i = 0; i < 4; i++)
        {
            Transform slot = _slotsRoot.Find($"Skill{i + 1}");
            _icons[i] = slot?.Find("Icon")?.GetComponent<Image>();
            _fills[i] = slot?.Find("Fill")?.GetComponent<Image>();

            if (_icons[i] == null || _fills[i] == null)
                Debug.LogWarning($"{nameof(SkillUI)}: Skill{i + 1}의 Icon 또는 Fill을 찾을 수 없습니다.", this);
        }
    }

    private void OnEnable()
    {
        // 구독 즉시 채널이 네 슬롯의 최신 상태를 재전달한다.
        _playerCore?.SkillDisplayChannel.Subscribe(HandleSlotChanged);
    }

    private void OnDisable()
    {
        _playerCore?.SkillDisplayChannel.Unsubscribe(HandleSlotChanged);
        for (int i = 0; i < _fillRoutines.Length; i++)
        {
            if (_fillRoutines[i] == null) continue;
            StopCoroutine(_fillRoutines[i]);
            _fillRoutines[i] = null;
        }
    }

    private void HandleSlotChanged(SkillSlotDisplayState state)
    {
        if (state.Slot < 1 || state.Slot > 4) return;
        int index = state.Slot - 1;
        _slotStates[index] = state;
        RefreshSlot(index);
    }

    private void RefreshSlot(int index)
    {
        SkillSlotDisplayState state = _slotStates[index];
        Image icon = _icons[index];
        if (icon != null)
        {
            // 데이터가 없는 슬롯은 배경과 번호만 남긴다.
            icon.sprite = state.Icon;
            icon.enabled = state.Icon != null;
        }

        if (_fillRoutines[index] != null)
        {
            StopCoroutine(_fillRoutines[index]);
            _fillRoutines[index] = null;
        }

        Image fill = _fills[index];
        if (fill == null) return;

        // 재활성화 시에도 남은 시간으로 Fill을 복원한다.
        float amount = GetFillAmount(state);
        fill.fillAmount = amount;
        if (amount > 0f)
            _fillRoutines[index] = StartCoroutine(AnimateFill(index));
    }

    private IEnumerator AnimateFill(int index)
    {
        Image fill = _fills[index];
        while (true)
        {
            yield return null;
            float amount = GetFillAmount(_slotStates[index]);
            fill.fillAmount = amount;
            if (amount <= 0f) break;
        }
        _fillRoutines[index] = null;
    }

    private static float GetFillAmount(SkillSlotDisplayState state)
    {
        if (state.CooldownDuration <= 0f) return 0f;
        // 사용 직후 1, 쿨다운 종료 시 0으로 감소한다.
        return Mathf.Clamp01((state.CooldownEndTime - Time.time) / state.CooldownDuration);
    }
}
