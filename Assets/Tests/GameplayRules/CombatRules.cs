using UnityEngine;

public static class CombatRules
{
    public static int ApplyDamage(int currentHealth, int damage)
    {
        return Mathf.Max(0, currentHealth - Mathf.Max(0, damage));
    }

    public static int ApplyHealing(int currentHealth, int maxHealth, int healing)
    {
        return Mathf.Clamp(currentHealth + Mathf.Max(0, healing), 0, Mathf.Max(0, maxHealth));
    }

    public static Vector3 CalculateDashPosition(Vector3 start, Vector3 direction, float distance)
    {
        if (direction.sqrMagnitude < 0.0001f || distance <= 0f) return start;
        return start + direction.normalized * distance;
    }

    public static bool Roll(float chance, float randomValue)
    {
        if (chance <= 0f) return false;
        if (chance >= 100f) return true;
        return randomValue < Mathf.Clamp01(chance / 100f);
    }

    public static bool CanAddToInventory(int itemCount, int maxSlots)
    {
        return itemCount >= 0 && maxSlots > 0 && itemCount < maxSlots;
    }
}
