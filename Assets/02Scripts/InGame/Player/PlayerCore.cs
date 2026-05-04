// RootComposition : 조립(최상위)
// Boundary : 외부 이벤트 입력/출력(수행)
// Flow : 흐름 제어
// Module : 기능 연산 위주, 수행은 플젝 작을 경우 같이해도 무방

using UnityEngine;
using alpha.player.boundary;
using alpha.player.module;
using alpha.player.flow;

// 플레이어의 전체적인 연결 관리
namespace alpha.player
{
    [RequireComponent(typeof(PlayerCharacterControllerBoudary))]
    [RequireComponent(typeof(PlayerAnimationBoundary))]
    [RequireComponent(typeof(PlayerStateMachineFlow))]
    [RequireComponent(typeof(PlayerLocomotionModule))]
    public class PlayerCore : MonoBehaviour
    {
        // 외부 Bind
        public InputSystemBoundary InputSystemBoundary { get; private set; }

        #region 내부
        // Boundary
        public PlayerAnimationBoundary AnimBoundary { get; private set; }
        public PlayerCharacterControllerBoudary CharacterCtrlBoudary { get; private set; }

        // Flow
        public PlayerStateMachineFlow StateMachineFlow { get; private set; }

        // Module
        public PlayerLocomotionModule LocomotionModule { get; private set; }
        #endregion

        private void Awake()
        {
            // Boundary
            AnimBoundary = GetComponent<PlayerAnimationBoundary>();
            CharacterCtrlBoudary = GetComponent<PlayerCharacterControllerBoudary>();
            
            // Flow
            StateMachineFlow = GetComponent<PlayerStateMachineFlow>();

            // Module
            LocomotionModule = GetComponent<PlayerLocomotionModule>();
        }

        public void Bind(InputSystemBoundary inputSystemBoundary)
        {
            InputSystemBoundary = inputSystemBoundary;

            StateMachineFlow.Bind(this);
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