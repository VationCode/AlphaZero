using alpha.ingame.input;
using alpha.ingame.player;
using UnityEngine;

namespace alpha.ingame.item
{
    public enum EGunFireMode
    {
        Single = 0,
        Automatic = 1,
        Charge = 2
    }

    public enum EGunShotMode
    {
        Projectile = 0,
        Hitscan = 1
    }

    /// <summary>
    /// 입력 방식과 탄환 판정 방식을 독립적으로 선택한다. 카메라 중앙으로 조준한 뒤
    /// FirePos에서 투사체를 발사하거나 히트스캔을 검사한다.
    /// </summary>
    public class GunWeapon : EquippableWeapon
    {
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _muzzleEffectPoint;
        [SerializeField] private EGunFireMode _fireMode = EGunFireMode.Single;
        [SerializeField] private EGunShotMode _shotMode = EGunShotMode.Projectile;
        [SerializeField, Min(0.01f)] private float _fireInterval = 0.2f;
        [SerializeField] private GunProjectile _projectilePrefab;
        [SerializeField, Min(0)] private int _initialPoolSize = 12;
        [SerializeField] private ImpactData _impactData;
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField, Min(0.01f)] private float _aimDistance = 200f;
        [SerializeField, Min(0.01f)] private float _chargeDuration = 1.5f;
        [SerializeField, Min(0f)] private float _minimumChargeTime = 0.2f;
        [SerializeField, Min(1f)] private float _fullChargeDamageMultiplier = 2f;

        private GunProjectilePool _projectilePool;
        private ImpactPool _impactPool;
        private float _nextShotTime;
        private float _chargeStartedAt;
        private bool _isCharging;

        public Transform FirePoint => _firePoint;
        public Transform MuzzleEffectPoint => _muzzleEffectPoint != null ? _muzzleEffectPoint : _firePoint;

        public override void Initialize(PlayerCore owner)
        {
            base.Initialize(owner);
            // 무기를 만들 때 선택한 판정 방식에 필요한 풀을 준비한다.
            EnsurePools();
        }

        private void Update()
        {
            AlphaInput input = AlphaInput.Instance;
            if (!IsEquipped || Owner == null || input == null || input.IsQuickSlotOpen || input.IsQuickSlotPressed)
            {
                _isCharging = false;
                return;
            }

            switch (_fireMode)
            {
                case EGunFireMode.Single:
                    if (input.IsAttackPressed) TryFire(0f);
                    break;
                case EGunFireMode.Automatic:
                    if (input.IsAttackPressed || input.IsAttackHeld) TryFire(0f);
                    break;
                case EGunFireMode.Charge:
                    if (input.IsAttackPressed)
                    {
                        _isCharging = true;
                        _chargeStartedAt = Time.time;
                    }
                    if (_isCharging && input.IsAttackReleased)
                    {
                        _isCharging = false;
                        float heldTime = Time.time - _chargeStartedAt;
                        if (heldTime >= _minimumChargeTime)
                            TryFire(Mathf.Clamp01(heldTime / Mathf.Max(0.01f, _chargeDuration)));
                    }
                    break;
            }
        }

        private void OnDisable() => _isCharging = false;

        private void OnDestroy()
        {
            _projectilePool?.Dispose();
            _impactPool?.Dispose();
        }

        private void TryFire(float charge)
        {
            if (_firePoint == null || Time.time < _nextShotTime) return;
            _nextShotTime = Time.time + Mathf.Max(0.01f, _fireInterval);
            EnsurePools();

            Vector3 direction = GetAimDirection();
            float damage = Mathf.Lerp(_damage, _damage * _fullChargeDamageMultiplier, charge);
            if (_shotMode == EGunShotMode.Hitscan)
            {
                // 총구에서 다시 검사해 카메라와 총구 사이의 장애물을 반영한다.
                if (TryGetNearestHit(new Ray(_firePoint.position, direction), _aimDistance, out RaycastHit hit))
                    _impactPool?.Spawn(hit, Owner.transform, damage, -direction);
            }
            else
            {
                _projectilePool.Rent().Launch(_projectilePool, _impactPool,
                    Owner.transform, _firePoint.position, direction,
                    damage, Mathf.Max(0.01f, _aimDistance));
            }
        }

        private void EnsurePools()
        {
            if (_shotMode == EGunShotMode.Projectile)
                _projectilePool ??= new GunProjectilePool(_projectilePrefab, Mathf.Max(0, _initialPoolSize), name);
            if (_impactPool == null && _impactData != null)
                _impactPool = new ImpactPool(_impactData, name);
        }

        private Vector3 GetAimDirection()
        {
            Camera camera = Camera.main;
            if (camera == null) return _firePoint.forward;

            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            Vector3 target = TryGetNearestHit(ray, _aimDistance, out RaycastHit hit)
                ? hit.point : ray.GetPoint(_aimDistance);

            Vector3 direction = target - _firePoint.position;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : _firePoint.forward;
        }

        private bool TryGetNearestHit(Ray ray, float maxDistance, out RaycastHit nearestHit)
        {
            nearestHit = default;
            float nearestDistance = maxDistance;
            bool found = false;
            foreach (RaycastHit hit in Physics.RaycastAll(ray, maxDistance,
                         Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null || hit.collider.transform.IsChildOf(Owner.transform) ||
                    hit.distance >= nearestDistance) continue;
                nearestHit = hit;
                nearestDistance = hit.distance;
                found = true;
            }
            return found;
        }
    }
}
