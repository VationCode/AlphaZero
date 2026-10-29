using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>Animator 오브젝트의 공격 이벤트를 부모 AnimationView에 전달한다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossAttackAnimationEventRelay : MonoBehaviour
    {
        private BossAnimationView _view;

        private void Awake() => _view = GetComponentInParent<BossAnimationView>();

        public void JumpStart(AnimationEvent animationEvent) =>
            _view?.NotifyJumpStart(animationEvent);
        public void JumpEnd() => _view?.NotifyJumpEnd();
        public void ButtStart(AnimationEvent animationEvent) =>
            _view?.NotifyButtStart(animationEvent);
        public void ButtEnd() => _view?.NotifyButtEnd();

        // 클립의 String Parameter에 인스펙터 Event Key(예: Effect4)를 지정한다.
        public void EffectStart(string eventKey) => _view?.NotifyEffect(eventKey, true);
        public void EffectEnd(string eventKey) => _view?.NotifyEffect(eventKey, false);
        public void EffectSpawn(string eventKey) => _view?.NotifyEffectSpawn(eventKey);
        public void TargetingStart() => _view?.NotifyTargetingStart();

        // 피해 판정은 추후 구현한다. 클립에 준비된 이벤트만 수신한다.
        public void CollisionStart() { }
        public void CollisionEnd() { }

        // Attack6의 발사 처리는 추후 연결한다. 현재는 이벤트만 수신한다.
        public void Fire() { }
    }
}
