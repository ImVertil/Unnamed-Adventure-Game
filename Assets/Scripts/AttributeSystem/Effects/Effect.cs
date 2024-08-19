using Character.Effects;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Effect/New Effect")]
public sealed class Effect : ScriptableObject
{
    public float Duration = 0f;
    [Min(0f)]
    public float TickTime = 0f;
    public EffectType Type;
    public List<EffectModifier> Modifiers;
    [SerializeField] private ExecutionCalculator _executionCalculator;

    private HashSet<AttributeType> _affectedAttributes;
    public HashSet<AttributeType> AffectedAttributes
    {
        get
        {
            if (_affectedAttributes != null)
                return _affectedAttributes;

            _affectedAttributes = new();
            foreach (EffectModifier mod in Modifiers)
            {
                _affectedAttributes.Add(mod.AttributeType);
            }

            return _affectedAttributes;
        }
    }

    /*private void SetDamageCalculator()
    {
        Type t = Assembly.GetExecutingAssembly().GetType(ExecutionCalculatorName);
        _executionCalculator = Activator.CreateInstance(t) as ExecutionCalculator;
        if (_executionCalculator == null)
        {
            Debug.LogError($"Using default ExecutionCalculator - Cannot find ExecutionCalculator with name {ExecutionCalculatorName}");
            t = Assembly.GetExecutingAssembly().GetType("ExecutionCalculator");
            _executionCalculator = Activator.CreateInstance(t) as ExecutionCalculator;
        }
    }*/
}