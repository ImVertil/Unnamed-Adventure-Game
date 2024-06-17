using System.Collections;
using UnityEngine;

public class PlayerDashingState : PlayerState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered Dashing state");
        player.Dash();
    }

    public override void UpdateState(PlayerController player)
    {
        if (!player.GetDashingStatus())
        {
            if (PlayerInputHandler.Instance.Move == Vector2.zero)
            {
                player.ChangeState(player.IdleState);
            }
            else
            {
                player.ChangeState(player.MovingState);
            }

            return;
        }

        player.Dash();
    }
}