using alpha.player.state;
using UnityEngine;

namespace alpha.player.combat
{
    public class AttackState : PlayerStateBase
    {
        public override void Enter(PlayerCore p_playerCore)
        {
            var combat = p_playerCore.CombatModule;

            combat.EnterCombat();
        }

        public override void Update(PlayerCore p_playerCore)
        {

        }

        public override void Exit(PlayerCore p_playerCore)
        {
            
        }

    }
}