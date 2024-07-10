using UnityEngine;

public sealed class PlayerMovingState : PlayerMovementState
{
    private Vector2 _inputVector => PlayerInputHandler.Instance.Move;
    private Vector2 _currentCombatParamVector;
    private Vector2 _currentCombatParamVelocity;

    public override void EnterState(PlayerController player)
    {
        Debug.Log("Entered Moving state");
        _currentCombatParamVector = Vector2.zero;
    }

    public override void UpdateState(PlayerController player)
    {
        //if (player.CurrentSpeed == 0 && PlayerInputHandler.Instance.Move == Vector2.zero)
        if (player.CurrentSqrSpeed == 0 && PlayerInputHandler.Instance.Move == Vector2.zero)
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
            Vector3 cursorDirection = player.GetDirectionTowardsMouseCursor();
            Vector2 lookDirection = new Vector2(cursorDirection.x, cursorDirection.z);
            Debug.DrawLine(player.transform.position, player.transform.position + cursorDirection, Color.red);

            _currentCombatParamVector = Vector2.SmoothDamp(_currentCombatParamVector, _inputVector, ref _currentCombatParamVelocity, player.SmoothTime);
            Vector3 combatMovementVector = new Vector3(_currentCombatParamVector.x, 0f, _currentCombatParamVector.y);
            Vector3 cross = Vector3.Cross(Vector3.up, cursorDirection);
            float dotX = Vector3.Dot(cross, combatMovementVector);
            float dotY = Vector3.Dot(cursorDirection, combatMovementVector);

            player.SetAnimatorPosParam(dotX, dotY);
            player.Move(_inputVector, player.CombatSpeed);
            player.Rotate(lookDirection);
        }
        else
        {
            player.Move(_inputVector, PlayerInputHandler.Instance.Sprint ? player.SprintSpeed : player.Speed);
            if (_inputVector != Vector2.zero)
            {
                player.Rotate(_inputVector);
            }
        }

    }
}