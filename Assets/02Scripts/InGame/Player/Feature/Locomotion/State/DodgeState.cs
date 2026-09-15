using UnityEngine;

namespace alpha.player.locomotion
{
    public class DodgeState : LocomotionState
    {
        public DodgeState(PlayerCore core) : base(core){}

        private Vector3 _dodgeDirection;
        private float _elapsedTime;

        public override void Enter()
        {
            base.Enter();

            _elapsedTime = 0f;

            Vector2 dodgeInput = _Core.LocomotionContext.DodgeInput;
            Vector2 localDodgeDirection;

            if (dodgeInput.sqrMagnitude > 0.01f)
            {
                // 실제 이동 : Camera 기준
                _dodgeDirection = _LocomotionModule.GetMoveDirection(dodgeInput);

                // Animation : Character Local 기준
                localDodgeDirection = _LocomotionModule.GetLocalDirection(_dodgeDirection).normalized;
            }
            else
            {
                // 입력이 없으면 실제 이동은 Player Forward
                _dodgeDirection = _LocomotionModule.GetForwardDirection();

                // Animation도 명확하게 Forward
                localDodgeDirection = Vector2.up;
            }

            // 이동은 월드 방향, 애니메이션은 진입 시 확정한 캐릭터 로컬 방향을 사용한다.
            _AnimView.DodgeAnim(localDodgeDirection);
        }

        public override void Update()
        {
            _elapsedTime += Time.deltaTime;

            _LocomotionModule.Dodge(_dodgeDirection);

            // 클립 길이와 관계없이 회피 지속 시간에 맞춰 끝까지 재생한다.
            float duration = _LocomotionModule.DodgeDuration;
            //_AnimView.UpdateDodgeAnim(duration > 0f ? _elapsedTime / duration : 1f);

            if (_elapsedTime >= duration)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Idle);
            }
        }

        public override void Exit()
        {
            _Core.LocomotionContext.ClearDodgeInput();
        }
    }
}
