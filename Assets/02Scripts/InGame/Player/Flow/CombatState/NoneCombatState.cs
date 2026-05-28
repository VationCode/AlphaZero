using alpha.player.state;
using UnityEngine;
namespace alpha.player.combat
{
    public class NoneCombatState : PlayerStateBase
    {
        public override void Enter(PlayerCore p_playerCore)
        {

        }
        public override void Update(PlayerCore p_playerCore)
        {
            var input = p_playerCore.InputSystemBoundary;
            var combatFlow = p_playerCore.CombatFlow;

            combatFlow.UpdateInput(input);

            if(combatFlow.IsAttackPressed)
            {
                p_playerCore.StateMachineFlow.ChangeCombatState(ECombatStateType.InCombat);
            }
        }

        public override void Exit(PlayerCore p_playerCore)
        {

        }

    }
}
