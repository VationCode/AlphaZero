// RootComposition: 전체 조립 / DI
// Boundary: 외부와 연결(입력 / 출력 전달만)
// Domain : 데이터 / 개념
// Flow: 상태 / 흐름 / 의사결정
// Module: 기능 실행

using UnityEngine;
using alpha.input;
using alpha.player.anim;
using alpha.player.locomotion;
using alpha.player.combat;


// 플레이어의 전체적인 연결 관리
namespace alpha.player
{
    public class PlayerCore : MonoBehaviour
    {
        // 외부 Bind
        public AlphaInputSystem Input { get; private set; }


        public AnimationView AnimView { get; private set; }

        public LocomotionContext LocomotionContext { get; private set; } = new();
        public CombatContext CombatContext { get; private set; } = new();

        // Flow
        public LocomotionFlow LocomotionFlow { get; private set; }
        public CombatFlow CombatFlow { get; private set; }
        public CombatModeFlow CombatModeFlow { get; private set; }

        // Module
        public LocomotionModule LocomotionModule { get; private set; }
        public CombatModule CombatModule { get; private set; }

        private void Awake()
        {
            AnimView = GetComponentInChildren<AnimationView>(true);

            LocomotionFlow = GetComponentInChildren<LocomotionFlow>(true);
            CombatFlow = GetComponentInChildren<CombatFlow>(true);
            CombatModeFlow = GetComponentInChildren<CombatModeFlow>(true);

            LocomotionModule = GetComponentInChildren<LocomotionModule>(true);
            CombatModule = GetComponentInChildren<CombatModule>(true);
        }

        public void Bind(AlphaInputSystem p_inputSystem)
        {
            Input = p_inputSystem;
        }

        private void Start()
        {
            LocomotionFlow.Bind(this);
            LocomotionModule.Bind(LocomotionContext);

            CombatFlow.Bind(this);
            CombatModule.Bind(CombatContext);
            CombatModeFlow.Bind(CombatContext);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}