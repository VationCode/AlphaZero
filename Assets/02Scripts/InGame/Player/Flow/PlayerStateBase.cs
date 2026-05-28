using UnityEngine;

namespace alpha.player.state
{
    public abstract class PlayerStateBase
    {
        public virtual EBlockedLocomotionAction BlockedLocoAction => EBlockedLocomotionAction.None;
        public virtual EBlockedCombatAction BlockedCombatAction => EBlockedCombatAction.None;
        public abstract void Enter(PlayerCore p_playerCore);
        public abstract void Update(PlayerCore p_playerCore);
        public abstract void Exit(PlayerCore p_playerCore);
    }
}