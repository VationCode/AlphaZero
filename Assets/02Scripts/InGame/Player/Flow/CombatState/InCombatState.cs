using UnityEngine;
using alpha.player.state;

namespace alpha.player.combat
{
    public class InCombatState : PlayerStateBase
    {
        public override void Enter(PlayerCore playerCore)
        {
            var combatModule = playerCore.CombatModule;
            combatModule.EnterCombat();
        }
        public override void Update(PlayerCore playerCore)
        {
            var combatModule = playerCore.CombatModule;
            var combatFlow = playerCore.CombatFlow;
            var stateMachineFlow = playerCore.StateMachineFlow;


            if (combatFlow.IsAttackPressed)
            {
                combatModule.EnterCombat();
                return;
            }
            combatModule.UpdateCombat();

            if (!combatModule.IsCombatMode)
            {
                stateMachineFlow.ChangeCombatState(ECombatStateType.NoneCombat);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {
            
        }
    }
}
