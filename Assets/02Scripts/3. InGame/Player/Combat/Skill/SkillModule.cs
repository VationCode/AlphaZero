using System;
using System.Collections.Generic;
using alpha.ingame.skill;
using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>1~4번 슬롯의 데이터와 설치형 스킬 실행을 연결한다.</summary>
    [RequireComponent(typeof(SkillStateFlow))]
    [DefaultExecutionOrder(100)]
    public sealed class SkillModule : MonoBehaviour
    {
        [SerializeField] private SkillData[] _slots = new SkillData[4];

        private readonly Dictionary<SkillData, float> _readyAt = new();
        private SkillStateFlow _flow;
        private PlayerCore _playerCore;
        private DeployableSkillData _activeSkill;
        private SkillPlacementPreview _preview;

        // 실제 스킬 실행이 성공하고 쿨다운이 시작된 뒤 알린다.
        public event Action<int> SkillUsed;

        private void Awake()
        {
            _flow = GetComponent<SkillStateFlow>();
            _playerCore = GetComponentInParent<PlayerCore>(true);
        }

        private void OnEnable()
        {
            _flow.SkillRequested += OnSkillRequested;
            _flow.ExecuteConfirmRequested += OnExecuteConfirmRequested;
            _flow.ExecuteCanceled += OnExecuteCanceled;
            for (int slot = 1; slot <= 4; slot++) PublishSlot(slot);
        }

        private void OnDisable()
        {
            _flow.SkillRequested -= OnSkillRequested;
            _flow.ExecuteConfirmRequested -= OnExecuteConfirmRequested;
            _flow.ExecuteCanceled -= OnExecuteCanceled;
            _flow.CancelExecute();
            ClearActiveSkill();
            _playerCore?.SkillDisplayChannel.Clear();
        }

        private void LateUpdate()
        {
            if (_activeSkill == null || _preview == null || !_flow.IsExecuting) return;

            bool canPlace = SkillPlacementValidator.TryGetPlacement(_activeSkill,
                _playerCore.transform, Camera.main, out Pose pose, out bool hasTarget);
            _preview.Show(pose, hasTarget, canPlace);
        }

        private SkillData GetSkill(int slot) =>
            slot >= 1 && slot <= 4 && _slots != null && slot <= _slots.Length
                ? _slots[slot - 1] : null;

        private float GetRemainingCooldown(int slot)
        {
            SkillData skill = GetSkill(slot);
            return skill != null && _readyAt.TryGetValue(skill, out float readyAt)
                ? Mathf.Max(0f, readyAt - Time.time) : 0f;
        }

        private void OnSkillRequested(int slot)
        {
            // 현재는 설치형만 실행한다. 다른 스킬 종류는 실행 로직을 추가할 때 연결한다.
            if (GetSkill(slot) is not DeployableSkillData skill || skill.Prefab == null ||
                GetRemainingCooldown(slot) > 0f || _playerCore == null) return;

            if (!_flow.BeginExecute(slot)) return;
            _activeSkill = skill;
            _preview = new SkillPlacementPreview(skill.Prefab);
            _playerCore.CombatModule?.CancelSecondary();
        }

        private void OnExecuteConfirmRequested(int slot)
        {
            if (_activeSkill == null || !_flow.IsExecuting || _flow.SelectedSlot != slot ||
                GetSkill(slot) != _activeSkill || _playerCore == null) return;

            Camera camera = Camera.main;
            if (!SkillPlacementValidator.TryGetPlacement(_activeSkill, _playerCore.transform,
                    camera, out Pose pose)) return;

            Instantiate(_activeSkill.Prefab, pose.position, pose.rotation);
            _readyAt[_activeSkill] = Time.time + _activeSkill.Cooldown;
            ClearActiveSkill();
            _flow.CompleteExecute();
            PublishSlot(slot);
            SkillUsed?.Invoke(slot);
        }

        private void OnExecuteCanceled(int slot) => ClearActiveSkill();

        private void ClearActiveSkill()
        {
            _preview?.Dispose();
            _preview = null;
            _activeSkill = null;
        }

        private void PublishSlot(int slot)
        {
            if (_playerCore == null) return;
            SkillData skill = GetSkill(slot);
            float endTime = skill != null && _readyAt.TryGetValue(skill, out float readyAt)
                ? readyAt : 0f;
            _playerCore.SkillDisplayChannel.Publish(new SkillSlotDisplayState(slot, skill?.Icon,
                skill?.Cooldown ?? 0f, endTime));
        }
    }
}
