namespace alpha.ingame.player
{
    /// <summary>전투 상태가 공유하는 플레이어와 전투 모듈 참조를 제공한다.</summary>
    public abstract class CombatState
    {
        protected readonly CombatStateFlow Flow;
        protected readonly CombatModule Module;

        protected CombatState(PlayerCore playerCore)
        {
            Flow = playerCore.CombatStateFlow;
            Module = playerCore.CombatModule;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
    }
}
