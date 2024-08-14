using Character.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttributeManager : MonoBehaviour
{
    [SerializeField] private DefaultAttributes _defaultAttribtues;
    public CharacterAttributes Attributes { get; private set; }
    public List<Effect> ActiveEffects { get; private set; } = new();

    private void Awake()
    {
        if (_defaultAttribtues == null)
        {
            Debug.LogError("DefaultAttributes not set");
        }

        Attributes = new(_defaultAttribtues);
    }

    public void ApplyEffect(Effect effect)
    {
        if (effect == null)
            return;

        if (effect.Duration != 0f)
        {
            ActiveEffects.Add(effect);
            foreach (EffectModifier modifier in effect.Modifiers)
            {
                Attributes.GetAttribute(modifier.AttributeType).ActiveModifiers.Add(modifier);
            }
            RecalculateValue(effect.GetAffectedAttributes());
        }

        StartCoroutine(EffectExecution(effect));
    }

    public void RemoveEffect(Effect effect)
    {
        if (effect == null)
            return;

        if (effect.Duration != 0f)
        {
            ActiveEffects.Remove(effect);
            foreach (EffectModifier modifier in effect.Modifiers)
            {
                Attributes.GetAttribute(modifier.AttributeType).ActiveModifiers.Remove(modifier);
            }
            RecalculateValue(effect.GetAffectedAttributes());
        }
    }

    private void RecalculateBaseValue(Effect effect)
    {
        foreach (EffectModifier modifier in effect.Modifiers)
        {
            Attribute attribute = Attributes.GetAttribute(modifier.AttributeType);
            float newValue = 0f;
            switch (modifier.ValueOperator)
            {
                case ValueOperator.Add:
                    newValue = attribute.BaseValue + modifier.Value;
                    break;
                case ValueOperator.Multiply:
                    newValue = attribute.BaseValue * (1f + (modifier.Value - 1f));
                    break;
                case ValueOperator.Divide:
                    newValue = attribute.BaseValue / (1f + (modifier.Value - 1f));
                    break;
            }
            Attributes.OnPreAttributeChange(attribute, ref newValue);
            attribute.BaseValue = newValue;
            Attributes.OnPostAttributeChange(attribute);
        }
    }

    private void RecalculateValue(HashSet<AttributeType> attrTypes)
    {
        foreach (AttributeType attrType in attrTypes)
        {
            RecalculateValue(attrType);
        }
    }

    private void RecalculateValue(AttributeType attrType)
    {
        Attribute attribute = Attributes.GetAttribute(attrType);
        if (attribute == null)
            return;

        float newVal = 0f;

        if (attribute.ActiveModifiers.Count == 0)
        {
            newVal = attribute.BaseValue;
        }
        else
        {
            float Add = SumMods(attribute.ActiveModifiers, ValueOperator.Add, 0f);
            float Multiply = SumMods(attribute.ActiveModifiers, ValueOperator.Multiply, 1f);
            float Divide = SumMods(attribute.ActiveModifiers, ValueOperator.Divide, 1f);
            Debug.Log($"Add {Add} | Multiply {Multiply} | Divide {Divide}");
            newVal = (attribute.BaseValue + Add) * Multiply / Divide;
        }

        Attributes.OnPreAttributeChange(attribute, ref newVal);
        Debug.Log($"[{gameObject.name}] {attrType} {attribute.CurrentValue} -> {newVal}");
        attribute.CurrentValue = newVal;
        Attributes.OnPostAttributeChange(attribute);
    }

    private float SumMods(List<EffectModifier> modifiers, ValueOperator type, float bias)
    {
        float result = bias;

        foreach (EffectModifier modifier in modifiers)
        {
            if (modifier.ValueOperator == type)
            {
                result += modifier.Value - bias;
            }
        }

        return result;
    }

    private IEnumerator EffectExecution(Effect effect)
    {
        if (effect.TickTime == 0f && effect.Duration != 0f)
        {
            yield return new WaitForSeconds(effect.Duration);
        }
        else
        {
            float timePassed = 0f;
            while (timePassed <= effect.Duration)
            {
                RecalculateBaseValue(effect);
                yield return new WaitForSeconds(effect.TickTime);
                timePassed += Time.deltaTime;
            }
        }

        RemoveEffect(effect);
    }
}