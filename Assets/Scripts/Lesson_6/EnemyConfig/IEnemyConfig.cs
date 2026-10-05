using System;
using UnityEngine;

public interface IEnemyConfig
{
    int EnemyHealth { get; }
    float EnemySpeed { get; }
    float EnemyDamage { get; }
    float EnemyAttackRange { get; }
}

[Serializable]
public class EnemyConfig : IEnemyConfig
{
    public int enemyHealth = 20;
    public float enemySpeed = 2.5f;
    public float enemyDamage = 15f;
    public float enemyAttackRange = 1.5f;

    public int EnemyHealth => enemyHealth;
    public float EnemySpeed => enemySpeed;
    public float EnemyDamage => enemyDamage;
    public float EnemyAttackRange => enemyAttackRange;
}