using UnityEngine;

public abstract class ExecutionCalculator : ScriptableObject
{
    public abstract float GetCalculatedValue(CharacterAttributes source, CharacterAttributes target, float value);
}