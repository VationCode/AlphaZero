using alpha.input;
using UnityEngine;

namespace alpha.player.combat
{
    public class PlayerCombatFlow : MonoBehaviour
    {
        public bool IsAttackPressed { get; private set; }
        public bool IsAimPressed { get; private set; }
        public bool IsSwapPressed { get; private set; }

        public void UpdateInput(InputSystemBoundary p_input)
        {
            IsSwapPressed = p_input.IsSwapInput;
            IsAttackPressed = p_input.IsAttackInput;
            //IsAimPressed = p_input.IsAimInput;
        }
    }
}