using System;
using UnityEngine;

namespace alpha.ingame.player
{
    public class PlayerCore : MonoBehaviour
    {
        public AnimationView AnimationView { get; private set; }
        
        public LocomotionStateFlow LocomotionStateFlow { get; private set; }


        public LocomotionModule LocomotionModule { get; private set; }
        public EquipmentManager EquipmentManager {  get; private set; }


        public event Action<EAreteType> OnPlayerSetup;

        private void Awake()
        {
            LocomotionStateFlow = GetComponentInChildren<LocomotionStateFlow>(true);
            LocomotionModule = GetComponentInChildren<LocomotionModule>(true);

            EquipmentManager = GetComponentInChildren<EquipmentManager>(true);

            AnimationView = GetComponentInChildren<AnimationView>(true);
        }


        private void Start()
        {
            if (GameManager.Instance == null) return;
            OnPlayerSetup += EquipmentManager.HandlePlayerSetup;
            OnPlayerSetup += AnimationView.HandlePlayerSetup;

            OnPlayerSetup?.Invoke(GameManager.Instance.AreteType);

            LocomotionModule.OnMove += AnimationView.MoveAnim;

        }

    }
}