using UnityEngine;

namespace alpha.player.contorller
{
    // 리지드바디와의 차이
    // 물리영향거의x, 직접스크립트제어 방식
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacterControllerBoudary : MonoBehaviour
    {
        // Ref
        [SerializeField]
        private CharacterController m_characterCtrl;

        #region Config
        [SerializeField]
        private float m_groundDistance = 0.25f;
        [SerializeField]
        private LayerMask m_groundMask;
        #endregion

        private void Awake()
        {
            m_characterCtrl = GetComponent<CharacterController>();
        }

        public void SetMove(Vector3 finalVelocity)
        {
            m_characterCtrl.Move(finalVelocity * Time.deltaTime);
        }

        public bool CheckGround()
        {
            // m_characterController.center 바닥에서 조금 띄어져있는 상태
            Vector3 worldCenter = m_characterCtrl.transform.TransformPoint(m_characterCtrl.center);

            Vector3 bottom = worldCenter - Vector3.up * (m_characterCtrl.height * 0.5f - m_characterCtrl.skinWidth);

            return Physics.CheckSphere(bottom, m_groundDistance, m_groundMask);
        }
    }
}