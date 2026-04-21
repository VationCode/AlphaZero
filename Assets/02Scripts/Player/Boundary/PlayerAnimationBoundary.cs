using UnityEngine;

namespace player.boundary
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
    }
}