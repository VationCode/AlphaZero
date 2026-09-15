using UnityEngine;

namespace alpha.player.combat
{
    public enum ECombatMode
    {
        OutCombat,
        InCombat
    }
    public class CombatContext
    {
        public ECombatMode CurrentMode { get; private set; }
        public ECombatStateType CurrentStateType { get; private set; }
        public bool IsInCombat => CurrentMode == ECombatMode.InCombat;

        public void SetCombatMode(ECombatMode p_mode)
        {
            CurrentMode = p_mode;
        }

        public void SetCurrentStateType(ECombatStateType p_stateType)
        {
            CurrentStateType = p_stateType;
        }

    }
}