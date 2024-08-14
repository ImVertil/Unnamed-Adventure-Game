using UnityEngine;
using Character.Effects;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using Events.AbilitySystem;

[CreateAssetMenu(fileName = "New Effect", menuName = "Effect/New Effect")]
public class Effect : ScriptableObject
{
    public float Duration = 0f;
    public float TickTime = 0f;
    public EffectType Type;
    public string ExecutionCalculatorName = "ExecutionCalculator";
    public List<EffectModifier> Modifiers;
    public Action<Effect> OnEffectEnd;
    private ExecutionCalculator _executionCalculator;

    public HashSet<AttributeType> GetAffectedAttributes()
    {
        HashSet<AttributeType> attributes = new();
        foreach (EffectModifier mod in Modifiers)
        {
            attributes.Add(mod.AttributeType);
        }

        return attributes;
    }

    private void SetDamageCalculator()
    {
        Type t = Assembly.GetExecutingAssembly().GetType(ExecutionCalculatorName);
        _executionCalculator = Activator.CreateInstance(t) as ExecutionCalculator;
        if (_executionCalculator == null)
        {
            Debug.LogError($"Using default ExecutionCalculator - Cannot find ExecutionCalculator with name {ExecutionCalculatorName}");
            t = Assembly.GetExecutingAssembly().GetType("ExecutionCalculator");
            _executionCalculator = Activator.CreateInstance(t) as ExecutionCalculator;
        }
    }
}