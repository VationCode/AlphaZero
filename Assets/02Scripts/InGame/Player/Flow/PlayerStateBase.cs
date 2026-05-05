using alpha.player.module;
using UnityEngine;

namespace alpha.player.flow
{
    public abstract class PlayerStateBase
    {
        public abstract void Enter(PlayerCore playerCore);
        public abstract void Update(PlayerCore playerCore);
        public abstract void Exit(PlayerCore playerCore);
    }
}