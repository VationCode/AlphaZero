// RootComposition: 전체 조립 / DI
// Boundary: 외부와 연결(입력 / 출력 전달만)
// Domain : 데이터 / 개념
// Flow: 상태 / 흐름 / 의사결정
// Module: 기능 실행

using UnityEngine;
using alpha.input;
using alpha.player.anim;
using alpha.player.locomotion;


// 플레이어의 전체적인 연결 관리
namespace alpha.player
{
    public class PlayerCore : MonoBehaviour
    {
        // 외부 Bind
        public AlphaInputSystem InputSystemBoundary { get; private set; }


        public AnimationView AnimView { get; private set; }

        // Flow


        // Module
        public LocomotionModule LocomotionModule { get; private set; }


        private void Awake()
        {
            AnimView = GetComponent<AnimationView>();
           

            // Module
            LocomotionModule = GetComponent<LocomotionModule>();

        }

        public void Bind(AlphaInputSystem p_inputSystemBoundary)
        {
            InputSystemBoundary = p_inputSystemBoundary;

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