using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfigTest", menuName = "Configs/PlayerConfigTest")]
public class PlayerConfigTest : ScriptableObject
{
    public float moveSpeed = 5f;
    public int maxHealth = 100;
    public int damage = 10;
}