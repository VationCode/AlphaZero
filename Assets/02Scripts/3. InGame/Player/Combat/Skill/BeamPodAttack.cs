using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>FirePos에서 배정된 적을 확인하고 빔과 충돌 이펙트를 제어한다.</summary>
    [DefaultExecutionOrder(170)]
    [RequireComponent(typeof(SummonLifetime))]
    public sealed class BeamPodAttack : MonoBehaviour
    {
        [SerializeField] private Transform _firePos;
        [SerializeField] private BeamPodBeamVisual _beamVisual;
        [SerializeField] private Transform _hitEffect;
        [SerializeField] private AudioSource _attackAudioSource;

        private readonly RaycastHit[] _hits = new RaycastHit[64];
        private BeamPodTargetModule _targetModule;
        private BeamPodTravel _travel;
        private SummonLifetime _lifetime;
        private Transform _player;
        private Transform _group;
        private LayerMask _hitLayers;
        private LayerMask _targetLayers;
        private bool _isReady;
        private bool _isFiring;

        private void Awake()
        {
            _lifetime = GetComponent<SummonLifetime>();
            _travel = GetComponent<BeamPodTravel>();
            StopEffects();
        }

        public bool Begin(Transform player, LayerMask hitLayers, LayerMask targetLayers)
        {
            _targetModule = GetComponentInParent<BeamPodTargetModule>();
            if (player == null || _targetModule == null || _lifetime == null ||
                _travel == null || _firePos == null || _beamVisual == null ||
                !_beamVisual.IsConfigured || _hitEffect == null)
            {
                Debug.LogError($"{nameof(BeamPodAttack)}: FirePos, BeamVisual, HitEffect를 연결하세요.", this);
                return false;
            }

            _player = player;
            _group = transform.parent;
            _hitLayers = hitLayers;
            _targetLayers = targetLayers;
            _isReady = true;
            StopEffects();
            return true;
        }

        private void LateUpdate()
        {
            if (!_isReady || _lifetime.IsExpired || !_targetModule.IsAttacking ||
                !_travel.IsAtAttackPosition)
            {
                StopEffects();
                return;
            }

            Collider target = _targetModule.GetTarget(_lifetime);
            if (target == null || !target.enabled || !target.gameObject.activeInHierarchy)
            {
                StopEffects();
                return;
            }

            Vector3 origin = _firePos.position;
            Vector3 toTarget = target.bounds.center - origin;
            float distance = toTarget.magnitude;
            if (distance < 0.01f)
            {
                StopEffects();
                return;
            }

            // RaycastNonAlloc 결과는 거리순이 아니므로, 자기 그룹과 플레이어를 제외한 첫 충돌을 찾는다.
            int count = Physics.RaycastNonAlloc(origin, toTarget / distance, _hits,
                distance + 0.1f, _hitLayers, QueryTriggerInteraction.Ignore);
            int nearest = -1;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider hitCollider = _hits[i].collider;
                if (hitCollider == null || hitCollider.transform.IsChildOf(_group)) continue;
                if (hitCollider.transform.IsChildOf(_player) &&
                    (_targetLayers.value & (1 << hitCollider.gameObject.layer)) == 0) continue;
                if (_hits[i].distance >= nearestDistance) continue;
                nearest = i;
                nearestDistance = _hits[i].distance;
            }

            if (nearest < 0 || !IsAssignedTarget(_hits[nearest].collider, target))
            {
                StopEffects();
                return;
            }

            // 반복되는 빔·Hit 파티클은 생성하지 않고 켜 둔 채 충돌점만 갱신한다.
            _hitEffect.position = _hits[nearest].point;
            _beamVisual.Show(origin, _hits[nearest].point);
            if (!_hitEffect.gameObject.activeSelf) _hitEffect.gameObject.SetActive(true);
            if (!_isFiring)
            {
                // 명중으로 빔이 처음 켜진 순간에만 재생한다.
                _isFiring = true;
                if (_attackAudioSource != null) _attackAudioSource.Play();
            }
        }

        private bool IsAssignedTarget(Collider hit, Collider target)
        {
            if (hit == target) return true;
            Transform root = target.attachedRigidbody != null
                ? target.attachedRigidbody.transform : target.transform;
            while (root.parent != null && root.parent != _player &&
                   (_targetLayers.value & (1 << root.parent.gameObject.layer)) != 0)
                root = root.parent;
            return hit.transform.IsChildOf(root);
        }

        private void StopEffects()
        {
            if (_isFiring)
            {
                _isFiring = false;
                if (_attackAudioSource != null) _attackAudioSource.Stop();
            }
            if (_hitEffect != null && _hitEffect.gameObject.activeSelf)
                _hitEffect.gameObject.SetActive(false);
            if (_beamVisual != null) _beamVisual.Hide();
        }

        private void OnDisable()
        {
            _isReady = false;
            StopEffects();
        }
    }
}
