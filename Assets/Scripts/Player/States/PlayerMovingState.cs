using System.Collections;
using UnityEngine;

public class PlayerMovingState : PlayerState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered Moving state");
    }

    public override void UpdateState(PlayerController player)
    {
        if(player.GetPlayerSpeed() == 0 && PlayerInputHandler.Instance.Move == Vector2.zero)
        {
            player.ChangeState(player.IdleState);
            return;
        }

        if (PlayerInputHandler.Instance.Dash && !player.dashOnCooldown)
        {
            player.ChangeState(player.DashingState);
            return;
        }

        player.Move();
    }
}