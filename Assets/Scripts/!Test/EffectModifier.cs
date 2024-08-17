using Character.Effects;
using UnityEngine;

[System.Serializable]
public class EffectModifier
{
    [SerializeField] private ExecutionCalculator _executionCalculator;
    public AttributeType AttributeType;
    public ValueOperator ValueOperator;
    public float Value;

    public float GetValue(CharacterAttributes source, CharacterAttributes target)
    {
        if (_executionCalculator == null)
            return Value;

        return _executionCalculator.GetCalculatedValue(source, target, Value);
    }
}
