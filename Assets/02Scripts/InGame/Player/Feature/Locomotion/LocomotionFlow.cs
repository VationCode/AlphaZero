using UnityEngine;

namespace alpha.player.locomotion
{
    // 다른 이동 모드 추가
    public enum ELocomotionMode
    {
        Ground,
        Flight,
        Swimming,
        Climbing,
    }

    public class LocomotionFlow : MonoBehaviour
    {
        public ELocomotionMode CurrentLocomotionMode { get; private set; } = ELocomotionMode.Ground;


    }
}
