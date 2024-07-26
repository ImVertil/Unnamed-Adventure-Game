using UnityEngine;

public sealed class PlayerOutOfCombatState : PlayerCombatState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered OutOfCombat state");
    }

    public override void UpdateState(PlayerController player)
    {
        if (player.IsInDashingState)
            return;

        if (PlayerInputHandler.Instance.Equip)
        {
            player.ChangeCombatState(player.InCombatState);
        }
    }
}