﻿using UnityEngine;

public sealed class PlayerInCombatState : PlayerCombatState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered InCombat state");
    }

    public override void UpdateState(PlayerController player)
    {
        if (player.IsInDashingState)
            return;

        if (!PlayerInputHandler.Instance.Equip)
        {
            player.ChangeCombatState(player.OutOfCombatState);
            return;
        }

        if (PlayerInputHandler.Instance.Attack && player.CanAttack)
        {
            player.Attack();
        }
    }
}