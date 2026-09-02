using alpha.input;
using alpha.player.anim;
using UnityEngine;

namespace alpha.player.locomotion
{
    public abstract class LocomotionState
    {
        protected PlayerCore _Core;
        protected AlphaInputSystem _InputSystem => _Core.Input;
        protected LocomotionModule _LocomotionModule => _Core.LocomotionModule;
        protected LocomotionFlow _LocomotionFlow => _Core.LocomotionFlow;
        protected AnimationView _AnimView => _Core.AnimView;

        public LocomotionState(PlayerCore core)
        {
            _Core = core;
        }

        public virtual void Enter()
        {
            Debug.Log(_Core.LocomotionContext.CurrentStateType);
        }
        public abstract void Update();
        public abstract void Exit();
    }
}