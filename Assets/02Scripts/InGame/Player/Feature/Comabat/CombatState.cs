using alpha.input;
using alpha.player.anim;
using UnityEngine;

namespace alpha.player.combat
{
    public abstract class CombatState
    {
        protected PlayerCore _Core;
        protected CombatModule _CombatModule => _Core.CombatModule;
        protected CombatFlow _CombatFlow => _Core.CombatFlow;
        protected AnimationView _AnimView => _Core.AnimView;
        
        protected AlphaInputSystem _Input => _Core.Input;

        public CombatState(PlayerCore p_core)
        {
            _Core = p_core;
        }

        public virtual void Enter()
        {
        }
        public abstract void Update();
        public abstract void Exit();
    }
}