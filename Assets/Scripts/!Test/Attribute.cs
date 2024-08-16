using System.Collections.Generic;

public sealed class Attribute
{
    public float BaseValue;
    public float CurrentValue;
    public List<EffectModifier> ActiveModifiers { get; private set; } = new();

    public Attribute(float baseValue)
    {
        BaseValue = baseValue;
        CurrentValue = baseValue;
    }
}