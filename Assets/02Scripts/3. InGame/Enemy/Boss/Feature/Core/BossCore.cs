using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>보스의 진행 상태와 전투 행동 상태를 연결하는 진입점이다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossCore : MonoBehaviour
    {
        public BossEncounterFlow Encounter { get; private set; }
        public BossActionFlow Action { get; private set; }
        public BossActionModule ActionModule { get; private set; }
        public BossLocomotionModule Locomotion { get; private set; }
        public BossChasingModule Chasing { get; private set; }
        public BossAnimationView Animation { get; private set; }

        private void Awake()
        {
            Encounter = GetComponentInChildren<BossEncounterFlow>(true);
            Action = GetComponentInChildren<BossActionFlow>(true);
            ActionModule = GetComponentInChildren<BossActionModule>(true);
            Locomotion = GetComponentInChildren<BossLocomotionModule>(true);
            Chasing = GetComponentInChildren<BossChasingModule>(true);
            Animation = GetComponentInChildren<BossAnimationView>(true);
            Rigidbody body = GetComponent<Rigidbody>();
            if (Encounter == null || Action == null || ActionModule == null ||
                Locomotion == null || Chasing == null || body == null ||
                Animation == null || !Animation.IsReady)
            {
                Debug.LogError($"{name}: 보스 상태, 추적·이동 모듈, 애니메이션 또는 Rigidbody가 누락되었습니다.", this);
                enabled = false;
                return;
            }

            BossIdleState idle = new();
            BossChasingState chasingState = new();
            BossAttackingState attackingState = new();
            Locomotion.Bind(body);
            Chasing.Bind(Locomotion);
            ActionModule.Bind(transform);
            Action.Bind(Encounter, idle, chasingState, attackingState,
                ActionModule, Chasing, Locomotion, transform.position, transform.forward);
            Animation.Bind(Action);
            Animation.AttackAnimationFinished += Action.CompleteAttackAnimation;
            Animation.Attack3JumpStarted += Action.BeginAttack3Jump;
            Animation.Attack3JumpProgressChanged += Action.UpdateAttack3Jump;
            Animation.Attack3JumpEnded += Action.EndAttack3Jump;
            Animation.Attack5ButtStarted += Action.BeginAttack5Butt;
            Animation.Attack5ButtProgressChanged += Action.UpdateAttack5Butt;
            Animation.Attack5ButtEnded += Action.EndAttack5Butt;
        }

        private void OnDestroy()
        {
            if (Animation != null && Action != null)
            {
                Animation.AttackAnimationFinished -= Action.CompleteAttackAnimation;
                Animation.Attack3JumpStarted -= Action.BeginAttack3Jump;
                Animation.Attack3JumpProgressChanged -= Action.UpdateAttack3Jump;
                Animation.Attack3JumpEnded -= Action.EndAttack3Jump;
                Animation.Attack5ButtStarted -= Action.BeginAttack5Butt;
                Animation.Attack5ButtProgressChanged -= Action.UpdateAttack5Butt;
                Animation.Attack5ButtEnded -= Action.EndAttack5Butt;
            }
        }
    }
}
