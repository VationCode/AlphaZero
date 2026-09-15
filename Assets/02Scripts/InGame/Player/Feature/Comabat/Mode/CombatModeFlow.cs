
using UnityEngine;

namespace alpha.player.combat
{
    // 공격이 끝난 뒤부터 일정 시간 동안 전투 상태를 유지
    // 현재 전투 중인가를 관리
    public class CombatModeFlow : MonoBehaviour
    {
        private CombatContext _context;

        [SerializeField]
        private float _combatDuration = 3f;
        
        private float _combatRemainTime;
        private bool _isCombatActionRunning;

        public void Bind(CombatContext p_context)
        {
            _context = p_context;

            Initialize();
        }

        private void Initialize()
        {
            _context.SetCombatMode(ECombatMode.OutCombat);

            _combatRemainTime = 0f;
            _isCombatActionRunning = false;
        }

        private void Update()
        {
            if (!_context.IsInCombat)
                return;

            // 실제 전투 행동 중이라면 유지시간을 감소시키지 않는다.
            if (_isCombatActionRunning)
                return;

            _combatRemainTime -= Time.deltaTime;

            if (_combatRemainTime <= 0f)
            {
                ChangeMode(ECombatMode.OutCombat);
            }
        }

        public void BeginCombatAction()
        {
            _isCombatActionRunning = true;

            ChangeMode(ECombatMode.InCombat);
        }

        public void EndCombatAction()
        {
            _isCombatActionRunning = false;

            RefreshCombatTime();
        }

        private void RefreshCombatTime()
        {
            _combatRemainTime = _combatDuration;
        }

        private void ChangeMode(ECombatMode p_mode)
        {
            if (_context.CurrentMode == p_mode)
                return;

            _context.SetCombatMode(p_mode);
        }
    }
}