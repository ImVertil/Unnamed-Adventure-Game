using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Calculator", menuName = "Effect/New Calculator")]
public class ExecutionCalculator : ScriptableObject
{
    //protected virtual float GetCalculatedValue(CharacterAttributes target, float value)
    protected virtual float GetCalculatedValue()
    {
        return 1f;
    } 
}