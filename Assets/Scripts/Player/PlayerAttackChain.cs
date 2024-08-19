using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Chain", menuName = "Player/New Attack Chain")]
public sealed class PlayerAttackChain : ScriptableObject
{
    public PlayerWeaponAttack[] Attacks;
    public int AttacksAmount => Attacks.Length;
}
