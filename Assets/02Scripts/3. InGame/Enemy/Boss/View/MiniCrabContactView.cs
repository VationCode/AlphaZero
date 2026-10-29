using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>소환된 MiniCrab의 플레이어 접촉을 폭발 표현으로 전환한다.</summary>
    [RequireComponent(typeof(SphereCollider))]
    public sealed class MiniCrabContactView : MonoBehaviour
    {
        [SerializeField] private LayerMask _playerLayers = 1 << 7;
        [SerializeField] private Transform _model;
        [SerializeField] private Transform _explosion;

        private SphereCollider _trigger;
        private MiniCrabMotionModule _motion;
        private bool _exploded;

        private void Awake()
        {
            _trigger = GetComponent<SphereCollider>();
            _motion = GetComponentInParent<MiniCrabMotionModule>();
            if (_model != null && _explosion != null) return;
            Debug.LogError($"{name}: MiniCrab 모델 또는 폭발 이펙트가 연결되지 않았습니다.", this);
            enabled = false;
        }

        private void Start()
        {
            if (!_exploded && _explosion != null)
                _explosion.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_exploded || other == null) return;
            CharacterController player = other as CharacterController;
            if (player == null || !player.enabled ||
                (_playerLayers.value & (1 << player.gameObject.layer)) == 0) return;

            _exploded = true;
            _motion?.Stop();
            _trigger.enabled = false; // 중복 폭발을 막는다.
            _model.gameObject.SetActive(false);
            _explosion.gameObject.SetActive(true);
            foreach (ParticleSystem particle in _explosion.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (!particle.gameObject.activeInHierarchy) continue;
                particle.Clear();
                particle.Play();
            }
        }
    }
}
