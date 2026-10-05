using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class EnemyLootEntry
{
    public ItemConfig item;
    public GameObject pickupPrefab;
    [Range(0f, 100f)] public float chance = 25f;
}

public class EnemyLootDrop : MonoBehaviour
{
    [SerializeField] private List<EnemyLootEntry> entries = new List<EnemyLootEntry>();
    [SerializeField] private Transform dropPoint;
    [SerializeField] private bool allowMultipleDrops;

    public IReadOnlyList<EnemyLootEntry> Entries => entries;

    public void DropLoot()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) return;

        Transform point = dropPoint != null ? dropPoint : transform;
        foreach (EnemyLootEntry entry in entries)
        {
            if (entry == null || entry.pickupPrefab == null) continue;
            if (!CombatRules.Roll(entry.chance, UnityEngine.Random.value)) continue;

            GameObject pickup = Instantiate(entry.pickupPrefab, point.position, Quaternion.identity);
            ItemPickup itemPickup = pickup.GetComponent<ItemPickup>();
            if (itemPickup != null && entry.item != null) itemPickup.SetItem(entry.item);
            NetworkObject networkObject = pickup.GetComponent<NetworkObject>();
            if (networkObject != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                networkObject.Spawn();
            }
            else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning($"[EnemyLootDrop] {entry.pickupPrefab.name} has no NetworkObject and cannot be synchronized.");
            }

            if (!allowMultipleDrops) break;
        }
    }
}
