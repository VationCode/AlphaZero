using System;
using System.Collections.Generic;
using UnityEngine;

namespace alpha.ingame.player
{
    // 아레테마다 타겟 사용법은 달라도 화면 검색과 타겟 표시는 플레이어가 공유한다.
    public interface IPlayerTargetSession
    {
        void OnTargetsChanged(IReadOnlyList<Collider> targets);
    }

    // 카메라가 LateUpdate에서 회전한 뒤 현재 화면의 타겟을 선정한다.
    [DefaultExecutionOrder(140)]
    public sealed class PlayerTargetingModule : MonoBehaviour
    {
        [SerializeField] private TargetMarkerView _markerView;
        [SerializeField, Min(0.05f)] private float _validationInterval = 0.2f;

        private readonly Collider[] _candidates = new Collider[128];
        private readonly RaycastHit[] _sightHits = new RaycastHit[64];
        private readonly List<TargetCandidate> _visible = new();
        private readonly List<Collider> _targets = new();
        private PlayerCore _player;
        private IPlayerTargetSession _session;
        private LayerMask _targetLayers;
        private LayerMask _sightLayers;
        private float _maxDistance;
        private float _nextValidation;
        private int _maxTargets;
        private bool _isLiveTargeting;

        private struct TargetCandidate
        {
            public Collider Collider;
            public Transform Root;
            public float DistanceSquared;
        }

        private void Awake() => _player = GetComponentInParent<PlayerCore>(true);

        public bool Begin(IPlayerTargetSession session, float maxDistance, int maxTargets,
            LayerMask targetLayers, LayerMask sightLayers)
        {
            if (session == null || _player == null || _markerView == null || Camera.main == null)
                return false;

            if (_session != null && _session != session)
                _session.OnTargetsChanged(Array.Empty<Collider>());
            End(_session);
            _session = session;
            _maxDistance = Mathf.Max(0.1f, maxDistance);
            _maxTargets = Mathf.Max(1, maxTargets);
            _targetLayers = targetLayers;
            _sightLayers = sightLayers;
            _isLiveTargeting = true;
            Retarget(session);
            return true;
        }

        public void Retarget(IPlayerTargetSession session)
        {
            if (_session != session || _player == null) return;
            _isLiveTargeting = true;
            Camera camera = Camera.main;
            if (camera == null) return;

            RefreshVisibleTargets(camera, true);
        }

        // 공격 명령 시 타겟은 포드에 남기고 표시 이미지만 숨긴다.
        public void FreezeTargets(IPlayerTargetSession session)
        {
            if (_session != session) return;
            _isLiveTargeting = false;
            _nextValidation = Time.time + _validationInterval;
            _markerView?.Hide();
        }

        private void RefreshVisibleTargets(Camera camera, bool forcePublish)
        {
            _visible.Clear();
            int count = Physics.OverlapSphereNonAlloc(_player.transform.position, _maxDistance,
                _candidates, _targetLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider candidate = _candidates[i];
                if (candidate == null || !candidate.enabled || !candidate.gameObject.activeInHierarchy)
                    continue;

                Transform root = TargetRoot(candidate);
                Vector3 point = candidate.bounds.center;
                float distanceSquared = (point - _player.transform.position).sqrMagnitude;
                if (distanceSquared > _maxDistance * _maxDistance ||
                    !IsOnScreen(camera, point) || !HasLineOfSight(camera, candidate, root))
                    continue;

                int existing = -1;
                for (int j = 0; j < _visible.Count; j++)
                    if (_visible[j].Root == root)
                    {
                        existing = j;
                        break;
                    }
                if (existing >= 0 && _visible[existing].DistanceSquared <= distanceSquared) continue;
                TargetCandidate visible = new()
                {
                    Collider = candidate,
                    Root = root,
                    DistanceSquared = distanceSquared
                };
                if (existing >= 0) _visible[existing] = visible;
                else _visible.Add(visible);
            }

            _visible.Sort((a, b) =>
            {
                int byDistance = a.DistanceSquared.CompareTo(b.DistanceSquared);
                return byDistance != 0 ? byDistance :
                    a.Collider.GetInstanceID().CompareTo(b.Collider.GetInstanceID());
            });
            int targetCount = Mathf.Min(_maxTargets, _visible.Count);
            bool changed = _targets.Count != targetCount;
            if (!changed)
                for (int i = 0; i < targetCount; i++)
                    if (_targets[i] != _visible[i].Collider)
                    {
                        changed = true;
                        break;
                    }

            if (changed)
            {
                _targets.Clear();
                for (int i = 0; i < targetCount; i++)
                    _targets.Add(_visible[i].Collider);
            }

            // 타겟이 같으면 포드 재배정을 생략하고 표시 카메라만 갱신한다.
            _markerView?.Show(_targets, camera);
            if (changed || forcePublish) _session?.OnTargetsChanged(_targets);
        }

        public void ClearTargets(IPlayerTargetSession session)
        {
            if (_session != session) return;
            _isLiveTargeting = false;
            _targets.Clear();
            Publish(Camera.main);
        }

        public void End(IPlayerTargetSession session)
        {
            if (_session != session) return;
            _session = null;
            _isLiveTargeting = false;
            _targets.Clear();
            _markerView?.Hide();
        }

        private void LateUpdate()
        {
            if (_session == null) return;
            if (_isLiveTargeting)
            {
                Camera camera = Camera.main;
                if (camera != null) RefreshVisibleTargets(camera, false);
                return;
            }

            if (Time.time < _nextValidation) return;
            _nextValidation = Time.time + _validationInterval;
            bool changed = false;
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                Collider target = _targets[i];
                if (IsStillValid(target)) continue;
                _targets.RemoveAt(i);
                changed = true;
            }
            if (changed) Publish(Camera.main);
        }

        private void OnDisable() => End(_session);

        // 공격 중에는 카메라 시야와 플레이어 거리가 바뀌어도 타겟을 유지한다.
        private static bool IsStillValid(Collider target)
        {
            return target != null && target.enabled && target.gameObject.activeInHierarchy;
        }

        private void Publish(Camera camera)
        {
            // 공격 중 타겟이 사라져 배정을 갱신해도 이미지는 다시 켜지지 않는다.
            if (_isLiveTargeting) _markerView?.Show(_targets, camera);
            else _markerView?.Hide();
            _session?.OnTargetsChanged(_targets);
        }

        private Transform TargetRoot(Collider collider)
        {
            if (collider.attachedRigidbody != null)
                return collider.attachedRigidbody.transform;

            // Enemy 레이어의 부모까지만 묶어 플레이어 아래에 놓인 시험용 적도 구별한다.
            Transform root = collider.transform;
            while (root.parent != null && root.parent != _player.transform &&
                   (_targetLayers.value & (1 << root.parent.gameObject.layer)) != 0)
                root = root.parent;
            return root;
        }

        private static bool IsOnScreen(Camera camera, Vector3 point)
        {
            Vector3 screen = camera.WorldToScreenPoint(point);
            return screen.z > 0f && camera.pixelRect.Contains(new Vector2(screen.x, screen.y));
        }

        private bool HasLineOfSight(Camera camera, Collider target, Transform targetRoot)
        {
            Vector3 origin = camera.transform.position;
            Vector3 direction = target.bounds.center - origin;
            float distance = direction.magnitude;
            if (distance < 0.01f) return true;

            int count = Physics.RaycastNonAlloc(origin, direction / distance, _sightHits,
                distance, _sightLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Transform hit = _sightHits[i].transform;
                if (hit == null ||
                    (hit.IsChildOf(_player.transform) &&
                     (_targetLayers.value & (1 << hit.gameObject.layer)) == 0) ||
                    hit.IsChildOf(targetRoot)) continue;
                return false;
            }
            return true;
        }
    }
}
