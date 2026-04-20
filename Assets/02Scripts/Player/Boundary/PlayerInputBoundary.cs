// Boundary : 외부와의 연결점
using UnityEngine;
using UnityEngine.InputSystem;

namespace player.boundary
{
    public class PlayerInputBoundary : MonoBehaviour
    {
        private PlayerInputAction m_inputAction;

        #region LocomotionInput
        public Vector2 MoveDirection { get; private set; }
        #endregion

        #region CombatInput

        #endregion
        void OnEnable()
        {
            if (m_inputAction == null)
            {
                m_inputAction = new PlayerInputAction();

                m_inputAction.Player.Move.performed += i => MoveDirection = i.ReadValue<Vector2>();
                if(MoveDirection.sqrMagnitude > 1.0)
                {
                    MoveDirection.Normalize();
                }

                // 활성화해야 동작
                m_inputAction.Enable();
            }
        }
    }
}