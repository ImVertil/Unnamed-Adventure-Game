using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Attack", menuName = "Player/New Weapon Attack")]
public class PlayerWeaponAttack : ScriptableObject
{
    public AnimationClip Clip;
    [Range(0f, 1f)]
    public float NextAttackWindowTime;
    [Range(0f, 5f)]
    public float HitWidth;
    [Range(0f, 5f)]
    public float HitHeight;
}
