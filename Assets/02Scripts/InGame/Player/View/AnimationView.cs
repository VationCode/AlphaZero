using UnityEngine;

namespace alpha.player.anim
{
    public class AnimationView : MonoBehaviour
    {
        // Ref
        [SerializeField]
        private Animator _animator;

        [SerializeField] private float _blendSpeed = 8f;

        private int _currentMoveState = -1;
        private readonly int MoveTreeHash = Animator.StringToHash("MoveTree");
        private readonly int MoveXHash = Animator.StringToHash("MoveX");
        private readonly int MoveYHash = Animator.StringToHash("MoveY");

        private readonly int MoveMagnitudeHash = Animator.StringToHash("MoveMagnitude");

        private readonly int DashHash = Animator.StringToHash("Dash");
        
        private readonly int DodgeXHash = Animator.StringToHash("DodgeX");
        private readonly int DodgeYHash = Animator.StringToHash("DodgeY");
        private readonly int DodgeTimeHash = Animator.StringToHash("DodgeTime");


        private readonly int SprintHash = Animator.StringToHash("Sprint");
        private readonly int CombatSprintHash = Animator.StringToHash("CombatSprint");
        private readonly int DodgeTreeHash = Animator.StringToHash("DodgeTree");

        //Move
        private float _moveAnimsmoothTime = 0.1f;
        private float _moveAnimMagnitude;
        private float _moveAnimVelocity;

        //Flight
        private float _flightMoveAnimsmoothTime = 0.1f;
        private float _flightMoveAnimMagnitude;
        private float _flightMoveAnimVelocity;

        private int _currentLayerIndex = 0;
        private int _targetLayerIndex = 0;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _currentLayerIndex = 0;
        }

        public void ActivateLootMotion(bool p_isActivate)
        {
            _animator.applyRootMotion = p_isActivate;
        }

        public void UpdateGroundMove(Vector2 p_localVelocity, bool p_isSprint = false, bool p_isInCombat = false)
        {
            float magnitude = p_localVelocity.magnitude;

            if (magnitude < 0.01f)
            {
                _moveAnimMagnitude = 0f;
            }
            else
            {
                _moveAnimMagnitude = 
                    Mathf.SmoothDamp(_moveAnimMagnitude, magnitude, ref _moveAnimVelocity, _moveAnimsmoothTime);
            }

            int targetState;

            if (p_isSprint)
            {
                targetState = p_isInCombat? CombatSprintHash : SprintHash;
            }
            else
            {
                targetState = MoveTreeHash;
            }

            if (_currentMoveState != targetState)
            {
                _animator.CrossFadeInFixedTime(targetState, 0.05f, 0, 0f);
                _currentMoveState = targetState;
            }

            _animator.SetFloat(MoveXHash, p_localVelocity.x);
            _animator.SetFloat(MoveYHash, p_localVelocity.y);
            _animator.SetFloat(MoveMagnitudeHash, _moveAnimMagnitude);
        }

        public void DodgeAnim(Vector2 p_localDirection)
        {
            // DodgeState에서 확정한 방향을 그대로 사용한다. 무입력은 정면 회피.
            Vector2 dodgeDirection = p_localDirection.sqrMagnitude > 0.01f
                ? p_localDirection.normalized : Vector2.up;

            _animator.SetFloat(DodgeXHash, dodgeDirection.x);
            _animator.SetFloat(DodgeYHash, dodgeDirection.y);
            //_animator.SetFloat(DodgeTimeHash, 0f);

            // 이전 이동 클립 길이에 영향받지 않는 짧은 전환으로 시작한다.
            _animator.CrossFadeInFixedTime(DodgeTreeHash, 0.05f, 0, 0f);

            _currentMoveState = DodgeTreeHash;
        }

        public void UpdateDodgeAnim(float p_normalizedTime)
        {
            _animator.SetFloat(DodgeTimeHash, Mathf.Clamp01(p_normalizedTime));
        }

        public void JumpUpAnim()
        {
            _animator.Play("JumpUp");
        }

        public void FallAnim()
        {
            _animator.CrossFade("Fall", 0.2f);
        }

        public void LandAnim()
        {
            _animator.CrossFade("Landing", 0.143f, 0, 0.443f);
        }

        public void DashAnim()
        {
            _animator.CrossFadeInFixedTime(DashHash, 0.05f, 0, 0f);
        }

        public void FlyUpAnim()
        {
            _animator.Play("FlyUp");
        }

        public void FlightCrossFade()
        {
            _animator.CrossFade("FlightTree", 0.2f);
        }
        public void FlightAnim(Vector3 p_velocity)
        {
            Vector3 horizontal = new Vector3(p_velocity.x, 0f, p_velocity.z);

            _flightMoveAnimMagnitude = Mathf.SmoothDamp(
                _flightMoveAnimMagnitude,
                horizontal.magnitude,
                ref _flightMoveAnimVelocity,
                _flightMoveAnimsmoothTime
            );


            _animator.SetFloat("FlightMove", _flightMoveAnimMagnitude);
        }

        // Combat
        public void SwapAnim(int p_swapNum)
        {
            _animator.CrossFade("Swap", 0.1f);
            _currentLayerIndex = p_swapNum;
        }
        public void ChangeLayer(int p_targetLayer)
        {
            _targetLayerIndex = p_targetLayer;
        }

        public void BlendLayers(int p_targetLayerNum)
        {
            int layerCount = _animator.layerCount;

            for (int i = 0; i < layerCount; i++)
            {
                float currentWeight = _animator.GetLayerWeight(i);

                float targetWeight =
                    i == p_targetLayerNum ? 1f : 0f;

                float nextWeight = Mathf.Lerp(
                    currentWeight,
                    targetWeight,
                    Time.deltaTime * _blendSpeed);

                _animator.SetLayerWeight(i, nextWeight);
            }
        }
    }
}
