using Unity.Netcode;
using UnityEngine;

public class RemotePlayerSetup : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        // »щем спавнер и просим его настроить игрока
        var spawner = FindObjectOfType<PlayerSpawner>();
        if (spawner != null)
        {
            spawner.SetupPlayerLocally(GetComponent<NetworkObject>());
        }
    }
}