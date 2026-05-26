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
            var equip = p_playerCore.EquipmentModule;
            var anim = p_playerCore.AnimBoundary;

            if (input.IsSwapInput)
            {
                if(equip.GetCurrentSwapNum() == input.SwapNum)
                {
                    return;
                }
                p_playerCore.StateMachineFlow.ChangeCombatState(ECombatStateType.Swap);
                equip.SetSwapNum(input.SwapNum);
            }
        }

        public override void Exit(PlayerCore p_playerCore)
        {

        }

    }
}
