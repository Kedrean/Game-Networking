using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private int pointsToWin = 50;

    public bool IsGameEnded => gameEnded;

    private bool gameEnded = false;

    private bool hostRematchReady = false;
    private bool clientRematchReady = false;

    private ulong hostClientId;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            hostClientId = NetworkManager.Singleton.LocalClientId;
        }
    }

    //==========================================================
    // WIN CONDITIONS
    //==========================================================

    public void CheckScoreWinCondition()
    {
        if (!NetworkManager.Singleton.IsServer || gameEnded)
            return;

        int hostScore = ScoreManager.Instance.GetHostScore();
        int clientScore = ScoreManager.Instance.GetClientScore();

        if (hostScore >= pointsToWin)
        {
            EndGame(hostClientId);
        }
        else if (clientScore >= pointsToWin)
        {
            EndGame(GetClientId());
        }
    }

    public void MatchEnded()
    {
        if (!NetworkManager.Singleton.IsServer || gameEnded)
            return;

        int host = ScoreManager.Instance.GetHostScore();
        int client = ScoreManager.Instance.GetClientScore();

        if (host > client)
        {
            EndGame(hostClientId);
        }
        else if (client > host)
        {
            EndGame(GetClientId());
        }
        else
        {
            gameEnded = true;

            TimeManager.Instance.StopTimer();

            ShowWinnerRpc(ulong.MaxValue);
        }
    }

    private void EndGame(ulong winnerId)
    {
        gameEnded = true;

        TimeManager.Instance.StopTimer();

        FreezePlayersRpc();

        ShowWinnerRpc(winnerId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void FreezePlayersRpc()
    {
        foreach (NetworkPlayer player in FindObjectsByType<NetworkPlayer>(
             FindObjectsSortMode.None))
        {
            player.SetFrozen(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UnfreezePlayersRpc()
    {
        foreach (NetworkPlayer player in FindObjectsByType<NetworkPlayer>(
             FindObjectsSortMode.None))
        {
            player.SetFrozen(false);
        }
    }

    //==========================================================
    // UI
    //==========================================================

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowWinnerRpc(ulong winnerId)
    {
        MultiplayerUI.Instance.ShowEndMenu(winnerId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HideWinnerRpc()
    {
        MultiplayerUI.Instance.HideEndMenu();
    }

    //==========================================================
    // REMATCH
    //==========================================================

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestRematchRpc(ulong clientId)
    {
        if (clientId == hostClientId)
        {
            hostRematchReady = true;
        }
        else
        {
            clientRematchReady = true;
        }

        if (hostRematchReady && clientRematchReady)
        {
            RestartMatch();
        }
    }

    public void RequestRematch()
    {
        RequestRematchRpc(NetworkManager.Singleton.LocalClientId);
    }

    private void RestartMatch()
    {
        gameEnded = false;

        hostRematchReady = false;
        clientRematchReady = false;

        ScoreManager.Instance.ResetScores();
        TimeManager.Instance.ResetTimer();
        CollectibleSpawner.Instance.ResetCollectibles();

        TimeManager.Instance.StartTimer();

        UnfreezePlayersRpc();

        HideWinnerRpc();
    }

    //==========================================================
    // HELPERS
    //==========================================================

    private ulong GetClientId()
    {
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (id != hostClientId)
                return id;
        }

        return ulong.MaxValue;
    }
}