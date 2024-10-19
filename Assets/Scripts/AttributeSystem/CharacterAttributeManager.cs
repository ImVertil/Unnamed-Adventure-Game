using Character.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CharacterAttributeManager : MonoBehaviour
{
    [SerializeField] private DefaultAttributes _defaultAttribtues;
    public CharacterAttributes Attributes { get; private set; }
    public List<Effect> ActiveEffects { get; private set; } = new();

    private void Awake()
    {
        if (_defaultAttribtues == null)
        {
            Debug.LogError($"({gameObject.name}) DefaultAttributes not set");
            return;
        }

        Attributes = new(_defaultAttribtues);
    }

    public void ApplyEffect(EffectData data)
    {
        if (data.Effect == null)
            return;

        if (data.Effect.Duration > 0f)
        {
            ActiveEffects.Add(data.Effect);
            foreach (EffectModifier modifier in data.Effect.Modifiers)
            {
                Attributes.GetAttribute(modifier.AttributeType).ActiveModifiers.Add(modifier);
            }

            if (data.Effect.TickTime == 0f)
            {
                RecalculateValue(data);
            }
        }

        StartCoroutine(EffectExecution(data));
    }

    public void RemoveEffect(EffectData data)
    {
        if (data.Effect == null)
            return;

        if (data.Effect.Duration > 0f)
        {
            ActiveEffects.Remove(data.Effect);
            foreach (EffectModifier modifier in data.Effect.Modifiers)
            {
                Attributes.GetAttribute(modifier.AttributeType).ActiveModifiers.Remove(modifier);
            }

            if (data.Effect.TickTime == 0f)
            {
                RecalculateValue(data);
            }
        }
    }

    private void RecalculateBaseValue(EffectData data)
    {
        foreach (EffectModifier modifier in data.Effect.Modifiers)
        {
            Attribute attribute = Attributes.GetAttribute(modifier.AttributeType);
            float newVal = 0f;
            switch (modifier.ValueOperator)
            {
                case ValueOperator.Add:
                    newVal = attribute.BaseValue + modifier.GetValue(data.Source, data.Target);
                    break;
                case ValueOperator.Multiply:
                    newVal = attribute.BaseValue * (1f + (modifier.GetValue(data.Source, data.Target) - 1f));
                    break;
                case ValueOperator.Divide:
                    newVal = attribute.BaseValue / (1f + (modifier.GetValue(data.Source, data.Target) - 1f));
                    break;
            }
            Attributes.OnPreAttributeChange(attribute, ref newVal);
            Debug.Log($"[{gameObject.name} BASE] {modifier.AttributeType} {attribute.BaseValue} -> {newVal}");
            attribute.BaseValue = newVal;
            Attributes.OnPostAttributeChange(attribute);
        }
    }

    private void RecalculateValue(EffectData data)
    {
        foreach (AttributeType attrType in data.Effect.AffectedAttributes)
        {
            Attribute attribute = Attributes.GetAttribute(attrType);
            if (attribute == null)
                return;

            float newVal;

            if (attribute.ActiveModifiers.Count == 0)
            {
                newVal = attribute.BaseValue;
            }
            else
            {
                float Add = SumMods(attribute.ActiveModifiers, data, ValueOperator.Add, 0f);
                float Multiply = SumMods(attribute.ActiveModifiers, data, ValueOperator.Multiply, 1f);
                float Divide = SumMods(attribute.ActiveModifiers, data, ValueOperator.Divide, 1f);
                newVal = (attribute.BaseValue + Add) * Multiply / Divide;
            }

            Attributes.OnPreAttributeChange(attribute, ref newVal);
            Debug.Log($"[{gameObject.name} CURRENT] {attrType} {attribute.CurrentValue} -> {newVal}");
            attribute.CurrentValue = newVal;
            Attributes.OnPostAttributeChange(attribute);
        }
    }

    private float SumMods(List<EffectModifier> modifiers, EffectData data, ValueOperator type, float bias)
    {
        float result = bias;

        foreach (EffectModifier modifier in modifiers)
        {
            if (modifier.ValueOperator == type)
            {
                result += modifier.GetValue(data.Source, data.Target) - bias;
            }
        }

        return result;
    }

    private IEnumerator EffectExecution(EffectData data)
    {
        if (data.Effect.Duration == 0f)
        {
            RecalculateBaseValue(data);
            yield break;
        }

        if (data.Effect.TickTime == 0f)
        {
            yield return new WaitForSeconds(data.Effect.Duration);
        }
        else
        {
            float timePassed = 0f;
            while (timePassed <= data.Effect.Duration)
            {
                timePassed += data.Effect.TickTime;
                RecalculateBaseValue(data);
                yield return new WaitForSeconds(data.Effect.TickTime);
            }
        }

        RemoveEffect(data);
    }
}