using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public Transform blueSpawn;
    public Transform redSpawn;

    int redScore;
    int blueScore;

    private NetworkVariable<bool> gameEnded =
        new NetworkVariable<bool>(false);

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
        if (!IsServer) 
            return;

        if (gameEnded.Value) 
            return;

        if (deadPlayer == 0)
            redScore++;
        else
            blueScore++;

        UpdateScoreClientRpc(
            blueScore,
            redScore);

        if (blueScore >= 5)
        {
            gameEnded.Value = true;
            ShowWinnerClientRpc("BLUE WINS!");
        }

        if (redScore >= 5)
        {
            gameEnded.Value = true;
            ShowWinnerClientRpc("RED WINS!");
        }
    }

    public bool IsGameOver()
    {
        return gameEnded.Value;
    }

    [ClientRpc]
    void UpdateScoreClientRpc(int blue, int red)
    {
        UIManager.Instance.UpdateScore(blue, red);
    }

    [ClientRpc]
    void ShowWinnerClientRpc(string winner)
    {
        gameEnded.Value = true;
        UIManager.Instance.ShowWinner(winner);
    }
}