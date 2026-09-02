// Module : 기능 연산 위주
using System;
using UnityEngine;

namespace alpha.player.locomotion
{
    [Serializable]
    public struct MovementSettings
    {
        public ELocomotionMode LocomotionMode;

        [Header("Move Speed")]
        public float NormalMoveSpeed;
        public float SprintMoveSpeed;
        public float CombatMoveSpeed;
        //public float MoveAcceleration;
        //public float MoveDeceleration;

        [Header("Rotation")]
        public float RotationSmoothTime;
    }

    // 플레이어의 이동과 관련된 기능들을 담당하는 모듈
    public class LocomotionModule : MonoBehaviour
    {
        [SerializeField] // 0: Ground, 1: Flight, 2: Swimming, 3: Climbing
        private MovementSettings[] m_movementSettings;


        private void Start()
        {
            
        }

     
    }
}