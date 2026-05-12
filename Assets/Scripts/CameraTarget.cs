using UnityEngine;
using Unity.Netcode;

public class CameraTarget : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(transform);
        }
    }
}
