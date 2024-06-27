using UnityEngine;

public abstract class PlayerState
{
    public abstract void EnterState(PlayerController player);
    public abstract void UpdateState(PlayerController player);
    //public abstract void ExitState(PlayerController player);
}