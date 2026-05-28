using alpha.player.state;
using UnityEngine;

namespace alpha.player.combat
{
    public class NoneActionState : PlayerStateBase
    {
        public override void Enter(PlayerCore p_playerCore)
        {
            var combatModule = p_playerCore.CombatModule;

            combatModule.ClearCombatTimer();
        }

        public override void Update(PlayerCore p_playerCore)
        {
            var input = p_playerCore.InputSystemBoundary;
            var equip = p_playerCore.EquipmentModule;
            var combatFlow = p_playerCore.CombatFlow;

            if (combatFlow.IsSwapPressed)
            {
                if(equip.GetCurrentSwapNum() == input.SwapNum) return;
                
                p_playerCore.StateMachineFlow.ChangeCombatActionState(ECombatActionStateType.Swap);
                equip.SetSwapNum(input.SwapNum);
            }
            else if (combatFlow.IsAttackPressed)
            {
                p_playerCore.StateMachineFlow.ChangeCombatActionState(ECombatActionStateType.Attack);
            }
        }

        public override void Exit(PlayerCore p_playerCore)
        {
            
        }

    }
}