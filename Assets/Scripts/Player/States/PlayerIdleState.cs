﻿using UnityEngine;

public sealed class PlayerIdleState : PlayerMovementState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered Idle state");
        player.SetAnimatorSpeedParam(0f);
        player.SetAnimatorPosParam(0f, 0f);
        player.Move(Vector2.zero, 0f);
    }

    public override void UpdateState(PlayerController player)
    {
        if (PlayerInputHandler.Instance.Move != Vector2.zero)
        {
            player.ChangeMovementState(player.MovingState);
            return;
        }

        if (PlayerInputHandler.Instance.Dash && !player.IsDashOnCooldown)
        {
            player.ChangeMovementState(player.DashingState);
            return;
        }

        if (player.IsInCombat)
        {
            Vector3 cursorDirection = player.GetDirectionTowardsMouseCursor();
            Vector2 lookDirection = new Vector2(cursorDirection.x, cursorDirection.z);
            player.Rotate(lookDirection);
        }

        //player.Idle();
    }
}