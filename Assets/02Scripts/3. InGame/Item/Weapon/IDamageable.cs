using UnityEngine;

namespace alpha.ingame.item
{
    // 투사체 충돌을 받는 대상이 구현할 피해 처리 계약.
    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject source);
    }
}
