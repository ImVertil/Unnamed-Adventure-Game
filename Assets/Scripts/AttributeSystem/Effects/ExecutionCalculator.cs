using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Calculator", menuName = "Effect/New Calculator")]
public abstract class ExecutionCalculator : ScriptableObject
{
    public abstract float GetCalculatedValue(CharacterAttributes source, CharacterAttributes target, float value);
}