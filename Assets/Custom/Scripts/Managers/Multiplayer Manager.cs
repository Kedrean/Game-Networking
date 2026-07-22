using System;
using System.Threading.Tasks;
using UnityEngine;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    [Header("Relay Settings")]
    [SerializeField] private int maxPlayers = 2;
    [SerializeField] private string connectionType = "dtls";

    public string JoinCode { get; private set; }

    public event Action<string> OnJoinCodeGenerated;
    public event Action<string> OnError;

    public event Action OnHostStarted;
    public event Action OnClientJoined;

    // NEW
    public event Action OnSessionEnded;

    private bool servicesInitialized;

    private async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        if (servicesInitialized)
            return;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            servicesInitialized = true;

            Debug.Log("Unity Services Initialized");
            Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            OnError?.Invoke(ex.Message);
        }
    }

    public async void HostGame()
    {
        try
        {
            await InitializeServices();

            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

            JoinCode =
                await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, connectionType));

            bool success = NetworkManager.Singleton.StartHost();

            if (!success)
            {
                Debug.LogError("Failed to start Host.");
                return;
            }

            Debug.Log("Host Started");
            Debug.Log("Join Code: " + JoinCode);

            OnHostStarted?.Invoke();
            OnJoinCodeGenerated?.Invoke(JoinCode);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            OnError?.Invoke(ex.Message);
        }
    }

    public async void JoinGame(string joinCode)
    {
        try
        {
            await InitializeServices();

            JoinAllocation allocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, connectionType));

            bool success = NetworkManager.Singleton.StartClient();

            if (!success)
            {
                Debug.LogError("Failed to start Client.");
                return;
            }

            Debug.Log("Joined Host");

            OnClientJoined?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            OnError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Called by the End button.
    /// Host ends the session for everyone.
    /// Client leaves the current session.
    /// </summary>
    public void EndSession()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsListening)
        {
            OnSessionEnded?.Invoke();
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Host ended the session.");
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Client left the session.");
        }

        NetworkManager.Singleton.Shutdown();

        JoinCode = string.Empty;

        OnSessionEnded?.Invoke();
    }

    // Kept for backwards compatibility.
    public void Disconnect()
    {
        EndSession();
    }
}