using System.Collections.Generic;
using UnityEngine;

namespace alpha.player.combat
{
    public enum ECombatStateType
    {
        Idle,
        Attack,
        Skill,
    }

    public class CombatFlow : MonoBehaviour
    {
        private PlayerCore _Core;

        private CombatState _currentState;
        private ECombatStateType _currentStateType;
        private Dictionary<ECombatStateType, CombatState> _stateDict;

        public void Bind(PlayerCore p_core)
        {
            _Core = p_core;

            Initialize();
        }

        private void Initialize()
        {
            _stateDict = new Dictionary<ECombatStateType, CombatState>
            {
                { ECombatStateType.Idle, new PrimaryIdleState(_Core) },
                //{ ECombatStateType.Attack, new AttackState(_Core) },
                //{ ECombatStateType.Skill, new SkillState(_Core) },
            };

            ChangeState(ECombatStateType.Idle);
        }

        private void Update()
        {
            if(_Core == null) return;
            _currentState?.Update();
        }

        public void ChangeState(ECombatStateType p_newStateType)
        {
            if(_currentState == null)
            {
                _currentState = _stateDict[p_newStateType];
                _currentStateType = p_newStateType;
                _Core.CombatContext.SetCurrentStateType(p_newStateType);
                _currentState.Enter();
            }
            else if(_currentStateType != p_newStateType)
            {
                _currentState.Exit();
                _currentState = _stateDict[p_newStateType];
                _currentStateType = p_newStateType;
                _Core.CombatContext.SetCurrentStateType(p_newStateType);
                _currentState.Enter();
            }
        }
    }
}