using UnityEngine;

namespace alpha.player.combat
{
    public class CombatModule : MonoBehaviour
    {
        private CombatContext _combatContext;

        public void Bind(CombatContext p_context)
        {
            _combatContext = p_context;
        }
    }
}