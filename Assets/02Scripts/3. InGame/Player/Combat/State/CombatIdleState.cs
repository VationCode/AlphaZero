using alpha.ingame.input;

namespace alpha.ingame.player
{
    /// <summary>평상시 공격 입력을 기다린다.</summary>
    public sealed class CombatIdleState : CombatState
    {
        public CombatIdleState(PlayerCore playerCore) : base(playerCore) { }

        public override void EnterState() { }

        public override void UpdateState()
        {
            AlphaInput input = AlphaInput.Instance;
            if (input != null && input.IsAttackPressed && Module.CanAttack)
                Flow.ChangeState(ECombatState.Attack);
        }

        public override void ExitState() { }
    }
}
