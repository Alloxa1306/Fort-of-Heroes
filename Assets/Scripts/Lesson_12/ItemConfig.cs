using UnityEngine;

public enum PerkType
{
    Heal,      
    SpeedBoost,   
    FireRateBoost, 
    BounceAmmo,
    Shield
}

[CreateAssetMenu(fileName = "NewItemConfig", menuName = "Inventory/ItemConfig")]
public class ItemConfig : ScriptableObject
{
    public string itemName = "Перк";
    [TextArea(2, 4)] public string description;
    public Sprite icon;
    public PerkType perkType;
    public float value = 10f;  
    public float duration = 15f; 
}