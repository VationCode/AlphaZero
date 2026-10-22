using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>Canon의 생존 시간이 끝나면 파괴 외형과 물리 파편으로 전환한다.</summary>
    public sealed class CanonLifetime : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private GameObject _createEffect;
        [SerializeField] private Transform _intactRoot;
        [SerializeField] private Transform _brokenRoot;
        [SerializeField] private GameObject _breakEffectPrefab;

        [Header("Lifetime")]
        [SerializeField, Min(0f)] private float _survivalTime = 15f;
        [SerializeField, Min(0f)] private float _debrisLifetime = 2f;

        [Header("Break Physics")]
        [SerializeField, Min(0f)] private float _breakImpulse = 1f;
        [SerializeField, Min(0f)] private float _breakTorque = 0.25f;

        private bool _isBroken;

        public bool IsBroken => _isBroken;

        private void Awake()
        {
            if (_intactRoot == null || _brokenRoot == null || _intactRoot == _brokenRoot)
            {
                Debug.LogError($"{nameof(CanonLifetime)}: 일반/파괴 포탑 자식 참조를 확인하세요.", this);
                enabled = false;
                return;
            }

            _intactRoot.gameObject.SetActive(true);
            _brokenRoot.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (!_isBroken) Invoke(nameof(Break), _survivalTime);
            Destroy(_createEffect, 2.2f);
        }

        private void OnDisable() => CancelInvoke(nameof(Break));

        // 시간이 끝나거나 포탑이 파괴될 때 한 번만 호출한다.
        public void Break()
        {
            if (_isBroken || _intactRoot == null || _brokenRoot == null) return;
            _isBroken = true;
            CancelInvoke(nameof(Break));

            _intactRoot.gameObject.SetActive(false);
            _brokenRoot.gameObject.SetActive(true);

            /*foreach (Rigidbody body in _brokenRoot.GetComponentsInChildren<Rigidbody>())
            {
                Vector3 direction = body.worldCenterOfMass - transform.position;
                if (direction.sqrMagnitude < 0.001f) direction = Random.insideUnitSphere;
                direction.y = Mathf.Max(direction.y, 0.25f);
                body.AddForce(direction.normalized * _breakImpulse, ForceMode.Impulse);
                body.AddTorque(Random.onUnitSphere * _breakTorque, ForceMode.Impulse);
            }*/

            if (_breakEffectPrefab != null)
            {
                GameObject effect = Instantiate(_breakEffectPrefab, transform.position, transform.rotation);
                Destroy(effect, _debrisLifetime);
            }

            Destroy(gameObject, _debrisLifetime);
        }
    }
}
