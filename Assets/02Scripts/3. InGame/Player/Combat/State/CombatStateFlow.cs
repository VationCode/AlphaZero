using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>Unity 생명주기와 자세·행동 Flow를 연결한다.</summary>
    [DefaultExecutionOrder(-100)]
    public class CombatStateFlow : MonoBehaviour
    {
        private PlayerCore _playerCore;

        public CombatStanceFlow Stance { get; private set; }
        public CombatActionFlow Action { get; private set; }

        private void Awake() => _playerCore = GetComponentInParent<PlayerCore>(true);

        private void Start()
        {
            Stance = new CombatStanceFlow(_playerCore);
            Action = new CombatActionFlow(_playerCore);
            SubscribeSkill();
        }

        private void OnEnable()
        {
            if (Action != null) SubscribeSkill();
        }

        private void Update() => Action?.Tick();

        private void OnDisable()
        {
            if (_playerCore?.SkillStateFlow != null)
                _playerCore.SkillStateFlow.ExecutionEnded -= HandleSkillEnded;
            Action?.CancelAll();
        }

        public void CancelForAirState() => Action?.CancelAll();

        private void SubscribeSkill()
        {
            if (_playerCore?.SkillStateFlow == null) return;
            _playerCore.SkillStateFlow.ExecutionEnded -= HandleSkillEnded;
            _playerCore.SkillStateFlow.ExecutionEnded += HandleSkillEnded;
        }

        private void HandleSkillEnded() => Action?.EndSkill();
    }
}
