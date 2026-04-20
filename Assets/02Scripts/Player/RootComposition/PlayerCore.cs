// RootComposition : 시스템의 최상위 클래스
// Boundary : 외부와의 연결점
// Module : 기능들의 집합체
using UnityEngine;
using player.boundary;
using player.module;

// 플레이어의 전체적인 연결성 관리
namespace alpha.player
{
    [RequireComponent(typeof(PlayerLocomotionModule))]
    public class PlayerCore : MonoBehaviour
    {
        #region Boundary
        [SerializeField] private PlayerInputBoundary m_inputBoundary;
        #endregion

        #region Module
        [SerializeField] private PlayerLocomotionModule m_locomotionModule;
        #endregion

        void Start()
        {
            Bind();
        }

        void Bind()
        {
            m_locomotionModule.Bind(m_inputBoundary);
        }
        // Update is called once per frame
        void Update()
        {
            
        }
    }
}