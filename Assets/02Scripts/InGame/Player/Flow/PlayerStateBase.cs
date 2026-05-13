using UnityEngine;

namespace alpha.player.state
{
    public abstract class PlayerStateBase
    {
        public abstract void Enter(PlayerCore p_playerCore);
        public abstract void Update(PlayerCore p_playerCore);
        public abstract void Exit(PlayerCore p_playerCore);
    }
}