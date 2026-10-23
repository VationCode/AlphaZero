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
        private Pose _castPose;
        private int _castingSlot;
        private bool _castSpawned;
        private float _castTimeoutAt;

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
            if (_flow.IsCasting)
            {
                UpdateCast();
                return;
            }

            if (_activeSkill == null || _preview == null ||
                _flow.CurrentState != ESkillState.Execute) return;

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
                GetRemainingCooldown(slot) > 0f || _playerCore == null || !CanCastOnGround()) return;

            if (skill.ActivationMode == ESkillActivationMode.Self)
            {
                // 위치 선택 없이 시전을 시작하고 입력 시점의 플레이어 위치를 보관한다.
                Transform owner = _playerCore.transform;
                Quaternion rotation = Quaternion.Euler(0f, owner.eulerAngles.y, 0f);
                Pose pose = new Pose(owner.position + rotation * skill.PlacementOffset, rotation);
                _activeSkill = skill;
                _playerCore.CombatModule?.CancelSecondary();
                if (!TryBeginCast(slot, pose, true))
                    ClearActiveSkill();
                return;
            }

            if (!_flow.BeginExecute(slot)) return;
            _activeSkill = skill;
            _preview = new SkillPlacementPreview(skill.Prefab, skill.ClearanceRadius);
            _playerCore.CombatModule?.CancelSecondary();
        }

        private void OnExecuteConfirmRequested(int slot)
        {
            if (_activeSkill == null || _flow.CurrentState != ESkillState.Execute ||
                _flow.SelectedSlot != slot || GetSkill(slot) != _activeSkill ||
                _playerCore == null || !CanCastOnGround()) return;

            Camera camera = Camera.main;
            if (!SkillPlacementValidator.TryGetPlacement(_activeSkill, _playerCore.transform,
                    camera, out Pose pose)) return;

            TryBeginCast(slot, pose, false);
        }

        private bool TryBeginCast(int slot, Pose pose, bool self)
        {
            AnimationView animation = _playerCore.AnimationView;
            if (animation == null || !animation.PlaySkill(_activeSkill.AnimationState,
                    _activeSkill.CastAnimationClip, _activeSkill.CastDuration))
            {
                Debug.LogWarning($"{nameof(SkillModule)}: {_activeSkill.name} 스킬 모션을 재생할 수 없습니다.", this);
                return false;
            }

            _castPose = pose;
            _castingSlot = slot;
            _castSpawned = false;
            _castTimeoutAt = Time.time + _activeSkill.CastDuration + 2f;
            if (!(self ? _flow.BeginSelfCast(slot) : _flow.BeginCast()))
            {
                animation.StopSkill();
                return false;
            }

            _preview?.Dispose();
            _preview = null;

            return true;
        }

        private bool CanCastOnGround()
        {
            if (_playerCore?.LocomotionModule?.IsGrounded != true) return false;
            ELocomotionState state = _playerCore.LocomotionStateFlow.CurrentState;
            return state == ELocomotionState.Idle || state == ELocomotionState.Move;
        }

        private void UpdateCast()
        {
            if (_activeSkill == null || _playerCore?.AnimationView == null)
            {
                _flow.CancelExecute();
                return;
            }

            // 예기치 않은 Animator 전환이 발생해도 이동 잠금이 남지 않게 한다.
            if (Time.time > _castTimeoutAt)
            {
                Debug.LogWarning($"{nameof(SkillModule)}: 스킬 모션이 종료되지 않아 실행을 중단합니다.", this);
                if (_castSpawned) FinishCast();
                else _flow.CancelExecute();
                return;
            }

            if (!_playerCore.AnimationView.TryGetSkillProgress(out float progress)) return;

            if (progress < 1f) return;
            if (!_castSpawned)
            {
                Debug.LogWarning($"{nameof(SkillModule)}: {_activeSkill.name} 실행 이벤트 없이 스킬 모션이 끝났습니다.", this);
                _flow.CancelExecute();
                return;
            }

            FinishCast();
        }

        // 현재 시전 중인 스킬의 키 설정으로 실행/오디오 이벤트를 구분한다.
        public void HandleAnimationEvent(string eventKey, AudioSource source)
        {
            if (!_flow.IsCasting || _activeSkill == null || string.IsNullOrEmpty(eventKey)) return;

            if (string.Equals(eventKey, _activeSkill.ExecuteEventKey, StringComparison.Ordinal))
            {
                if (!_castSpawned && !TrySpawn()) _flow.CancelExecute();
                return;
            }

            if (source == null) return;
            AudioClip clip = string.Equals(eventKey, _activeSkill.Audio1EventKey, StringComparison.Ordinal)
                ? _activeSkill.Audio1
                : string.Equals(eventKey, _activeSkill.Audio2EventKey, StringComparison.Ordinal)
                    ? _activeSkill.Audio2 : null;
            if (clip != null) source.PlayOneShot(clip);
        }

        private bool TrySpawn()
        {
            GameObject instance = Instantiate(_activeSkill.Prefab, _castPose.position, _castPose.rotation);
            if (!_activeSkill.TryActivateInstance(instance))
            {
                Destroy(instance);
                return false;
            }

            _castSpawned = true;
            _readyAt[_activeSkill] = Time.time + _activeSkill.Cooldown;
            PublishSlot(_castingSlot);
            SkillUsed?.Invoke(_castingSlot);
            return true;
        }

        private void FinishCast()
        {
            ClearActiveSkill();
            _flow.CompleteExecute();
        }

        private void OnExecuteCanceled(int slot) => ClearActiveSkill();

        private void ClearActiveSkill()
        {
            _playerCore?.AnimationView?.StopSkill();
            _preview?.Dispose();
            _preview = null;
            _activeSkill = null;
            _castingSlot = 0;
            _castSpawned = false;
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
