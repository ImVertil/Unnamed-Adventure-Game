using System.Collections;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered Idle state");
        player.SetAnimatorSpeedParam(0f);
    }

    public override void UpdateState(PlayerController player)
    {
        if(PlayerInputHandler.Instance.Move != Vector2.zero)
        {
            player.ChangeState(player.MovingState);
            return;
        }

        if(PlayerInputHandler.Instance.Dash && !player.dashOnCooldown)
        {
            player.ChangeState(player.DashingState);
            return;
        }
    }
}