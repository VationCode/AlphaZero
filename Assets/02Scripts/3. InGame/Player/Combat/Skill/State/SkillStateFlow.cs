using System;
using System.Collections.Generic;
using alpha.ingame.input;
using UnityEngine;

namespace alpha.ingame.player
{
    public enum ESkillState
    {
        Idle,
        Execute
    }

    /// <summary>스킬 슬롯 입력과 실행 상태를 관리한다.</summary>
    [DefaultExecutionOrder(-200)]
    public class SkillStateFlow : MonoBehaviour
    {
        private Dictionary<ESkillState, SkillState> _states;
        private SkillState _currentState;
        private int _previousInputSlot;
        private int _primaryConsumedFrame = -1;

        public ESkillState CurrentState { get; private set; } = ESkillState.Idle;
        public bool IsExecuting => _currentState != null && CurrentState == ESkillState.Execute;
        public bool IsPrimaryConsumedThisFrame => _primaryConsumedFrame == Time.frameCount;
        public int SelectedSlot { get; private set; }
        public int PressedSlotThisFrame { get; private set; }

        public event Action<int> SkillRequested;
        public event Action<int> ExecuteConfirmRequested;
        public event Action<int> ExecuteCanceled;

        private void Awake()
        {
            _states = new Dictionary<ESkillState, SkillState>
            {
                { ESkillState.Idle, new SkillIdleState(this) },
                { ESkillState.Execute, new SkillExecuteState(this) }
            };
        }

        private void OnEnable() => ChangeState(ESkillState.Idle);

        private void Update()
        {
            AlphaInput input = AlphaInput.Instance;
            int inputSlot = input?.SkillNum ?? 0;
            PressedSlotThisFrame = inputSlot != 0 && inputSlot != _previousInputSlot ? inputSlot : 0;
            _previousInputSlot = inputSlot;

            if (input == null || input.IsQuickSlotOpen)
            {
                if (IsExecuting) CancelExecute();
                return;
            }

            _currentState?.UpdateState();
        }

        private void OnDisable()
        {
            if (IsExecuting) CancelExecute();
            _currentState?.ExitState();
            _currentState = null;
            CurrentState = ESkillState.Idle;
            SelectedSlot = 0;
            PressedSlotThisFrame = 0;
            _previousInputSlot = 0;
            _primaryConsumedFrame = -1;
        }

        // 슬롯의 스킬이 실행 가능한지 판단한 뒤 호출한다.
        public bool BeginExecute(int slot)
        {
            if (slot < 1 || slot > 4 || _currentState == null || CurrentState != ESkillState.Idle)
                return false;

            SelectedSlot = slot;
            ChangeState(ESkillState.Execute);
            return true;
        }

        public void CompleteExecute()
        {
            if (!IsExecuting) return;
            ChangeState(ESkillState.Idle);
            SelectedSlot = 0;
        }

        public void CancelExecute()
        {
            if (!IsExecuting) return;
            int slot = SelectedSlot;
            ChangeState(ESkillState.Idle);
            SelectedSlot = 0;
            ExecuteCanceled?.Invoke(slot);
        }

        public void RequestSkill(int slot) => SkillRequested?.Invoke(slot);

        public void RequestExecuteConfirm()
        {
            if (!IsExecuting) return;

            // 설치에 성공해 Idle로 돌아와도 같은 클릭을 무기 공격에 재사용하지 않는다.
            _primaryConsumedFrame = Time.frameCount;
            ExecuteConfirmRequested?.Invoke(SelectedSlot);
        }

        private void ChangeState(ESkillState nextState)
        {
            if (_states == null || !_states.TryGetValue(nextState, out SkillState next) || _currentState == next)
                return;

            _currentState?.ExitState();
            _currentState = next;
            CurrentState = nextState;
            _currentState.EnterState();
        }
    }
}
