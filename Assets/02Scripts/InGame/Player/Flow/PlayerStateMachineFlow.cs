using alpha.player.locomotion;
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
        None,
        Swap,
        InCombat,
        Skill
    }

    public class PlayerStateMachineFlow : MonoBehaviour
    {
        private PlayerCore m_playerCore;

        [SerializeField]
        private TextMeshProUGUI m_locoStateText;

        private PlayerStateBase m_locoState;
        private PlayerStateBase m_cobatState;
        private Dictionary<ELocomotionStateType, Func<PlayerStateBase>> m_locomotionStateCreateDic;


        public ELocomotionStateType m_currentLocoStateType { get; private set; }

        public void Bind(PlayerCore playerCore)
        {
            m_playerCore = playerCore;

            // 일반 new로 작성시 상태가 현재 Dic 선언시에 객체로 저장이 되어져 그저 해당 상태를 재사용하는꼴임
            // Func을 통한 함수로써 객체를 만든다의 방식은 완전한 새로운 객체를 생성해내는 것
            m_locomotionStateCreateDic = new Dictionary<ELocomotionStateType, Func<PlayerStateBase>>()
            {
                //{ LocomotionStateType.Idle, () => new IdleState() },
                { ELocomotionStateType.Move, () => new MoveState() },
                { ELocomotionStateType.JumpUp, () => new JumpUpState() },
                { ELocomotionStateType.Fall, () => new FallState() },
                { ELocomotionStateType.Land, () => new LandState() },
                { ELocomotionStateType.Dash, () => new DashState() },
                { ELocomotionStateType.FlyUp, () => new FlyUpState() },
                { ELocomotionStateType.Flight, () => new FlightState() }
            };

            m_locoState = m_locomotionStateCreateDic[ELocomotionStateType.Move]();
            
        }

        private void Update()
        {
            if (m_locoState == null || m_playerCore == null) return;

            m_locoStateText.text = $"{m_currentLocoStateType}";

            m_locoState.Update(m_playerCore);
            //m_cobatState.Update(m_playerCore);
        }

        public void ChangeLocoState(ELocomotionStateType newState)
        {
            if (m_currentLocoStateType == newState) return;

            m_locoState?.Exit(m_playerCore);
            m_locoState = m_locomotionStateCreateDic[newState]();
            m_currentLocoStateType = newState;

            m_locoState.Enter(m_playerCore);
        }
        public void ChangeCobatState()
        {
        }
    }
}