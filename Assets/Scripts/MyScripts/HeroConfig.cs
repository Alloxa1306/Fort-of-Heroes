using UnityEngine;

[CreateAssetMenu(fileName = "NewHeroConfig", menuName = "Configs/HeroConfig")]
public class HeroConfig : ScriptableObject
{
    public string heroId;          // Уникальный ID (например: "blue_knight", "samurai")
    public string heroName;        // Имя для UI
    public int price;              // Цена в монетах
    public GameObject heroPrefab;  // Префаб персонажа для игры
}