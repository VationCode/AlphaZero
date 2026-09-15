using UnityEngine;
namespace alpha.player.locomotion
{
    public class DashState : LocomotionState
    {
        public DashState(PlayerCore core) : base(core) { }
        private Vector3 _dashDirection;
        private float _elapsedTime;
        public override void Enter()
        {
            base.Enter();

            _elapsedTime = 0f;
            
            _dashDirection = _Core.LocomotionContext.LastMoveDirection;

            if (_dashDirection.sqrMagnitude < 0.01f)
            {
                _dashDirection = _LocomotionModule.GetForwardDirection();
            }

            _LocomotionModule.RotationImmediate(_dashDirection);

            _AnimView.DashAnim();
        }
        public override void Update()
        {
            _elapsedTime += Time.deltaTime;

            _LocomotionModule.Dash(_dashDirection);

            if (_elapsedTime >= _LocomotionModule.DashDuration)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Idle);
            }
        }

        public override void Exit()
        {

        }
    }
}
