public class DummyEnemyConfigProvider : IEnemyConfig
{
    public int EnemyHealth => 1000000000; 
    public float EnemySpeed => 0.8f;
    public float EnemyDamage => 0f; 
    public float EnemyAttackRange => 1.2f;
}