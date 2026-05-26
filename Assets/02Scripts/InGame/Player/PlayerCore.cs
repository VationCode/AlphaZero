// RootComposition: 전체 조립 / DI
// Boundary: 외부와 연결(입력 / 출력 전달만)
// Domain : 데이터 / 개념
// Flow: 상태 / 흐름 / 의사결정
// Module: 기능 실행

using UnityEngine;
using alpha.input;
using alpha.player.anim;
using alpha.player.contorller;
using alpha.player.state;
using alpha.player.locomotion;
using alpha.player.combat;
using alpha.player.equipment;

// 플레이어의 전체적인 연결 관리
namespace alpha.player
{
    [RequireComponent(typeof(PlayerCharacterControllerBoudary))]
    [RequireComponent(typeof(PlayerAnimationBoundary))]
    [RequireComponent(typeof(PlayerStateMachineFlow))]
    [RequireComponent(typeof(PlayerEquipmentModule))]
    [RequireComponent(typeof(PlayerLocomotionModule))]
    [RequireComponent(typeof(PlayerCombatModule))]
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
        public PlayerCombatModule CombatModule { get; private set; }
        public PlayerEquipmentModule EquipmentModule { get; private set; }
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
            CombatModule = GetComponent<PlayerCombatModule>();
            EquipmentModule = GetComponent<PlayerEquipmentModule>();
        }

        public void Bind(InputSystemBoundary p_inputSystemBoundary)
        {
            InputSystemBoundary = p_inputSystemBoundary;

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