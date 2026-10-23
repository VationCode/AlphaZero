using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>모델에 미리 배치된 빔포드들을 한 번에 소환한다.</summary>
    public sealed class BeamPodSummon : MonoBehaviour
    {
        private SummonLifetime[] _pods;

        private void Awake() => _pods = GetComponentsInChildren<SummonLifetime>(true);

        public bool Activate()
        {
            if (_pods == null || _pods.Length == 0)
            {
                Debug.LogError($"{nameof(BeamPodSummon)}: 빔포드에 {nameof(SummonLifetime)}을 연결하세요.", this);
                return false;
            }

            // 각 포드의 OnEnable이 생존 시간을 새로 시작한다.
            foreach (SummonLifetime pod in _pods)
            {
                if (pod == null) continue;
                pod.gameObject.SetActive(false);
                pod.gameObject.SetActive(true);
            }
            return true;
        }
    }
}
