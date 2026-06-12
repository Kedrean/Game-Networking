using System;
using UnityEngine;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    private const string ConnectionType = "wss";

    private async void Awake()
    {
        Instance = this;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log("Services Initialized");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(1);

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(
                    allocation.AllocationId);

            UIManager.Instance.ShowJoinCode(joinCode);

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.UseWebSockets = true;

            transport.SetRelayServerData(
                AllocationUtils.ToRelayServerData(
                    allocation,
                    ConnectionType
                )
            );

            NetworkManager.Singleton.StartHost();

            Debug.Log("Host Started");
        }
        catch (Exception e)
        {
            Debug.LogError($"Create Relay Failed: {e}");
        }
    }

    public async void JoinRelay(string code)
    {
        try
        {
            JoinAllocation allocation =
                await RelayService.Instance.JoinAllocationAsync(code);

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.UseWebSockets = true;

            transport.SetRelayServerData(
                AllocationUtils.ToRelayServerData(
                    allocation,
                    ConnectionType
                )
            );

            NetworkManager.Singleton.StartClient();

            Debug.Log("Client Started");
        }
        catch (Exception e)
        {
            Debug.LogError($"Join Relay Failed: {e}");
        }
    }
}