using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>소환물의 지속시간과 종료 시 파괴/비활성화를 관리한다.</summary>
    public sealed class SummonLifetime : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private GameObject _createEffect;
        [SerializeField] private Transform _intactRoot; // 소환 중 보여줄 정상 외형
        [SerializeField] private Transform _brokenRoot; // 파괴 시 정상 외형 대신 보여줄 외형
        [SerializeField] private GameObject _breakEffectPrefab;

        [Header("Lifetime")]
        [SerializeField, Min(0f)] private float _survivalTime = 15f;
        [SerializeField] private bool _destroyOnExpire = true;
        [Tooltip("부서지는 시간"),SerializeField, Min(0f)] private float _debrisLifetime = 2f;

        private bool _isExpired;

        public bool IsExpired => _isExpired;

        private void Awake()
        {
            if (_destroyOnExpire && ((_intactRoot == null) != (_brokenRoot == null) ||
                _intactRoot != null && _intactRoot == _brokenRoot))
            {
                Debug.LogError($"{nameof(SummonLifetime)}: 파괴 외형 참조를 확인하세요.", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            _isExpired = false;
            // Unity 참조의 빈 값은 ?. 대신 Unity의 null 비교로 확인한다.
            if (_intactRoot != null) _intactRoot.gameObject.SetActive(true);
            if (_brokenRoot != null) _brokenRoot.gameObject.SetActive(false);
            Invoke(nameof(Expire), Mathf.Max(0f, _survivalTime));
            if (_createEffect != null && _destroyOnExpire) Destroy(_createEffect, 2.2f);
        }

        private void OnDisable() => CancelInvoke(nameof(Expire));

        // 외부에서 조기 종료해도 한 번만 처리한다.
        public void Expire()
        {
            if (_isExpired) return;
            _isExpired = true;
            CancelInvoke(nameof(Expire));

            if (!_destroyOnExpire)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_intactRoot != null) _intactRoot.gameObject.SetActive(false);
            if (_brokenRoot != null) _brokenRoot.gameObject.SetActive(true);

            if (_breakEffectPrefab != null)
            {
                GameObject effect = Instantiate(_breakEffectPrefab, transform.position, transform.rotation);
                Destroy(effect, _debrisLifetime);
            }

            Destroy(gameObject, _debrisLifetime);
        }
    }
}
