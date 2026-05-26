using UnityEngine;

namespace alpha.player.anim
{
    public class PlayerAnimationBoundary : MonoBehaviour
    {
        // Ref
        [SerializeField]
        private Animator m_animator;

        #region Config
        [SerializeField] private float m_blendSpeed = 8f;

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

        #region RunTime
        private int m_currentLayerIndex = 0;
        private int m_targetLayerIndex = 0;

        #endregion
        private void Awake()
        {
            m_animator = GetComponentInChildren<Animator>();
            m_currentLayerIndex = 0;
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

        // Combat
        public void SwapAnim(int p_swapNum)
        {
            m_animator.CrossFade("Swap", 0.1f);
            m_currentLayerIndex = p_swapNum;
        }
        public void ChangeLayer(int p_targetLayer)
        {
            m_targetLayerIndex = p_targetLayer;
        }

        public void BlendLayers(int p_targetLayerNum)
        {
            int layerCount = m_animator.layerCount;

            for (int i = 0; i < layerCount; i++)
            {
                float currentWeight = m_animator.GetLayerWeight(i);

                float targetWeight =
                    i == p_targetLayerNum ? 1f : 0f;

                float nextWeight = Mathf.Lerp(
                    currentWeight,
                    targetWeight,
                    Time.deltaTime * m_blendSpeed);

                m_animator.SetLayerWeight(i, nextWeight);
            }
        }
    }
}