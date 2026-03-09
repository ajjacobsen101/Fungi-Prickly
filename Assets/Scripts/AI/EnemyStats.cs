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

    public EnemyAttackType attackType;
}

[CreateAssetMenu(menuName = "Combos/Enemy Combo")]
public class EnemyCombo : ScriptableObject
{
    

}