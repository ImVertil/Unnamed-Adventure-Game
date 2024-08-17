using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class MeleeDamageHandler : DamageHandler
{
    // TODO: rewrite, lots of unnecessary stuff
    [SerializeField] private Effect _effect;
    private BoxCollider _attackCollider;
    private CharacterAttributeManager _source;
    private HashSet<GameObject> _enemies = new();
    
    private void Start()
    {
        _source = GetComponent<CharacterAttributeManager>();
    }

    private void Update() // test
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DealDamage();
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(Tags.ENEMY))
            return;

        _enemies.Add(other.gameObject);
        Debug.LogWarning($"{other.name} enter");
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag(Tags.ENEMY))
            return;

        _enemies.Remove(other.gameObject);
        Debug.LogWarning($"{other.name} exit");
    }

    protected override void DealDamage()
    {   
        HashSet<GameObject> localEnemies = new(_enemies);
        foreach (GameObject enemy in localEnemies)
        {
            CharacterAttributeManager targetAttributeManager = enemy.GetComponent<CharacterAttributeManager>();
            if (targetAttributeManager != null)
            {
                EffectData data = new();
                data.Source = _source.Attributes;
                data.Target = targetAttributeManager.Attributes;
                data.Effect = _effect;
                targetAttributeManager.ApplyEffect(data);
            }
        }
    }
}