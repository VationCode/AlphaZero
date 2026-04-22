// RootComposition : 시스템의 최상위 클래스
// Boundary : 외부와의 연결점
// Module : 기능들의 집합체
using UnityEngine;
using alpha.player.boundary;
using alpha.player.module;

// 플레이어의 전체적인 연결성 관리
namespace alpha.player
{
    [RequireComponent(typeof(PlayerAnimationBoundary))]
    [RequireComponent(typeof(PlayerInputBoundary))]
    [RequireComponent(typeof(PlayerLocomotionModule))]
    public class PlayerCore : MonoBehaviour
    {
        #region Boundary
        public PlayerInputBoundary m_inputBoundary;
        public PlayerAnimationBoundary m_aniBoundary;
        #endregion

        #region Module
        [SerializeField] private PlayerLocomotionModule m_locomotionModule;
        #endregion

        private void Awake()
        {
            m_inputBoundary = GetComponent<PlayerInputBoundary>();
            m_aniBoundary = GetComponent<PlayerAnimationBoundary>();

            m_locomotionModule = GetComponent<PlayerLocomotionModule>();

            Bind();
        }

        void Start()
        {
            
        }

        void Bind()
        {
            m_locomotionModule.Bind(m_inputBoundary, m_aniBoundary);
        }
        // Update is called once per frame
        void Update()
        {
            
        }
    }
}