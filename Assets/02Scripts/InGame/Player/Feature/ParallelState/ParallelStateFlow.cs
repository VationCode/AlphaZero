using alpha.player.action;
using alpha.player.combat;
using alpha.player.locomotion;
using UnityEngine;

public class ParallelStateFlow : MonoBehaviour
{
    private LocomotionFlow _locomotionFlow;
    private CombatFlow _combatFlow;
    private ActionFlow _actionFlow;

    public bool CanAttack()
    {
        return true;
    }

    public bool CanDodge()
    {
        return true;
    }

    public bool CanInteract()
    {
        return true;
    }
}
