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

        if (effect.Duration != 0)
        {
            ActiveEffects.Add(effect);
            RecalculateAttribute(effect.GetAffectedAttributes());
            effect.OnEffectEnd += RemoveEffect;
        }
        
        //StartCoroutine(TimedEffectExecution(effect));
    }

    public void RemoveEffect(Effect effect)
    {
        if (effect == null)
            return;

        if (effect.Duration != 0)
        {
            ActiveEffects.Remove(effect);
            RecalculateAttribute(effect.GetAffectedAttributes());
            effect.OnEffectEnd -= RemoveEffect;
        }
    }

    private void RecalculateAttribute(HashSet<AttributeType> attributeTypes)
    {
        foreach (AttributeType attributeType in attributeTypes)
        {
            RecalculateAttribute(attributeType);
        }
    }

    private void RecalculateAttribute(AttributeType attributeType)
    {
        Attribute attribute = Attributes.GetAttribute(attributeType);
        if (attribute == null)
            return;

        List<EffectModifier> modifiers = new();
        foreach (Effect effect in ActiveEffects)
        {
            foreach (EffectModifier modifier in effect.Modifiers)
            {
                if (modifier.Attribute == attributeType)
                {
                    modifiers.Add(modifier);
                }
            }
        }
        float Add = SumMods(modifiers, ValueOperator.Add, 0f);
        float Multiply = SumMods(modifiers, ValueOperator.Multiply, 1f);
        float Divide = SumMods(modifiers, ValueOperator.Divide, 1f);
        Debug.Log($"Add {Add} | Multiply {Multiply} | Divide {Divide}");
        float newVal = (attribute.BaseValue + Add) * Multiply / Divide;

        Attributes.OnPreAttributeChange(attribute, ref newVal);
        Debug.Log($"[{gameObject.name}] {attributeType} {attribute.CurrentValue} -> {newVal}");
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

    private IEnumerator TimedEffectExecution(Effect effect)
    {
        float timePassed = 0f;

        while (timePassed <= effect.Duration)
        {
            //do effect execution stuff


            yield return new WaitForSeconds(effect.TickTime);
            timePassed += Time.deltaTime;
        }

        effect.OnEffectEnd?.Invoke(effect);
    }
}