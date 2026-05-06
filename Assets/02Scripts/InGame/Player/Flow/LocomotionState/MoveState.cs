using UnityEngine;
using alpha.player.state;


namespace alpha.player.locomotion
{
    public class MoveState : PlayerStateBase
    {
        public override void Enter(PlayerCore playerCore)
        {
            
        }
        public override void Update(PlayerCore playerCore)
        {
            var _loco = playerCore.LocomotionModule;
            var _input = playerCore.InputSystemBoundary;
            var _ctrl = playerCore.CharacterCtrlBoudary;
            var _anim = playerCore.AnimBoundary;
            var _state = playerCore.StateMachineFlow;

            // ==================== Ground 체크
            // 실제 물리적 체크
            bool _isGroundDetected = _ctrl.CheckGround();
            // 물리체크이후 찐 Ground체크 (OnLeaveGround함수를 통해 무시(False로 유지)해야하는 경우도 발생하기에)
            bool _isGroundHit = _loco.UpdateGround(_isGroundDetected);

            // ==================== 연산
            // 중력
            _loco.ApplyGravity();
            // 이동
            _loco.HandleMove(false, _input.MoveInputDir);
            // 회전
            _loco.HandleRotation(false);
            // 최종 반영될 속도 
            Vector3 _finalVelocity = _loco.GetFinalVelocity();
            Vector3 _horizontal = new Vector3(_finalVelocity.x, 0, _finalVelocity.z);   //Ground이동 애니메이션이기에 y제거
            
            // ==================== 적용
            // 실제 이동
            _ctrl.SetMove(_finalVelocity);
            // 애니메이션
            _anim.UpdateGroundMove(_horizontal);

            // ==================== 상태 전환
            if (_input.IsJumpInput)
            {
                _state.ChangeLocoState(ELocomotionStateType.JumpUp);
            }
            else if (_input.IsDashInput)
            {
                _state.ChangeLocoState(ELocomotionStateType.Dash);
            }
            else if (!_isGroundHit)
            {
                _state.ChangeLocoState(ELocomotionStateType.Fall);
            }
            else if (_input.IsFlyInput)
            {
                _state.ChangeLocoState(ELocomotionStateType.FlyUp);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {
            playerCore.AnimBoundary.UpdateGroundMove(Vector3.zero);
        }

    }
}