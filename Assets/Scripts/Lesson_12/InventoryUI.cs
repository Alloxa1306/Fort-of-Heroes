using UnityEngine;
using Unity.Netcode;

public class InventoryUI : MonoBehaviour
{
    [Header("—сылки")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private InventorySlotUI slotPrefab;

    private void Start()
    {
        PlayerSpawner.OnLocalPlayerSpawned += HandlePlayerSpawned;
        if(PlayerSpawner.LocalPlayerObject != null)
        {
            HandlePlayerSpawned(PlayerSpawner.LocalPlayerObject);
        }    
        //TryBindInventory();
    }

    //private void Update()
    //{
    //    // »щем инвентарь игрока каждый кадр, пока он не по€витс€ на сцене
    //    if (playerInventory == null)
    //    {
    //        TryBindInventory();
    //    }
    //}

    private void OnDestroy()
    {
        PlayerSpawner.OnLocalPlayerSpawned -= HandlePlayerSpawned;

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshUI;
        }
    }

    private void HandlePlayerSpawned(GameObject playerObject)
    {
        playerInventory = playerObject.GetComponent<PlayerInventory>();
        //PlayerInventory[] inventories = FindObjectsOfType<PlayerInventory>();

        //foreach (var inv in inventories)
        //{
        //    if (inv.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsOwner)
        //    {
        //        playerInventory = inv;
        //        playerInventory.OnInventoryChanged -= RefreshUI;
        //        playerInventory.OnInventoryChanged += RefreshUI;
        //        RefreshUI();
        //        break;
        //    }
        //}
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshUI;
            playerInventory.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (slotsContainer == null || slotPrefab == null || playerInventory == null) return;

        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }

        // —оздаем новые слоты по списку предметов
        foreach (var item in playerInventory.Items)
        {
            var slotInstance = Instantiate(slotPrefab, slotsContainer);
            slotInstance.SetupSlot(item, playerInventory);
        }
    }
}