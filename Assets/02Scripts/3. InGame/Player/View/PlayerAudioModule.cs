using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>플레이어의 Audios 아래에 모델별 재생 채널을 준비한다.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAudioModule : MonoBehaviour
    {
        private AudioSource _animationSource;
        private AudioSource _boosterSource;

        public void BindModel(GameObject model)
        {
            if (model == null) return;

            foreach (SkillAnimationEventRelay relay in model.GetComponentsInChildren<SkillAnimationEventRelay>(true))
            {
                if (_animationSource == null)
                {
                    // 기존 SFX2가 있으면 재사용하고, 없으면 Audios 아래에 만든다.
                    Transform existingChannel = transform.Find("SFX2");
                    _animationSource = existingChannel != null
                        ? existingChannel.GetComponent<AudioSource>() : null;
                    if (_animationSource == null) _animationSource = CreateSource("Animation");
                }
                relay.SetAudioSource(_animationSource);
            }

            foreach (SilyonBoosterAudio booster in model.GetComponentsInChildren<SilyonBoosterAudio>(true))
            {
                if (_boosterSource == null) _boosterSource = CreateSource("Booster");
                booster.SetAudioSource(_boosterSource);
            }
        }

        private AudioSource CreateSource(string channelName)
        {
            // 발소리 채널과 분리해 Stop() 호출이 다른 효과음을 끊지 않게 한다.
            GameObject channel = new GameObject(channelName);
            channel.transform.SetParent(transform, false);
            AudioSource source = channel.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }
    }
}
