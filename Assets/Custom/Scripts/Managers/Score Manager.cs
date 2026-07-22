using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    [Header("UI")]
    [SerializeField] private TMP_Text hostScoreText;
    [SerializeField] private TMP_Text clientScoreText;

    private readonly NetworkVariable<int> hostScore = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> clientScore = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

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

        hostScore.OnValueChanged += OnHostScoreChanged;
        clientScore.OnValueChanged += OnClientScoreChanged;

        UpdateUI();
    }

    public override void OnNetworkDespawn()
    {
        hostScore.OnValueChanged -= OnHostScoreChanged;
        clientScore.OnValueChanged -= OnClientScoreChanged;
    }

    public void AddPoint(ulong clientId, int points)
    {
        Debug.Log("AddPoint");

        if (!IsServer)
            return;

        if (clientId == hostClientId)
        {
            hostScore.Value += points;
        }
        else
        {
            clientScore.Value += points;
        }

        Debug.Log(GameManager.Instance);
        Debug.Log("About to call CheckScoreWinCondition");
        GameManager.Instance.CheckScoreWinCondition();
    }

    public void ResetScores()
    {
        if (!IsServer)
            return;

        hostScore.Value = 0;
        clientScore.Value = 0;
    }

    private void OnHostScoreChanged(int previous, int current)
    {
        UpdateUI();
    }

    private void OnClientScoreChanged(int previous, int current)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hostScoreText == null || clientScoreText == null)
            return;

        bool isHost = NetworkManager.Singleton.LocalClientId == hostClientId;

        if (isHost)
        {
            hostScoreText.text = $"Your Score : {hostScore.Value}";
            clientScoreText.text = $"Opponent Score : {clientScore.Value}";
        }
        else
        {
            hostScoreText.text = $"Your Score : {clientScore.Value}";
            clientScoreText.text = $"Opponent Score : {hostScore.Value}";
        }
    }

    public int GetHostScore()
    {
        return hostScore.Value;
    }

    public int GetClientScore()
    {
        return clientScore.Value;
    }
}