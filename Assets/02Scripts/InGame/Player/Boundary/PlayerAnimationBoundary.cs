using UnityEngine;

namespace alpha.player.boundary
{
    public class PlayerAnimationBoundary : MonoBehaviour
    {
        [SerializeField]
        private Animator m_animator;

        private void Awake()
        {
            m_animator = GetComponentInChildren<Animator>();
        }

        public void SetMove(float moveMagnitude)
        {
            m_animator.SetFloat("move", moveMagnitude);
        }
        public void SetJumpUp()
        {
            m_animator.CrossFade("JumpUp", 0.1f);
        }
        public void SetFall()
        {
            m_animator.CrossFade("Fall", 0.2f);
        }
        public void SetLand()
        {
            m_animator.CrossFade("Landing", 0.143f, 0, 0.443f);
        }
        public void SetDash()
        {
            m_animator.CrossFade("Dash", 0.1f);
        }
    }
}