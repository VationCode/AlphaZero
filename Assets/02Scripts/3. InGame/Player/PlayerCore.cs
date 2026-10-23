using System;
using alpha.ingame.item;
using alpha.ingame.skill;
using UnityEngine;

namespace alpha.ingame.player
{
    public class PlayerCore : MonoBehaviour
    {
        [Header("Model")]
        [SerializeField] private Transform _modelRoot;
        [SerializeField] private GameObject _hernaPrefab;
        [SerializeField] private GameObject _arinPrefab;
        [SerializeField] private GameObject _silyonPrefab;

        public GameObject CurrentModel { get; private set; }
        public Arete CurrentArete { get; private set; }
        public EAreteType CurrentAreteType { get; private set; }
        public AnimationView AnimationView { get; private set; }
        
        public LocomotionStateFlow LocomotionStateFlow { get; private set; }
        public CombatStateFlow CombatStateFlow { get; private set; }
        public CombatActionFlow CombatActionFlow => CombatStateFlow?.Action;
        public CombatStanceFlow CombatStanceFlow => CombatStateFlow?.Stance;
        public SkillStateFlow SkillStateFlow { get; private set; }
        public SkillModule SkillModule { get; private set; }
        public SkillDisplayChannel SkillDisplayChannel { get; } = new SkillDisplayChannel();
        public AmmoDisplayChannel AmmoDisplayChannel { get; } = new AmmoDisplayChannel();


        public LocomotionModule LocomotionModule { get; private set; }
        public CombatModule CombatModule { get; private set; }
        public EquipmentModule EquipmentModule { get; private set; }
        public InventoryModule InventoryModule { get; private set; }


        public event Action<EAreteType> OnPlayerSetup;

        private void Awake()
        {
            if (_modelRoot == null) _modelRoot = transform.Find("Model");

            LocomotionStateFlow = GetComponentInChildren<LocomotionStateFlow>(true);
            LocomotionModule = GetComponentInChildren<LocomotionModule>(true);
            CombatStateFlow = GetComponentInChildren<CombatStateFlow>(true);
            CombatModule = GetComponentInChildren<CombatModule>(true);
            SkillStateFlow = GetComponentInChildren<SkillStateFlow>(true);
            SkillModule = GetComponentInChildren<SkillModule>(true);

            EquipmentModule = GetComponentInChildren<EquipmentModule>(true);
            InventoryModule = GetComponentInChildren<InventoryModule>(true);

            AnimationView = GetComponentInChildren<AnimationView>(true);
        }


        private void Start()
        {
            if (GameManager.Instance == null) return;

            EAreteType areteType = GameManager.Instance.AreteType;
            CurrentAreteType = areteType;
            if (!CreateModel(areteType)) return;

            if (AnimationView != null)
            {
                // 모델 생성 후 Animator와 아레테 컨트롤러를 함께 설정한다.
                AnimationView.Initialize(CurrentModel.GetComponentInChildren<Animator>(true), areteType);
            }

            InventoryModule?.InitializeStartingWeapons(areteType);
            EquipmentModule?.Initialize(areteType, CurrentModel, InventoryModule);
            OnPlayerSetup?.Invoke(areteType);

            if (LocomotionModule != null && AnimationView != null)
            {
                LocomotionModule.OnMove += AnimationView.MoveAnim;
                LocomotionModule.OnJump += AnimationView.JumpAnim;
            }
        }

        private bool CreateModel(EAreteType p_areteType)
        {
            // 선택한 아레테 모델 하나만 Player/Model 아래에 배치한다.
            GameObject prefab = p_areteType switch
            {
                EAreteType.Heruna => _hernaPrefab,
                EAreteType.Arin => _arinPrefab,
                EAreteType.Silyon => _silyonPrefab,
                _ => null
            };

            if (_modelRoot == null || prefab == null)
            {
                Debug.LogWarning($"{nameof(PlayerCore)}: Model 또는 {p_areteType} 프리팹 참조가 없습니다.", this);
                return false;
            }

            // 씬의 편집용 모델이 있으면 런타임 모델과 겹치지 않게 제거한다.
            for (int i = _modelRoot.childCount - 1; i >= 0; i--)
            {
                GameObject existingModel = _modelRoot.GetChild(i).gameObject;
                existingModel.SetActive(false);
                Destroy(existingModel);
            }

            CurrentModel = Instantiate(prefab, _modelRoot);
            CurrentModel.transform.localPosition = Vector3.zero;
            CurrentModel.transform.localRotation = Quaternion.identity;
            CurrentModel.transform.localScale = Vector3.one;

            CurrentArete = CurrentModel.GetComponent<Arete>();
            if (CurrentArete == null || CurrentArete.AreteType != p_areteType)
            {
                Debug.LogError($"{nameof(PlayerCore)}: {p_areteType} 프리팹 루트에 올바른 Arete 컴포넌트가 없습니다.", this);
                Destroy(CurrentModel);
                CurrentModel = null;
                CurrentArete = null;
                return false;
            }

            CurrentArete.Initialize(this);
            return true;
        }

    }
}
