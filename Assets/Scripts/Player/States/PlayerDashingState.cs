using UnityEngine;

public sealed class PlayerDashingState : PlayerMovementState
{
    private Vector2 _inputVector => PlayerInputHandler.Instance.Move;
    private Vector2 _dashDirection;

    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered Dashing state");
        player.SetDashTrigger();
        _dashDirection = _inputVector == Vector2.zero ? new Vector2(player.transform.forward.x, player.transform.forward.z) : _inputVector;
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

        //player.DashMove();
        player.Move(_dashDirection, player.DashSpeed);
        player.Rotate(_dashDirection);
    }
}