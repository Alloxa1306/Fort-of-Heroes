using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkObject))]
public class QuickItemPickup : NetworkBehaviour
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
            if (other.GetComponentInParent<PlayerQuickSlots>() is PlayerQuickSlots quickSlots)
            {
                if (quickSlots.TryPickupItem(itemConfig))
                {
                    if (playerNetObj != null) RequestDestroyItemServerRpc();
                    else Destroy(gameObject);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDestroyItemServerRpc()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}