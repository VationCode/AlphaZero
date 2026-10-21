using alpha.ingame.player;
using UnityEngine;

namespace alpha.ingame.item
{
    public enum EGunFireMode
    {
        Single = 0,
        Automatic = 1,
        Charge = 2,
        Beam = 3
    }

    public enum EGunShotMode
    {
        Projectile = 0,
        Hitscan = 1
    }

    /// <summary>
    /// 입력 방식과 탄환 판정 방식을 선택한다. 단발·연사·차징은 투사체/히트스캔을 사용하고,
    /// 빔은 공격을 누르는 동안 FirePos에서 히트스캔 피해를 반복 적용한다.
    /// </summary>
    public class GunWeapon : EquippableWeapon
    {
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Transform _muzzleEffectPoint;
        [SerializeField] private GameObject _muzzleEffectPrefab;
        [SerializeField, Min(0)] private int _initialMuzzleEffectPoolSize = 4;
        [SerializeField, Min(0.01f)] private float _muzzleEffectMaxLifetime = 1f;
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
        [Header("Weapon Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _fireAudioClip;
        [SerializeField] private AudioClip _beamStartAudioClip;
        [SerializeField] private AudioClip _beamLoopAudioClip;
        [SerializeField] private AudioClip _beamEndAudioClip;

        private GunProjectilePool _projectilePool;
        private ImpactPool _impactPool;
        private MuzzleEffectPool _muzzleEffectPool;
        private float _nextShotTime;
        private float _chargeStartedAt;
        private bool _isCharging;
        private bool _beamActive;
        private bool _beamLoopStarted;
        private float _beamLoopAt;

        public Transform FirePoint => _firePoint;
        public Transform MuzzleEffectPoint => _muzzleEffectPoint != null ? _muzzleEffectPoint : _firePoint;

        public override void Initialize(PlayerCore owner)
        {
            base.Initialize(owner);
            // 무기를 만들 때 선택한 판정 방식에 필요한 풀을 준비한다.
            EnsurePools();
        }

        // 전투 상태가 공격을 시작할 때 호출한다. 발사 방식은 무기가 결정한다.
        public void BeginAttack()
        {
            if (!IsEquipped || Owner == null) return;

            switch (_fireMode)
            {
                case EGunFireMode.Single:
                case EGunFireMode.Automatic:
                    TryFire(0f);
                    break;
                case EGunFireMode.Charge:
                    _isCharging = true;
                    _chargeStartedAt = Time.time;
                    break;
                case EGunFireMode.Beam:
                    BeginBeam();
                    break;
            }
        }

        // 연사는 탄환을 발사하고 빔은 일정 간격으로 적중 판정을 갱신한다.
        public void UpdateAttack()
        {
            if (!IsEquipped || Owner == null) return;
            if (_fireMode == EGunFireMode.Automatic) TryFire(0f);
            if (_fireMode != EGunFireMode.Beam) return;

            if (!_beamActive) BeginBeam();
            else
            {
                if (!_beamLoopStarted && Time.time >= _beamLoopAt) StartBeamLoop();
                if (Time.time >= _nextShotTime) TickBeam();
            }
        }

        public void EndAttack()
        {
            if (_isCharging && IsEquipped && Owner != null)
            {
                float heldTime = Time.time - _chargeStartedAt;
                if (heldTime >= _minimumChargeTime)
                    TryFire(Mathf.Clamp01(heldTime / Mathf.Max(0.01f, _chargeDuration)));
            }
            _isCharging = false;
            StopBeam(true);
        }

        public void CancelAttack()
        {
            _isCharging = false;
            StopBeam(false);
        }

        private void OnDisable() => CancelAttack();

        private void OnDestroy()
        {
            _projectilePool?.Dispose();
            _impactPool?.Dispose();
            _muzzleEffectPool?.Dispose();
        }

        private bool TryFire(float charge)
        {
            if (_firePoint == null || Time.time < _nextShotTime) return false;
            if (_shotMode == EGunShotMode.Projectile && _projectilePrefab == null) return false;
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

            // 입력 시점이 아닌 실제 발사 시점에 무기별 총구 효과를 재생한다.
            _muzzleEffectPool?.Spawn(MuzzleEffectPoint);
            if (_audioSource != null && _fireAudioClip != null)
                _audioSource.PlayOneShot(_fireAudioClip);
            // 적중 여부와 관계없이 실제 발사가 일어나면 전투 자세 시간을 갱신한다.
            Owner.CombatModule?.RegisterSuccessfulAttack(this);
            return true;
        }

        private void BeginBeam()
        {
            if (_beamActive || _firePoint == null || Time.time < _nextShotTime) return;
            _beamActive = true;
            _beamLoopStarted = false;
            _beamLoopAt = Time.time;

            if (_audioSource != null && _beamStartAudioClip != null)
            {
                _audioSource.Stop();
                _audioSource.loop = false;
                _audioSource.clip = _beamStartAudioClip;
                _audioSource.Play();
                _beamLoopAt += _beamStartAudioClip.length;
            }
            else StartBeamLoop();

            TickBeam();
        }

        private void StartBeamLoop()
        {
            _beamLoopStarted = true;
            if (_audioSource == null || _beamLoopAudioClip == null) return;
            _audioSource.Stop();
            _audioSource.clip = _beamLoopAudioClip;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        private void TickBeam()
        {
            _nextShotTime = Time.time + Mathf.Max(0.01f, _fireInterval);
            Vector3 direction = GetAimDirection();
            if (TryGetNearestHit(new Ray(_firePoint.position, direction), _aimDistance, out RaycastHit hit))
            {
                // 빔 틱은 직접 피해를 준다. ImpactData의 지속 피해와 중첩하지 않는다.
                foreach (MonoBehaviour behaviour in hit.collider.GetComponentsInParent<MonoBehaviour>())
                {
                    if (behaviour is not IDamageable damageable) continue;
                    damageable.TakeDamage(_damage, Owner.gameObject);
                    break;
                }
            }
            Owner.CombatModule?.RegisterSuccessfulAttack(this);
        }

        private void StopBeam(bool playEndSound)
        {
            if (!_beamActive) return;
            _beamActive = false;
            _beamLoopStarted = false;
            if (_audioSource == null) return;
            _audioSource.Stop();
            _audioSource.clip = null;
            _audioSource.loop = false;
            if (playEndSound && _beamEndAudioClip != null && _audioSource.isActiveAndEnabled)
                _audioSource.PlayOneShot(_beamEndAudioClip);
        }

        private void EnsurePools()
        {
            if (_fireMode != EGunFireMode.Beam && _shotMode == EGunShotMode.Projectile && _projectilePrefab != null)
                _projectilePool ??= new GunProjectilePool(_projectilePrefab, Mathf.Max(0, _initialPoolSize), name);
            if (_impactPool == null && _impactData != null)
                _impactPool = new ImpactPool(_impactData, name);
            if (_muzzleEffectPool == null && _muzzleEffectPrefab != null)
                _muzzleEffectPool = new MuzzleEffectPool(_muzzleEffectPrefab,
                    Mathf.Max(0, _initialMuzzleEffectPoolSize), _muzzleEffectMaxLifetime, name);
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
