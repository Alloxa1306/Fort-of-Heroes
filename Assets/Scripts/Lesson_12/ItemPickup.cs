using UnityEngine;
using Unity.Netcode; 

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkObject))] 
public class ItemPickup : NetworkBehaviour 
{
    [SerializeField] private ItemConfig itemConfig;
    [SerializeField] private float rotationSpeed = 50f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject playerNetObj = other.GetComponentInParent<NetworkObject>();

        if (playerNetObj == null || playerNetObj.IsOwner)
        {
            if (other.GetComponentInParent<PlayerInventory>() is PlayerInventory inventory)
            {
                if (inventory.AddItem(itemConfig))
                {
                    if (playerNetObj != null) RequestDestroyItemServerRpc();
                    else Destroy(gameObject);
                }
            }
        }
    }

    // RequireOwnership = false ��������� ������ ������� ������� ������� "� �������� �������, ������"
    [ServerRpc(RequireOwnership = false)]
    private void RequestDestroyItemServerRpc()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    public void SetItem(ItemConfig config)
    {
        if (config != null) itemConfig = config;
    }
}
