using UnityEngine;

namespace alpha.ingame.item
{
    /// <summary>투사체 이동과 충돌을 처리하고 사용 후 풀에 반환한다.</summary>
    public class GunProjectile : MonoBehaviour
    {
        private static readonly RaycastHit[] Hits = new RaycastHit[32];

        [SerializeField, Min(0.01f)] private float _speed = 60f;
        [SerializeField, Min(0.01f)] private float _lifetime = 3f;
        [SerializeField] private SphereCollider _collisionCollider;

        private GunProjectilePool _pool;
        private ImpactPool _impactPool;
        private Transform _owner;
        private Vector3 _direction;
        private float _damage;
        private float _remainingDistance;
        private float _expiresAt;

        public void Launch(GunProjectilePool pool, ImpactPool impactPool,
            Transform owner, Vector3 position, Vector3 direction, float damage, float maxDistance)
        {
            _pool = pool;
            _impactPool = impactPool;
            _owner = owner;
            _direction = direction.normalized;
            _damage = damage;
            _remainingDistance = maxDistance;
            _expiresAt = Time.time + Mathf.Max(0.01f, _lifetime);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(_direction));
            gameObject.SetActive(true);

            // 콜라이더는 실제 물리 충돌 대신 이동 경로 검사에 사용할 모양을 제공한다.
            if (_collisionCollider == null)
                _collisionCollider = GetComponentInChildren<SphereCollider>(true);
            if (_collisionCollider == null)
            {
                Debug.LogError($"{name}: 투사체에 SphereCollider가 필요합니다.", this);
                _pool.Return(this);
            }
        }

        private void Update()
        {
            if (_pool == null) return;
            if (Time.time >= _expiresAt || _remainingDistance <= 0f)
            {
                _pool.Return(this);
                return;
            }

            float distance = Mathf.Min(Mathf.Max(0.01f, _speed) * Time.deltaTime, _remainingDistance);
            Vector3 scale = _collisionCollider.transform.lossyScale;
            float radius = _collisionCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Vector3 center = _collisionCollider.transform.TransformPoint(_collisionCollider.center);
            int count = Physics.SphereCastNonAlloc(center, radius, _direction, Hits,
                distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            int nearest = -1;
            float nearestDistance = distance;
            for (int i = 0; i < count; i++)
            {
                Collider collider = Hits[i].collider;
                if (collider == null || (_owner != null && collider.transform.IsChildOf(_owner))) continue;
                if (nearest >= 0 && Hits[i].distance >= nearestDistance) continue;
                nearest = i;
                nearestDistance = Hits[i].distance;
            }

            if (nearest >= 0)
            {
                _impactPool?.Spawn(Hits[nearest], _owner, _damage, -_direction);
                _pool.Return(this);
                return;
            }

            transform.position += _direction * distance;
            _remainingDistance -= distance;
        }
    }
}
