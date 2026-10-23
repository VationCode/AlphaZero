using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>설치된 쉴드의 발동 시간만 관리한다. 적 원거리 공격 차단은 공격 구현과 연결한다.</summary>
    [RequireComponent(typeof(SphereCollider))]
    public sealed class ShieldInstance : MonoBehaviour
    {
        private float _expiresAt;
        private bool _activated;

        public void Activate(float duration)
        {
            duration = Mathf.Max(0.1f, duration);
            _expiresAt = Time.time + duration;

            // 프리팹의 짧은 구체 버스트와 8초 지속 파티클을 설치 시간에 맞춘다.
            foreach (ParticleSystem particle in GetComponentsInChildren<ParticleSystem>(true))
            {
                particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                ParticleSystem.MainModule main = particle.main;
                if (main.duration <= 0.1f)
                    main.startLifetime = duration;
                else
                    main.duration = duration;
                particle.Play(false);
            }

            _activated = true;
        }

        private void Update()
        {
            if (_activated && Time.time >= _expiresAt)
                Destroy(gameObject);
        }
    }
}
