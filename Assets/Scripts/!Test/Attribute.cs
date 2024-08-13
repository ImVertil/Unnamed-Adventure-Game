public class Attribute
{
    public float BaseValue;
    public float CurrentValue;
    //public List<EffectModifier> Modifiers { get; private set; } = new();

    public Attribute(float baseValue)
    {
        BaseValue = baseValue;
        CurrentValue = baseValue;
    }
}