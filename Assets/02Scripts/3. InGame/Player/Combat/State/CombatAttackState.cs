using alpha.ingame.input;
using alpha.ingame.item;

namespace alpha.ingame.player
{
    /// <summary>단발·연사·차징·빔의 입력 구간을 관리하고 발사 방식은 무기에 맡긴다.</summary>
    public sealed class CombatAttackState : CombatState
    {
        private GunWeapon _gun;

        public CombatAttackState(PlayerCore playerCore) : base(playerCore) { }

        public override void EnterState()
        {
            _gun = Module.CurrentGun;
            _gun?.BeginAttack();
        }

        public override void UpdateState()
        {
            if (_gun == null || _gun != Module.CurrentGun || !Module.CanContinueAttack)
            {
                Flow.ChangeState(ECombatState.Idle);
                return;
            }

            AlphaInput input = AlphaInput.Instance;
            if (input == null)
            {
                Flow.ChangeState(ECombatState.Idle);
                return;
            }

            if (!input.IsPrimaryHeld)
            {
                // 차징 무기는 키를 놓을 때 발사하므로 종료 후 Ready 여부를 판단한다.
                _gun.EndAttack();
                Flow.ChangeState(Module.HasReadyTime ? ECombatState.Ready : ECombatState.Idle);
                return;
            }

            _gun.UpdateAttack();
        }

        public override void ExitState()
        {
            // 퀵슬롯이나 무기 전환으로 빠져나갈 때 차징을 취소한다.
            _gun?.CancelAttack();
            _gun = null;
        }
    }
}
