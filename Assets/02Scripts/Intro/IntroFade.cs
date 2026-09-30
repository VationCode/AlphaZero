using alpha.scene;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace alpha.title.intro
{
    [Serializable]
    public struct FadeData
    {
        public Image IntroImg;
        public float FadeInTime;
        public float StayTime;     // 유지 시간
        public float FadeOutTime;
    }

    [Serializable]
    public struct IntroSequenceData
    {
        public FadeData[] FadeData;
        public int Count => FadeData?.Length ?? 0;
        public AudioClip AudioClip;
    }

    public class IntroFade : MonoBehaviour
    {
        [SerializeField]
        private IntroSequenceData[] _introSequences;
        [SerializeField]
        private AudioSource _audioSource;

        private void OnEnable()
        {
            foreach (var seq in _introSequences)
            {
                foreach (var seq2 in seq.FadeData)
                {
                    seq2.IntroImg.gameObject.SetActive(false);
                }
            }
        }

        private void Start()
        {
            StartCoroutine(PlayAll());
        }

        private IEnumerator PlayAll()
        {
            // 모든 오디오 시퀀스를 순서대로 재생
            for (int i = 0; i < _introSequences.Length; i++)
                yield return StartCoroutine(Fade(i));

            SceneLoader.LoadLobbyScene();
        }

        private IEnumerator Fade(int p_sequenceIndex)
        {
            var sequence = _introSequences[p_sequenceIndex];

            /*// 오디오는 이미지마다 재시작하지 않고 시퀀스당 한 번만 재생
            _audioSource.Stop();
            _audioSource.clip = sequence.AudioClip;
            _audioSource.Play();*/

            for (int i = 0; i < sequence.Count; i++)
            {
                var data = sequence.FadeData[i];

                yield return StartCoroutine(FadeIn(data));
                yield return new WaitForSeconds(data.StayTime);
                yield return StartCoroutine(FadeOut(data));
            }

            // 이미지 연출이 먼저 끝났다면 오디오 종료를 기다림
            /*while (_audioSource.isPlaying)
                yield return null;*/
        }

        private IEnumerator FadeIn(FadeData p_Fade)
        {
            Image image = p_Fade.IntroImg;
            Color color = image.color;
            color.a = 0f;
            image.color = color;
            image.gameObject.SetActive(true);

            if (p_Fade.FadeInTime <= 0f)
            {
                color.a = 1f;
                image.color = color;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < p_Fade.FadeInTime)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsed / p_Fade.FadeInTime);
                image.color = color;
                yield return null;
            }

            color.a = 1f;
            image.color = color;
        }

        private IEnumerator FadeOut(FadeData p_Fade)
        {
            Image image = p_Fade.IntroImg;
            Color color = image.color;
            float elapsed = 0f;

            while (elapsed < p_Fade.FadeOutTime)
            {
                elapsed += Time.deltaTime;
                color.a = 1f - Mathf.Clamp01(elapsed / p_Fade.FadeOutTime);
                image.color = color;
                yield return null;
            }

            color.a = 0f;
            image.color = color;
            image.gameObject.SetActive(false);
        }
        public void Skip()
        {

        }

        public void Next()
        {

        }
    }
}