using alpha.ingame.input;

namespace alpha.ingame.player
{
    /// <summary>공격 또는 재장전 이후 전투 자세를 유지하고 재공격을 받는다.</summary>
    public sealed class CombatReadyState : CombatState
    {
        public CombatReadyState(PlayerCore playerCore) : base(playerCore) { }

        public override void EnterState() { }

        public override void UpdateState()
        {
            // 재장전 중에는 Ready 자세를 유지하며 공격과 자세 시간 만료를 보류한다.
            if (Flow.IsReloading) return;

            AlphaInput input = AlphaInput.Instance;
            if (input != null && input.IsPrimaryPressed && Module.CanAttack)
            {
                Flow.ChangeState(ECombatState.Attack);
                return;
            }

            if (!Module.HasReadyTime)
                Flow.ChangeState(ECombatState.Idle);
        }

        public override void ExitState() { }
    }
}
