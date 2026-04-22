// RootComposition : 조립(최상위)
// Boundary : 입력/외부 이벤트
// Flow : 흐름 제어
// Module : 기능 수행

using UnityEngine;
using alpha.player.boundary;
using alpha.player.module;

// 플레이어의 전체적인 연결 관리
namespace alpha.player
{
    [RequireComponent(typeof(PlayerAnimationBoundary))]
    [RequireComponent(typeof(PlayerLocomotionModule))]
    public class PlayerCore : MonoBehaviour
    {
        // Event
        private InputSystemBoundary m_inputSystemBoundary;
        
        [SerializeField] private PlayerAnimationBoundary m_aniBoundary;

        [SerializeField] private PlayerLocomotionModule m_locomotionModule;
        private void Awake()
        {
            m_aniBoundary = GetComponent<PlayerAnimationBoundary>();

            m_locomotionModule = GetComponent<PlayerLocomotionModule>();
        }

        public void Bind(InputSystemBoundary inputSystemBoundary)
        {
            m_inputSystemBoundary = inputSystemBoundary;

            m_locomotionModule.Bind(m_inputSystemBoundary, m_aniBoundary);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}