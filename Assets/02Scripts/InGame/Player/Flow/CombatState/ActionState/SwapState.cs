using alpha.player.state;
using Unity.VisualScripting;
using UnityEngine;

namespace alpha.player.combat
{
    public class SwapState : PlayerStateBase
    {
        private float m_timer;
        private float m_waitingTime = 0.2f; // 애니 길이에 맞춤

        private float m_nextTimer;
        private float m_nextWaitingTime = 0.6f; // 애니 길이에 맞춤
        public override void Enter(PlayerCore p_playerCore)
        {
            m_timer = 0;
            m_nextTimer = 0;

            var anim = p_playerCore.AnimBoundary;
            var equip = p_playerCore.EquipmentModule;

            // Swap Layer로 블렌드
            anim.ChangeLayer(3);

            anim.SwapAnim(equip.GetCurrentSwapNum());

            // 새 무기 Layer로 부드럽게 전환
            anim.ChangeLayer(equip.GetCurrentSwapNum());
        }

        public override void Update(PlayerCore p_playerCore)
        {
            m_timer += Time.deltaTime;

            var anim = p_playerCore.AnimBoundary;
            var equip = p_playerCore.EquipmentModule;

            // Swap 진행 중에는 Swap Layer Blend
            if (m_timer < m_waitingTime)
            {
                anim.BlendLayers(3);
            }

            // 무기 실제 교체
            if (m_timer >= m_waitingTime)
            {
                equip.OnSwap(equip.GetCurrentSwapNum());

                // 새 무기 Layer로 Blend
                anim.BlendLayers(equip.GetCurrentSwapNum());

                m_nextTimer += Time.deltaTime;
            }

            // 종료
            if (m_nextTimer >= m_nextWaitingTime)
            {
                p_playerCore.StateMachineFlow.ChangeCombatActionState(ECombatActionStateType.None);
            }
        }

        public override void Exit(PlayerCore p_playerCore)
        {
            var anim = p_playerCore.AnimBoundary;
            var equip = p_playerCore.EquipmentModule;

            
        }
    }
}