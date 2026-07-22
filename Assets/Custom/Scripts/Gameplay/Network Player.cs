using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayer : NetworkBehaviour
{
    private FirstPersonController controller;
    private Camera playerCamera;
    private AudioListener audioListener;

    private void Awake()
    {
        controller = GetComponent<FirstPersonController>();

        playerCamera = GetComponentInChildren<Camera>(true);

        if (playerCamera != null)
        {
            audioListener = playerCamera.GetComponent<AudioListener>();
        }
    }

    public override void OnNetworkSpawn()
    {
        bool isLocal = IsOwner;

        Debug.Log(
            $"{OwnerClientId} " +
            $"localScale = {transform.localScale}" +
            $" lossyScale = {transform.lossyScale}"
        );

        Debug.Log(
            $"NetworkPlayer Spawned | " +
            $"Object={gameObject.name} | " +
            $"OwnerClientId={OwnerClientId} | " +
            $"LocalClientId={NetworkManager.Singleton.LocalClientId} | " +
            $"IsOwner={IsOwner} | " +
            $"IsServer={IsServer} | " +
            $"IsClient={IsClient}"
        );

        controller.playerCanMove = isLocal;
        controller.cameraCanMove = isLocal;
        controller.enableHeadBob = isLocal;

        if (playerCamera != null)
            playerCamera.enabled = isLocal;

        if (audioListener != null)
            audioListener.enabled = isLocal;
    }

    public void SetFrozen(bool frozen)
    {
        if (!IsOwner)
            return;

        controller.playerCanMove = !frozen;
        controller.cameraCanMove = !frozen;
        controller.enableHeadBob = !frozen;

        if (frozen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}