using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Physical Damage Calculator", menuName = "Effect/Physical Damage Calculator")]
public class PhysicalDamageCalculator : ExecutionCalculator
{
    public override float GetCalculatedValue(CharacterAttributes source, CharacterAttributes target, float value)
    {
        return (-source.PhysicalAttack.CurrentValue + value) * (1f - target.Armor.CurrentValue / 100f);
    }
}