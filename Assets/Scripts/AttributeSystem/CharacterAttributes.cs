using Character.Effects;
using Events.AbilitySystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CharacterAttributes
{
    public Attribute Health { get; private set; }
    public Attribute MaxHealth { get; private set; }
    public Attribute Mana { get; private set; }
    public Attribute MaxMana { get; private set; }
    public Attribute Stamina { get; private set; }
    public Attribute MaxStamina { get; private set; }
    public Attribute PhysicalAttack { get; private set; }
    public Attribute MagicAttack { get; private set; }
    public Attribute Armor { get; private set; }
    public Attribute MagicArmor { get; private set; }

    public Action<float> OnHealthChanged;
    public Action<float> OnArmorChanged;
    
    private readonly Dictionary<AttributeType, Attribute> _attributeMap;

    public CharacterAttributes(DefaultAttributes attributes)
    {
        Health = new Attribute(attributes.MaxHealth);
        MaxHealth = new Attribute(attributes.MaxHealth);
        Mana = new Attribute(attributes.MaxMana);
        MaxMana = new Attribute(attributes.MaxMana);
        Stamina = new Attribute(attributes.MaxStamina);
        MaxStamina = new Attribute(attributes.MaxStamina);
        PhysicalAttack = new Attribute(attributes.PhysicalAttack);
        MagicAttack = new Attribute(attributes.MagicAttack);
        Armor = new Attribute(attributes.Armor);
        MagicArmor = new Attribute(attributes.MagicArmor);

        _attributeMap = new()
        {
            { AttributeType.Health, Health },
            { AttributeType.MaxHealth, MaxHealth },
            { AttributeType.Mana, Mana },
            { AttributeType.MaxMana, MaxMana },
            { AttributeType.Stamina, Stamina },
            { AttributeType.MaxStamina, MaxStamina },
            { AttributeType.PhysicalAttack, PhysicalAttack },
            { AttributeType.MagicAttack, MagicAttack },
            { AttributeType.Armor, Armor },
            { AttributeType.MagicArmor, MagicArmor },
        };
    }

    public Attribute GetAttribute(AttributeType attributeType)
    {
        return _attributeMap[attributeType];
    }

    public void OnPreAttributeChange(Attribute attribute, ref float newValue)
    {
        if(attribute == Health)
        {
            newValue = Mathf.Clamp(newValue, 0f, MaxHealth.CurrentValue);
        }

        if(attribute == Mana)
        {
            newValue = Mathf.Clamp(newValue, 0f, MaxMana.CurrentValue);
        }

        if (attribute == Stamina)
        {
            newValue = Mathf.Clamp(newValue, 0f, MaxStamina.CurrentValue);
        }

        if (attribute == Armor)
        {
            newValue = Mathf.Max(0f, newValue);
        }
    }

    public void OnPostAttributeChange(Attribute attribute)
    {
        if (attribute == Health)
        {
            OnHealthChanged?.Invoke(attribute.BaseValue);
        }

        if (attribute == Mana)
        {

        }

        if (attribute == Stamina)
        {
            
        }

        if (attribute == Armor)
        {
            OnArmorChanged?.Invoke(attribute.CurrentValue);
        }
    }
}
