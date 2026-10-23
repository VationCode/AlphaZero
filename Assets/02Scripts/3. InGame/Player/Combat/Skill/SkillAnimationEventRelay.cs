using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>실리온 Animator의 스킬 및 재장전 이벤트 키를 담당 객체에 전달한다.</summary>
    [DisallowMultipleComponent]
    public sealed class SkillAnimationEventRelay : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        private PlayerCore _playerCore;
        private SkillModule _skillModule;

        private PlayerCore Player => _playerCore != null
            ? _playerCore : _playerCore = GetComponentInParent<PlayerCore>();

        private SkillModule Module
        {
            get
            {
                if (_skillModule != null) return _skillModule;
                PlayerCore player = Player;
                return _skillModule = player != null
                    ? player.GetComponentInChildren<SkillModule>(true) : null;
            }
        }

        // 클립의 String Parameter를 현재 스킬 데이터의 이벤트 키와 대조한다.
        public void OnSkillEvent(string eventKey) => Module?.HandleAnimationEvent(eventKey, _audioSource);

        // 현재 장착 중인 총만 Reload 이벤트를 처리한다.
        public void OnReloadEvent(string eventKey) =>
            Player?.CombatModule?.CurrentGun?.HandleReloadAnimationEvent(eventKey);
    }
}
