using UnityEngine;

namespace alpha.player.combat
{
    public class PlayerCombatModule : MonoBehaviour
    {
        public bool IsCombatMode => m_combatTimer > 0f;
        private float m_combatTimer;

        [SerializeField]
        private float m_combatDuration = 4f;

        public void EnterCombat()
        {
            m_combatTimer = m_combatDuration;
        }

        public void UpdateCombat()
        {
            if (m_combatTimer <= 0f)
                return;

            m_combatTimer -= Time.deltaTime;
        }

        public void ClearCombatTimer()
        {
            m_combatTimer = 0f;
        }
        public void Attack()
        {

        }
    }
}