using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeDamageHandler : DamageHandler
{
    // TODO: rewrite, lots of unnecessary stuff
    [SerializeField] private Effect _effect;
    private BoxCollider _attackCollider;
    private CharacterAttributes _playerStats;
    private HashSet<GameObject> _enemies = new();
    
    private void Start()
    {

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
                targetAttributeManager.ApplyEffect(_effect);
            }
        }
    }
}