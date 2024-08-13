using UnityEngine;

[CreateAssetMenu(fileName = "DefaultAttributes", menuName = "Attributes/New Default Attributes")]
public class DefaultAttributes : ScriptableObject
{
    public float MaxHealth;
    public float MaxMana;
    public float MaxStamina;
    public float PhysicalAttack;
    public float MagicAttack;
    public float Armor;
    public float MagicArmor;
}
