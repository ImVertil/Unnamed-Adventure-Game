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
}