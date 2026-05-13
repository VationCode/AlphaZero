using UnityEngine;

namespace alpha.player.anim
{
    public class PlayerAnimationBoundary : MonoBehaviour
    {
        // Ref
        [SerializeField]
        private Animator m_animator;
        
        #region Config
        //Move
        private float m_moveAnimsmoothTime = 0.1f;

        //Flight
        private float m_flightMoveAnimsmoothTime = 0.1f;
        #endregion

        #region RunTime
        //Move
        private float m_moveAnimMagnitude;
        private float m_moveAnimVelocity;

        //Flight
        private float m_flightMoveAnimMagnitude;
        private float m_flightMoveAnimVelocity;
        #endregion
        private void Awake()
        {
            m_animator = GetComponentInChildren<Animator>();
        }

        public void UpdateGroundMove(Vector3 p_velocity)
        {
            Vector3 horizontal = new Vector3(p_velocity.x, 0f, p_velocity.z);

            if (horizontal == Vector3.zero)
            {
                m_moveAnimMagnitude = 0;
            }
            else
            {
                m_moveAnimMagnitude = Mathf.SmoothDamp(
                    m_moveAnimMagnitude,
                    horizontal.magnitude,
                    ref m_moveAnimVelocity,
                    m_moveAnimsmoothTime
                );
            }

            m_animator.SetFloat("Move", m_moveAnimMagnitude);
        }
        public void JumpUpAnim()
        {
            m_animator.Play("JumpUp");
        }
        public void FallAnim()
        {
            m_animator.CrossFade("Fall", 0.2f);
        }
        public void LandAnim()
        {
            m_animator.CrossFade("Landing", 0.143f, 0, 0.443f);
        }
        public void DashAnim()
        {
            m_animator.Play("Dash");
        }
        public void FlyUpAnim()
        {
            m_animator.Play("FlyUp");
        }

        public void FlightCrossFade()
        {
            m_animator.CrossFade("FlightTree", 0.2f);
        }
        public void FlightAnim(Vector3 p_velocity)
        {
            Vector3 horizontal = new Vector3(p_velocity.x, 0f, p_velocity.z);


            m_flightMoveAnimMagnitude = Mathf.SmoothDamp(
                m_flightMoveAnimMagnitude,
                horizontal.magnitude,
                ref m_flightMoveAnimVelocity,
                m_flightMoveAnimsmoothTime
            );


            m_animator.SetFloat("FlightMove", m_flightMoveAnimMagnitude);
        }
    }
}