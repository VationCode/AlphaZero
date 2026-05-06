using UnityEngine;

namespace alpha.player.state
{
    public abstract class PlayerStateBase
    {
        public abstract void Enter(PlayerCore playerCore);
        public abstract void Update(PlayerCore playerCore);
        public abstract void Exit(PlayerCore playerCore);
    }
}