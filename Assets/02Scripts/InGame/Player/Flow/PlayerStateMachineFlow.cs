using alpha.player.flow.locomotion;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace alpha.player.flow
{
    public enum LocomotionStateType
    {
        //Idle,
        Move,
        JumpUp,
        Fall,
        Land,
        DashStart,
        DashEnd
    }

    public class PlayerStateMachineFlow : MonoBehaviour
    {
        private PlayerCore m_playerCore;

        [SerializeField]
        private TextMeshProUGUI m_locoStateText;

        private PlayerStateBase m_locoState;
        private PlayerStateBase m_cobatState;
        private Dictionary<LocomotionStateType, Func<PlayerStateBase>> m_locomotionStateCreateDic;


        public LocomotionStateType m_currentLocoStateType { get; private set; }

        public void Bind(PlayerCore playerCore)
        {
            m_playerCore = playerCore;

            // 일반 new로 작성시 상태가 현재 Dic 선언시에 객체로 저장이 되어져 그저 해당 상태를 재사용하는꼴임
            // Func을 통한 함수로써 객체를 만든다의 방식은 완전한 새로운 객체를 생성해내는 것
            m_locomotionStateCreateDic = new Dictionary<LocomotionStateType, Func<PlayerStateBase>>()
            {
                //{ LocomotionStateType.Idle, () => new IdleState() },
                { LocomotionStateType.Move, () => new MoveState() },
                { LocomotionStateType.JumpUp, () => new JumpUpState() },
                { LocomotionStateType.Fall, () => new FallState() },
                { LocomotionStateType.Land, () => new LandState() },
                { LocomotionStateType.DashStart, () => new DashState() },
                { LocomotionStateType.DashEnd, () => new DashEndState() }
            };

            m_locoState = m_locomotionStateCreateDic[LocomotionStateType.Move]();
            
        }

        private void Update()
        {
            if (m_locoState == null || m_playerCore == null) return;

            m_locoStateText.text = $"{m_currentLocoStateType}";

            m_locoState.Update(m_playerCore);
            //m_cobatState.Update(m_playerCore);
        }

        public void ChangeLocoState(LocomotionStateType newState)
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