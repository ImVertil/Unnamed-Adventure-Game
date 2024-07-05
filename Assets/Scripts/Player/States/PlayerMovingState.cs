using UnityEngine;

public sealed class PlayerMovingState : PlayerMovementState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered Moving state");
    }

    public override void UpdateState(PlayerController player)
    {
        //if(player.CurrentSpeed == 0 && PlayerInputHandler.Instance.Move == Vector2.zero)
        if(player.CurrentSqrSpeed == 0 && PlayerInputHandler.Instance.Move == Vector2.zero)
        {
            player.ChangeMovementState(player.IdleState);
            return;
        }

        if (PlayerInputHandler.Instance.Dash && !player.IsDashOnCooldown)
        {
            player.ChangeMovementState(player.DashingState);
            return;
        }

        if (player.IsInCombat)
        {
            player.CombatMove();
        }
        else
        {
            player.Move();
        }
    }
}