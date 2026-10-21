using alpha.ingame.input;

namespace alpha.ingame.player
{
    /// <summary>마지막 발사 후 전투 자세를 유지하고 재공격을 받는다.</summary>
    public sealed class CombatReadyState : CombatState
    {
        public CombatReadyState(PlayerCore playerCore) : base(playerCore) { }

        public override void EnterState() { }

        public override void UpdateState()
        {
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
