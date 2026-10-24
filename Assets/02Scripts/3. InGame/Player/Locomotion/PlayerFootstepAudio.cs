using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>실제 지상 이동 거리에 맞춰 발소리를 재생한다.</summary>
    public sealed class PlayerFootstepAudio : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LocomotionModule _locomotion;
        [SerializeField] private LocomotionStateFlow _stateFlow;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _clips;

        [Header("Step Distance")]
        [SerializeField, Min(0f)] private float _minimumSpeed = 0.25f;
        [SerializeField, Min(0.01f)] private float _walkSpeed = 2f;
        [SerializeField, Min(0.01f)] private float _runSpeed = 6f;
        [SerializeField, Min(0.01f)] private float _walkStepDistance = 0.85f;
        [SerializeField, Min(0.01f)] private float _runStepDistance = 1.6f;
        [SerializeField, Min(0.01f)] private float _maxFrameDistance = 1f;

        [Header("Step Volume")]
        [SerializeField, Range(0f, 1f)] private float _walkVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _runVolume = 1f;

        private Vector3 _lastPosition;
        private float _distanceSinceStep;
        private int _lastClipIndex = -1;

        private void OnEnable()
        {
            if (_locomotion == null) _locomotion = GetComponent<LocomotionModule>();
            if (_stateFlow == null)
                _stateFlow = GetComponentInParent<PlayerCore>()?.GetComponentInChildren<LocomotionStateFlow>(true);
            if (_characterController == null) _characterController = GetComponentInParent<CharacterController>();
            if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

            // 기존 씬에 새 볼륨 필드가 없어서 둘 다 0으로 로드된 경우 기본값을 복원한다.
            if (_walkVolume <= 0f && _runVolume <= 0f)
            {
                _walkVolume = 0.5f;
                _runVolume = 1f;
            }

            _lastPosition = _characterController != null ? _characterController.transform.position : transform.position;
            _distanceSinceStep = 0f;
        }

        private void LateUpdate()
        {
            if (_locomotion == null || _stateFlow == null || _characterController == null ||
                _audioSource == null || _clips == null || _clips.Length == 0)
                return;

            Vector3 position = _characterController.transform.position;
            Vector3 movement = position - _lastPosition;
            _lastPosition = position;
            movement.y = 0f;
            float distance = movement.magnitude;

            // 점프, 착지, 기동 중이거나 순간 이동한 프레임에는 발소리를 내지 않는다.
            if (!_locomotion.IsGrounded || _locomotion.IsBoosting ||
                _stateFlow.CurrentState != ELocomotionState.Move ||
                distance > _maxFrameDistance || Time.deltaTime <= 0f)
            {
                _distanceSinceStep = 0f;
                return;
            }

            float speed = distance / Time.deltaTime;
            if (speed < _minimumSpeed)
            {
                _distanceSinceStep = 0f;
                return;
            }

            // 걷기와 달리기 사이에서 보폭을 보간해 속도별 발소리 간격을 정한다.
            float blend = Mathf.InverseLerp(_walkSpeed, _runSpeed, speed);
            float stepDistance = Mathf.Max(0.01f, Mathf.Lerp(_walkStepDistance, _runStepDistance, blend));
            _distanceSinceStep += distance;
            if (_distanceSinceStep < stepDistance) return;

            _distanceSinceStep -= stepDistance;
            // 보폭과 같은 속도 비율로 걷기는 작게, 달리기는 크게 재생한다.
            PlayStep(Mathf.Lerp(_walkVolume, _runVolume, blend));
        }

        private void PlayStep(float volume)
        {
            int index;
            if (_locomotion.MoveType == EMoveType.Walk && _clips.Length >= 2)
            {
                // Walk는 인스펙터의 1·2번 클립만 번갈아 사용한다.
                index = _lastClipIndex == 0 ? 1 : 0;
            }
            else
            {
                // 다른 이동은 전체 클립을 쓰되 직전 클립은 피한다.
                index = Random.Range(0, _clips.Length);
                if (_clips.Length > 1 && index == _lastClipIndex)
                    index = (index + Random.Range(1, _clips.Length)) % _clips.Length;
            }

            _lastClipIndex = index;
            if (_clips[index] != null) _audioSource.PlayOneShot(_clips[index], volume);
        }
    }
}
