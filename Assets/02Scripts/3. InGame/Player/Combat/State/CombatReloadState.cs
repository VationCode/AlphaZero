using alpha.ingame.item;

namespace alpha.ingame.player
{
    /// <summary>전투 자세와 별도로 재장전 동작의 시작·종료·취소를 관리한다.</summary>
    public sealed class CombatReloadState
    {
        private readonly CombatModule _module;
        private GunWeapon _gun;

        public bool IsActive => _gun != null;

        public CombatReloadState(PlayerCore playerCore)
        {
            _module = playerCore.CombatModule;
        }

        public bool TryEnter()
        {
            GunWeapon gun = _module.CurrentGun;
            if (IsActive || gun == null || !gun.TryReload()) return false;
            _gun = gun;
            return true;
        }

        public bool UpdateState()
        {
            if (_gun == null || _gun.IsReloading) return false;

            bool completed = _gun == _module.CurrentGun;
            _gun = null;
            return completed;
        }

        public void Cancel()
        {
            // 무기 교체·스킬·공중 상태에서는 진행 중인 장전을 취소한다.
            _gun?.CancelReload();
            _gun = null;
        }
    }
}
