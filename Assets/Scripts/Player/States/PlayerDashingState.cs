using UnityEngine;

public class PlayerDashingState : PlayerMovementState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered Dashing state");
        player.SetAnimationTrigger(player.AnimatorDashParamId);
    }

    public override void UpdateState(PlayerController player)
    {
        if (!player.IsDashAnimPlaying)
        {
            if (PlayerInputHandler.Instance.Move == Vector2.zero)
            {
                player.ChangeMovementState(player.IdleState);
            }
            else
            {
                player.ChangeMovementState(player.MovingState);
            }

            return;
        }

        player.DashMove();
    }
}