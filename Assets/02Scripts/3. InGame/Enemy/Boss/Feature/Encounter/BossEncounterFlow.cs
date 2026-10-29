using System;
using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    public enum EBossEncounterState
    {
        Dormant,
        Intro,
        Combat,
        PhaseTransition,
        Dead
    }

    /// <summary>보스전 시작, 페이즈 변경, 사망을 관리한다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossEncounterFlow : MonoBehaviour
    {
        [SerializeField] private bool _startInCombat;
        [SerializeField, Min(1)] private int _phaseCount = 2;

        public EBossEncounterState CurrentState { get; private set; } = EBossEncounterState.Dormant;
        public int CurrentPhase { get; private set; } = 1;
        public int PhaseCount => Mathf.Max(1, _phaseCount);

        public event Action<EBossEncounterState> StateChanged;

        private void Start()
        {
            // 진입 연출을 연결하기 전에는 씬 설정에 따라 바로 전투를 시작한다.
            if (_startInCombat) TryStartCombat();
        }

        public bool TryBeginIntro()
        {
            if (CurrentState != EBossEncounterState.Dormant) return false;
            ChangeState(EBossEncounterState.Intro);
            return true;
        }

        public bool TryStartCombat()
        {
            if (CurrentState != EBossEncounterState.Dormant &&
                CurrentState != EBossEncounterState.Intro) return false;
            ChangeState(EBossEncounterState.Combat);
            return true;
        }

        public bool TryBeginPhaseTransition()
        {
            if (CurrentState != EBossEncounterState.Combat ||
                CurrentPhase >= PhaseCount) return false;
            ChangeState(EBossEncounterState.PhaseTransition);
            return true;
        }

        public bool TryCompletePhaseTransition()
        {
            if (CurrentState != EBossEncounterState.PhaseTransition) return false;
            CurrentPhase++;
            ChangeState(EBossEncounterState.Combat);
            return true;
        }

        public void MarkDead()
        {
            if (CurrentState != EBossEncounterState.Dead)
                ChangeState(EBossEncounterState.Dead);
        }

        private void ChangeState(EBossEncounterState next)
        {
            CurrentState = next;
            StateChanged?.Invoke(next);
        }
    }
}
