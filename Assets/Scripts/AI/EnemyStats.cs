using NUnit.Framework;
using UnityEngine;


public enum EnemyAttackType
{
    Melee,
    Ranged,
    SpecialAttack
}

[CreateAssetMenu(menuName = "Stats/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Stats")]
    public float maxHealth;
    public float maxMana;
    public float strength;
    public float defense;

    [Header("Move Stats")]
    public float moveSpeed;

    [Header("Attack stats")]
    public float attackRange;
    public float detectionRange;
    public float attackCooldown;
    public float stunDuration;

    public EnemyAttackType attackType;
}

[CreateAssetMenu(menuName = "Combos/Enemy Combo")]
public class EnemyCombo : ScriptableObject
{
    [Header("Combo Setup")]

    public string[] animationTriggers;

    [Header("Damage")]
    public float[] damageValues;

    [Header("Timing")]
    public float[] hitDelays;
}