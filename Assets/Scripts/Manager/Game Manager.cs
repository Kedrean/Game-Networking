using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public Transform blueSpawn;
    public Transform redSpawn;

    int redScore;
    int blueScore;

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetSpawnPoint(ulong clientId)
    {
        return clientId == 0
            ? blueSpawn
            : redSpawn;
    }

    public void PlayerKilled(ulong deadPlayer)
    {
        if (!IsServer) return;

        if (deadPlayer == 0)
            blueScore++;
        else
            redScore++;

        UpdateScoreClientRpc(
            redScore,
            blueScore);

        if (redScore >= 5)
            ShowWinnerClientRpc("RED WINS!");

        if (blueScore >= 5)
            ShowWinnerClientRpc("BLUE WINS!");
    }

    [ClientRpc]
    void UpdateScoreClientRpc(int red, int blue)
    {
        UIManager.Instance.UpdateScore(red, blue);
    }

    [ClientRpc]
    void ShowWinnerClientRpc(string winner)
    {
        UIManager.Instance.ShowWinner(winner);
    }
}