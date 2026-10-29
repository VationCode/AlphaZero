using System;
using System.Collections.Generic;
using alpha.system;
using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>공격 이벤트에 연결된 배치 이펙트를 전환하거나 프리팹을 생성한다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossAttackEffectView : MonoBehaviour
    {
        private enum EffectMode
        {
            Toggle,
            Spawn
        }

        private enum SpawnPositionMode
        {
            Targeting,
            Current,
            Specified,
            RandomTargetArea
        }

        [Serializable]
        private sealed class EffectBinding
        {
            [SerializeField] private string _attackId;
            [SerializeField] private string _eventKey;
            [SerializeField] private GameObject _effectObject;
            [SerializeField] private EffectMode _mode;
            [SerializeField] private SpawnPositionMode _spawnPositionMode;
            [SerializeField] private List<Transform> _spawnPoints = new();
            [SerializeField, Min(0.01f)] private float _spawnLifetime = 2f;
            [SerializeField] private bool _preserveInactiveChildren;
            [SerializeField] private bool _randomYaw;

            public string AttackId => _attackId;
            public string EventKey => _eventKey;
            public GameObject EffectObject => _effectObject;
            public bool IsSpawn => _mode == EffectMode.Spawn;
            public SpawnPositionMode PositionMode => _spawnPositionMode;
            public IReadOnlyList<Transform> SpawnPoints => _spawnPoints;
            public float SpawnLifetime => _spawnLifetime;
            public bool PreserveInactiveChildren => _preserveInactiveChildren;
            public bool RandomYaw => _randomYaw;
        }

        [SerializeField] private List<EffectBinding> _bindings = new();

        private readonly Dictionary<(string AttackId, string EventKey), EffectBinding> _effects = new();

        private void Awake()
        {
            foreach (EffectBinding binding in _bindings)
            {
                if (binding == null || string.IsNullOrWhiteSpace(binding.AttackId) ||
                    string.IsNullOrWhiteSpace(binding.EventKey) || binding.EffectObject == null)
                {
                    Debug.LogError($"{name}: 공격 이펙트의 ID, EventKey 또는 객체가 비어 있습니다.", this);
                    continue;
                }

                var key = (binding.AttackId.Trim(), binding.EventKey.Trim());
                if (_effects.ContainsKey(key))
                {
                    Debug.LogError($"{name}: 중복 이펙트 키 {key.Item1}/{key.Item2}", this);
                    continue;
                }

                _effects.Add(key, binding);
                if (!binding.IsSpawn)
                    DisableEffect(binding.EffectObject);
            }
        }

        public void SetEffect(string attackId, string eventKey, bool active)
        {
            if (string.IsNullOrEmpty(attackId) ||
                !_effects.TryGetValue((attackId, eventKey), out EffectBinding binding)) return;

            if (binding.IsSpawn || binding.EffectObject == null) return;

            GameObject effect = binding.EffectObject;
            if (!active)
            {
                DisableEffect(effect);
                return;
            }

            // 반복 공격에서 이전 트레일과 파티클 상태를 이어받지 않도록 초기화한다.
            DisableEffect(effect);
            effect.SetActive(true);
        }

        public void SpawnEffect(string attackId, string eventKey,
            bool hasTargetPosition, Vector3 targetPosition)
        {
            if (!_effects.TryGetValue((attackId, eventKey), out EffectBinding binding) ||
                !binding.IsSpawn || binding.EffectObject == null ||
                binding.PositionMode == SpawnPositionMode.RandomTargetArea) return;

            if (binding.PositionMode == SpawnPositionMode.Specified)
            {
                if (binding.SpawnPoints == null || binding.SpawnPoints.Count == 0)
                {
                    Debug.LogError($"{name}: {binding.AttackId}/{binding.EventKey}의 생성 위치가 비어 있습니다.", this);
                    return;
                }
                foreach (Transform point in binding.SpawnPoints)
                {
                    if (point == null)
                    {
                        Debug.LogError($"{name}: {binding.AttackId}/{binding.EventKey}의 생성 위치에 빈 항목이 있습니다.", this);
                        continue;
                    }
                    SpawnAt(binding, point.position, point.rotation);
                }
                return;
            }

            if (TryGetSpawnPosition(binding, hasTargetPosition, targetPosition, out Vector3 position))
                SpawnAt(binding, position, binding.EffectObject.transform.rotation);
        }

        public bool IsRandomTargetArea(string attackId, string eventKey) =>
            _effects.TryGetValue((attackId, eventKey), out EffectBinding binding) &&
            binding.IsSpawn && binding.PositionMode == SpawnPositionMode.RandomTargetArea;

        public bool TryGetSpawnSpacing(string attackId, string eventKey, out float spacing)
        {
            spacing = 0f;
            if (!_effects.TryGetValue((attackId, eventKey), out EffectBinding binding) ||
                !binding.IsSpawn || binding.PositionMode != SpawnPositionMode.RandomTargetArea ||
                binding.EffectObject == null) return false;

            GameObject prefab = binding.EffectObject;
            float footprint = 0f;
            foreach (Collider collider in prefab.GetComponentsInChildren<Collider>(true))
            {
                if (!collider.enabled) continue;
                Vector3 scale = collider.transform.lossyScale;
                float radius;
                Vector3 localCenter;
                switch (collider)
                {
                    case SphereCollider sphere:
                        radius = sphere.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                        localCenter = sphere.center;
                        break;
                    case CapsuleCollider capsule when capsule.direction == 1:
                        radius = capsule.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                        localCenter = capsule.center;
                        break;
                    default:
                        continue;
                }
                Vector3 offset = collider.transform.TransformPoint(localCenter) - prefab.transform.position;
                offset.y = 0f;
                footprint = Mathf.Max(footprint, offset.magnitude + radius);
            }
            if (footprint <= 0f)
            {
                Debug.LogError($"{name}: {attackId}/{eventKey} 프리팹에서 소환 간격을 계산할 콜라이더가 없습니다.", this);
                return false;
            }

            spacing = footprint * 2f + 0.1f; // Trigger가 생성 직후 서로 닿지 않도록 여유를 둔다.
            return true;
        }

        public void SpawnEffectAtPositions(string attackId, string eventKey,
            IReadOnlyList<Vector3> positions)
        {
            if (!_effects.TryGetValue((attackId, eventKey), out EffectBinding binding) ||
                !binding.IsSpawn || binding.PositionMode != SpawnPositionMode.RandomTargetArea ||
                binding.EffectObject == null || positions == null) return;

            Quaternion rotation = binding.EffectObject.transform.rotation;
            foreach (Vector3 position in positions)
                SpawnAt(binding, position, rotation);
        }

        private void SpawnAt(EffectBinding binding, Vector3 position, Quaternion rotation)
        {
            if (binding.RandomYaw)
                rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up) * rotation;
            // 자식 Rigidbody가 생성될 때부터 같은 방향을 갖도록 회전값을 먼저 정한다.
            GameObject instance = Instantiate(binding.EffectObject, position, rotation);
            ParticleSystem[] particles = instance.GetComponentsInChildren<ParticleSystem>(true);
            if (!binding.PreserveInactiveChildren)
                foreach (ParticleSystem particle in particles)
                    for (Transform node = particle.transform;
                         node != null && node != instance.transform; node = node.parent)
                        node.gameObject.SetActive(true);
            instance.SetActive(true);
            foreach (ParticleSystem particle in particles)
            {
                if (!particle.gameObject.activeInHierarchy) continue;
                particle.Clear();
                particle.Play();
            }

            // 이동 객체는 자체 Lifetime으로 제거하고, 일반 이펙트만 바인딩 수명을 사용한다.
            if (instance.GetComponent<ObjectMove>() == null)
                Destroy(instance, Mathf.Max(0.01f, binding.SpawnLifetime));
        }

        private bool TryGetSpawnPosition(EffectBinding binding, bool hasTargetPosition,
            Vector3 targetPosition, out Vector3 position)
        {
            position = default;
            switch (binding.PositionMode)
            {
                case SpawnPositionMode.Targeting:
                    if (!hasTargetPosition) return false;
                    position = targetPosition;
                    return true;
                case SpawnPositionMode.Current:
                    position = transform.position;
                    return true;
                default:
                    return false;
            }
        }

        public void StopAll()
        {
            foreach (EffectBinding binding in _effects.Values)
                if (!binding.IsSpawn && binding.EffectObject != null)
                    DisableEffect(binding.EffectObject);
        }

        private static void DisableEffect(GameObject effect)
        {
            effect.SetActive(false);
            foreach (TrailRenderer trail in effect.GetComponentsInChildren<TrailRenderer>(true))
                trail.Clear();
        }

        private void OnDisable() => StopAll();
    }
}
