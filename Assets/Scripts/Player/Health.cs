using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public int maxHealth = 100;

    private NetworkVariable<int> health =
        new NetworkVariable<int>(100);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc(int damage)
    {
        ApplyDamage(damage);
    }

    public void ApplyDamage(int damage)
    {
        if (!IsServer) 
            return;

        health.Value -= damage;

        if (health.Value <= 0)
        {
            GameManager.Instance.PlayerKilled(OwnerClientId);
            Respawn();
        }
    }

    private void Respawn()
    {
        health.Value = maxHealth;

        Transform spawn =
            GameManager.Instance.GetSpawnPoint(OwnerClientId);

        transform.position = spawn.position;
    }
}