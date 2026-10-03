namespace alpha.ingame.player
{
    public abstract class LocomotionState
    {
        protected PlayerCore _PlayerCore;
        protected LocomotionStateFlow _StateMachine;
        protected LocomotionModule _LocomotionModule;
        protected AnimationView _AnimationView;

        public LocomotionState(PlayerCore p_playerCore)
        {
            _PlayerCore = p_playerCore;
            _StateMachine = _PlayerCore.LocomotionStateFlow;
            _LocomotionModule = _PlayerCore.LocomotionModule;
            _AnimationView = _PlayerCore.AnimationView;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
    }
}
