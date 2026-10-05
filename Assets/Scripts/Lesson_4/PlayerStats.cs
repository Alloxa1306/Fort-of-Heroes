using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerStats
{
    public string playerId = Guid.NewGuid().ToString(); // Уникальный ID персонажа

    public int shotsCount; // Счётчик выстрелов
    public int killsCount; // Счётчик убийств 
    public int hitsCount; // Количество попаданий по врагу 
    public int coinsCount; // количество нафармленых монет за уюиство врагов
    public float totalDamageDealt; // Количество нанесенного урона
    public float timeSurvived; // Время жизни

    public int globalTotalKills; // Общее количество убийств за всю игру
    public int globalTotalCoinsEarned; // Заработано монет за всю игру
    public int[] monsterKillsByID = new int[20]; // Массив для убийств конкретных монстров
    public int[] matchMonsterKillsByID = new int[20]; // Локальные убийства по ID для HUD

    // ID выбранного в данный момент персонажа
    public string selectedCharacterId = "default_hero";

    // Список ID персонажей, которыми владеет игрок
    public List<string> unlockedCharacterIds = new List<string>()
    {
        "default_hero" // Стандартный персонаж открыт сразу
    };
}
