using UnityEngine;

public abstract class PlayerMovementState : PlayerState
{
    protected Vector2 _inputVector => PlayerInputHandler.Instance.Move;
}