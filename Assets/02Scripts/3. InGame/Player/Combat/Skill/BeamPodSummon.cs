using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>모델에 미리 배치된 빔포드들을 한 번에 소환한다.</summary>
    [RequireComponent(typeof(BeamPodFollow))]
    public sealed class BeamPodSummon : MonoBehaviour
    {
        private SummonLifetime[] _pods;
        private BeamPodAutoMove[] _autoMoves;
        private Vector3[] _spawnLocalPositions;
        private Quaternion[] _spawnLocalRotations;
        private BeamPodFollow _follow;

        private void Awake()
        {
            _pods = GetComponentsInChildren<SummonLifetime>(true);
            _autoMoves = new BeamPodAutoMove[_pods.Length];
            _spawnLocalPositions = new Vector3[_pods.Length];
            _spawnLocalRotations = new Quaternion[_pods.Length];

            // 배치된 포드의 시작 자세를 보관해 스킬을 다시 쓸 때 복원한다.
            for (int i = 0; i < _pods.Length; i++)
            {
                Transform podTransform = _pods[i].transform;
                _autoMoves[i] = _pods[i].GetComponent<BeamPodAutoMove>();
                _spawnLocalPositions[i] = podTransform.localPosition;
                _spawnLocalRotations[i] = podTransform.localRotation;
            }

            _follow = GetComponent<BeamPodFollow>();
        }

        public bool Activate()
        {
            if (_pods == null || _pods.Length == 0)
            {
                Debug.LogError($"{nameof(BeamPodSummon)}: 빔포드에 {nameof(SummonLifetime)}을 연결하세요.", this);
                return false;
            }

            for (int i = 0; i < _pods.Length; i++)
            {
                if (_pods[i] != null && _autoMoves[i] != null) continue;
                Debug.LogError($"{nameof(BeamPodSummon)}: 각 빔포드에 {nameof(BeamPodAutoMove)}를 연결하세요.", this);
                return false;
            }

            // 모든 포드를 먼저 끄고 시작 자세로 되돌린다.
            foreach (SummonLifetime pod in _pods)
            {
                pod.gameObject.SetActive(false);
            }

            if (_follow == null || !_follow.Begin()) return false;

            // OnEnable이 각 포드의 생존 시간을 새로 시작한다.
            for (int i = 0; i < _pods.Length; i++)
            {
                SummonLifetime pod = _pods[i];
                pod.transform.SetLocalPositionAndRotation(_spawnLocalPositions[i], _spawnLocalRotations[i]);
                pod.gameObject.SetActive(true);
                _autoMoves[i].Begin(_spawnLocalPositions[i]);
            }
            return true;
        }

        // 타겟 획득/해제 시 타겟팅 로직에서 호출한다.
        public void SetTargeting(bool hasTarget)
        {
            if (_autoMoves == null) return;
            foreach (BeamPodAutoMove autoMove in _autoMoves)
                if (autoMove != null && autoMove.gameObject.activeInHierarchy)
                    autoMove.SetTargeting(hasTarget);
        }
    }
}
