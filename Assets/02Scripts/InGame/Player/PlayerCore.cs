// RootComposition : 조립(최상위)
// Boundary : 입력/외부 이벤트
// Flow : 흐름 제어
// Module : 기능 수행

using UnityEngine;
using alpha.player.boundary;
using alpha.player.module;
using alpha.player.flow;

// 플레이어의 전체적인 연결 관리
namespace alpha.player
{
    [RequireComponent(typeof(PlayerAnimationBoundary))]
    [RequireComponent(typeof(PlayerStateMachineFlow))]
    [RequireComponent(typeof(PlayerLocomotionModule))]
    public class PlayerCore : MonoBehaviour
    {
        // 외부 Event
        public InputSystemBoundary InputSystemBoundary { get; private set; }
        
        // 내부
        [SerializeField] private PlayerAnimationBoundary m_aniBoundary;

        public PlayerStateMachineFlow StateMachineFlow { get; private set; }

        public PlayerLocomotionModule LocomotionModule { get; private set; }
        private void Awake()
        {
            m_aniBoundary = GetComponent<PlayerAnimationBoundary>();

            StateMachineFlow = GetComponent<PlayerStateMachineFlow>();

            LocomotionModule = GetComponent<PlayerLocomotionModule>();
        }

        public void Bind(InputSystemBoundary inputSystemBoundary)
        {
            InputSystemBoundary = inputSystemBoundary;

            StateMachineFlow.Bind(this);
            LocomotionModule.Bind(InputSystemBoundary, m_aniBoundary);
        }

        private void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}