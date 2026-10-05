using NUnit.Framework;
using UnityEngine;


public class HealthTests
{

    [Test]
    public void Damage_DoesNotMakeHealthNegative()
    {
        Assert.AreEqual(0, CombatRules.ApplyDamage(10, 15));
    }

    [Test]
    public void Healing_DoesNotExceedMaximumHealth()
    {
        Assert.AreEqual(100, CombatRules.ApplyHealing(90, 100, 25));
    }
}

public class MovementTests
{
    [Test]
    public void Dash_UsesNormalizedDirection()
    {
        Vector3 result = CombatRules.CalculateDashPosition(Vector3.zero, new Vector3(2f, 0f, 0f), 5f);
        Assert.AreEqual(new Vector3(5f, 0f, 0f), result);
    }
}

public class LootTests
{
    [Test]
    public void Loot_ZeroChanceNeverDrops()
    {
        Assert.IsFalse(CombatRules.Roll(0f, 0f));
    }

    [Test]
    public void Loot_HundredPercentAlwaysDrops()
    {
        Assert.IsTrue(CombatRules.Roll(100f, 0.9999f));
    }
}

public class InventoryTests
{
    [Test]
    public void Inventory_RejectsItemsWhenCapacityIsReached()
    {
        Assert.IsFalse(CombatRules.CanAddToInventory(6, 6));
    }
}
