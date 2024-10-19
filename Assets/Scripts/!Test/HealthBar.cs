using Unity.Cinemachine;
using Events.AbilitySystem;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _name;

    [SerializeField]
    private TextMeshProUGUI _health;

    [SerializeField]
    private TextMeshProUGUI _armor;

    private void Start()
    {
        CharacterAttributeManager manager = GetComponentInParent<CharacterAttributeManager>();
        manager.Attributes.OnHealthChanged += UpdateHealth;
        manager.Attributes.OnArmorChanged += UpdateArmor;
        _armor.text = manager.Attributes.Armor.CurrentValue.ToString();
    }

    private void UpdateArmor(float newValue)
    {
        _armor.text = newValue.ToString();
    }

    private void UpdateHealth(float newValue)
    {
        float diff = float.Parse(_health.text) - newValue; // use later with flying text
        _health.text = newValue.ToString();
    }
}
