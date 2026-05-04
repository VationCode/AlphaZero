using UnityEngine;

namespace alpha.player.boundary
{
    public class PlayerAnimationBoundary : MonoBehaviour
    {
        // Ref
        [SerializeField]
        private Animator m_animator;
        
        #region Config
        private float m_moveAnismoothTime = 0.1f;
        #endregion

        #region RunTime
        private float m_moveAniMagnitude;
        private float m_moveAniVelocity;
        #endregion
        private void Awake()
        {
            m_animator = GetComponentInChildren<Animator>();
        }

        public void UpdateGroundMove(Vector3 velocity)
        {
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);

            if (velocity == Vector3.zero)
            {
                m_moveAniMagnitude = 0;
            }
            else
            {
                m_moveAniMagnitude = Mathf.SmoothDamp(
                    m_moveAniMagnitude,
                    horizontal.magnitude,
                    ref m_moveAniVelocity,
                    m_moveAnismoothTime
                );
            }

            m_animator.SetFloat("move", m_moveAniMagnitude);
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
            m_animator.CrossFade("Dash", 0.1f);
        }
    }
}