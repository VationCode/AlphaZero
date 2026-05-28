using alpha.player.locomotion;
using alpha.player.combat;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace alpha.player.state
{
    public enum ELocomotionStateType
    {
        Move,
        JumpUp,
        Fall,
        Land,
        Dash,
        FlyUp,
        Flight
    }

    public enum ECombatStateType
    {
        NoneCombat,
        InCombat,
    }

    public enum  ECombatActionStateType
    {
        None,
        Swap,
        Attack,
        Skill
    }
    [Flags]
    public enum EBlockedCombatAction
    {
        None = 0,
        InCombat = 1 << 0,
        Swap = 1 << 1,
        Attack = 1 << 2,
        Skill = 1 << 3,
    }

    [Flags]
    public enum EBlockedLocomotionAction
    {
        None = 0,
        Move = 1 << 0,
        Jump = 1 << 1,
        Fall = 1 << 2,
        Land = 1 << 3,
        Dash = 1 << 4,
        FlyUp = 1 << 5,
        Flight = 1 << 6,
    }

    public class PlayerStateMachineFlow : MonoBehaviour
    {
        private PlayerCore m_playerCore;

        [SerializeField]
        private TextMeshProUGUI m_locoStateText;
        [SerializeField]
        private TextMeshProUGUI m_combatStateText;
        [SerializeField]
        private TextMeshProUGUI m_combatActionStateText;


        private PlayerStateBase m_locoState;
        private Dictionary<ELocomotionStateType, Func<PlayerStateBase>> m_locomotionStateCreateDic;
        public ELocomotionStateType m_currentLocoStateType { get; private set; }

        private PlayerStateBase m_combatState;
        private Dictionary<ECombatStateType, Func<PlayerStateBase>> m_combatStateCreateDic;
        public ECombatStateType m_currentCombatStateType { get; private set; }

        private PlayerStateBase m_combatActionState;
        private Dictionary<ECombatActionStateType, Func<PlayerStateBase>> m_combatActionStateCreateDic;
        public ECombatActionStateType m_currentCombatActionStateType { get; private set; }


        public void Bind(PlayerCore p_playerCore)
        {
            m_playerCore = p_playerCore;

            // 일반 new로 작성시 상태가 현재 Dic 선언시에 객체로 저장이 되어져 그저 해당 상태를 재사용하는꼴임
            // Func을 통한 함수로써 객체를 만든다의 방식은 완전한 새로운 객체를 생성해내는 것
            m_locomotionStateCreateDic = new Dictionary<ELocomotionStateType, Func<PlayerStateBase>>()
            {
                { ELocomotionStateType.Move, () => new MoveState() },
                { ELocomotionStateType.JumpUp, () => new JumpUpState() },
                { ELocomotionStateType.Fall, () => new FallState() },
                { ELocomotionStateType.Land, () => new LandState() },
                { ELocomotionStateType.Dash, () => new DashState() },
                { ELocomotionStateType.FlyUp, () => new FlyUpState() },
                { ELocomotionStateType.Flight, () => new FlightState() }
            };
            m_locoState = m_locomotionStateCreateDic[ELocomotionStateType.Move]();


            m_combatStateCreateDic = new Dictionary<ECombatStateType, Func<PlayerStateBase>>()
            {
                { ECombatStateType.NoneCombat, () => new NoneCombatState() },
                { ECombatStateType.InCombat, () => new InCombatState() }
            };
            m_combatState = m_combatStateCreateDic[ECombatStateType.NoneCombat]();

            m_combatActionStateCreateDic = new Dictionary<ECombatActionStateType, Func<PlayerStateBase>>()
            {
                {ECombatActionStateType.None,() => new NoneActionState() },
                { ECombatActionStateType.Swap, () => new SwapState() },
                { ECombatActionStateType.Attack, () => new AttackState() }
            };
            m_combatActionState = m_combatActionStateCreateDic[ECombatActionStateType.None]();

            m_currentLocoStateType = ELocomotionStateType.Move;
            m_currentCombatStateType = ECombatStateType.NoneCombat;

            m_locoState.Enter(m_playerCore);
            m_combatState.Enter(m_playerCore);
        }

        private void Update()
        {
            if (m_playerCore == null) return;

            m_playerCore.CombatFlow.UpdateInput(m_playerCore.InputSystemBoundary);

            m_locoStateText.text = $"{m_currentLocoStateType}";

            m_combatStateText.text = $"{m_currentCombatStateType}";

            m_combatActionStateText.text = $"{m_currentCombatActionStateType}";

            m_locoState?.Update(m_playerCore);

            m_combatState?.Update(m_playerCore);

            m_combatActionState?.Update(m_playerCore);
        }

        public void ChangeLocoState(ELocomotionStateType p_newState)
        {
            if (!CanChangeLocomotionState(p_newState)) return;

            if (m_currentLocoStateType == p_newState) return;

            m_locoState?.Exit(m_playerCore);

            m_locoState = m_locomotionStateCreateDic[p_newState]();

            m_currentLocoStateType = p_newState;

            m_locoState.Enter(m_playerCore);
        }
        private bool CanChangeLocomotionState(ELocomotionStateType p_newState)
        {
            EBlockedLocomotionAction blocked =
                m_combatState.BlockedLocoAction;

            switch (p_newState)
            {
                case ELocomotionStateType.Dash:
                    return (blocked &
                        EBlockedLocomotionAction.Dash) == 0;

                case ELocomotionStateType.JumpUp:
                    return (blocked &
                        EBlockedLocomotionAction.Jump) == 0;
            }

            return true;
        }


        public void ChangeCombatState(ECombatStateType p_newState)
        {
            if (!CanChangeCombatState(p_newState)) return;

            if (m_currentCombatStateType == p_newState) return;

            m_combatState?.Exit(m_playerCore);

            m_combatState = m_combatStateCreateDic[p_newState]();

            m_currentCombatStateType = p_newState;

            m_combatState.Enter(m_playerCore);
        }
        private bool CanChangeCombatState(ECombatStateType p_newState)
        {
            EBlockedCombatAction blocked = m_locoState.BlockedCombatAction;

            switch (p_newState)
            {
                
            }
            return true;
        }

        public void ChangeCombatActionState(ECombatActionStateType p_newState)
        {
            if (!CanChangeCombatActionState(p_newState)) return;
            if (m_currentCombatActionStateType == p_newState) return;
            m_combatActionState?.Exit(m_playerCore);
            m_combatActionState = m_combatActionStateCreateDic[p_newState]();
            m_currentCombatActionStateType = p_newState;
            m_combatActionState.Enter(m_playerCore);
        }
        private bool CanChangeCombatActionState(ECombatActionStateType p_newState)
        {
            EBlockedCombatAction blocked = m_locoState.BlockedCombatAction | m_combatState.BlockedCombatAction;
            switch (p_newState)
            {
                case ECombatActionStateType.Attack:
                    return (blocked &
                        EBlockedCombatAction.Attack) == 0;
                case ECombatActionStateType.Skill:
                    return (blocked &
                        EBlockedCombatAction.Skill) == 0;
                case ECombatActionStateType.Swap:
                    return (blocked &
                        EBlockedCombatAction.Swap) == 0;
            }
            return true;
        }
    }
}