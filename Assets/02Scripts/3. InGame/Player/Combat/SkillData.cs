using UnityEngine;

namespace alpha.ingame.player
{
    public enum ESkillId
    {
        Shield = 0,
        Turret = 1,
        OrbitalArmament = 2,
        AreaAttack = 3
    }

    /// <summary>모든 스킬 에셋이 공유하는 변경되지 않는 설정값.</summary>
    public abstract class SkillData : ScriptableObject
    {
        [SerializeField] private ESkillId _id;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 4)] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField, Min(0f)] private float _cooldown;

        public ESkillId Id => _id;
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float Cooldown => Mathf.Max(0f, _cooldown);

        protected virtual void OnValidate()
        {
            _cooldown = Mathf.Max(0f, _cooldown);
        }
    }
}
