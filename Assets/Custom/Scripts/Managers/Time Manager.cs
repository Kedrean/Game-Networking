using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TimeManager : NetworkBehaviour
{
    public static TimeManager Instance;

    [Header("Game Settings")]
    [SerializeField] private float matchDuration = 180f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    private readonly NetworkVariable<float> remainingTime = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private bool timerRunning = false;
    private bool matchEnded = false;

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        remainingTime.OnValueChanged += OnTimeChanged;

        if (IsServer)
        {
            remainingTime.Value = matchDuration;
        }

        UpdateTimerUI(remainingTime.Value);
    }

    public override void OnNetworkDespawn()
    {
        remainingTime.OnValueChanged -= OnTimeChanged;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if (!timerRunning)
            return;

        if (matchEnded)
            return;

        remainingTime.Value -= Time.deltaTime;

        if (remainingTime.Value <= 0f)
        {
            remainingTime.Value = 0f;

            timerRunning = false;
            matchEnded = true;

            GameManager.Instance.MatchEnded();
        }
    }

    private void OnTimeChanged(float previousValue, float newValue)
    {
        UpdateTimerUI(newValue);
    }

    private void UpdateTimerUI(float time)
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartTimer()
    {
        if (!IsServer)
            return;

        timerRunning = true;
        matchEnded = false;
    }

    public void StopTimer()
    {
        if (!IsServer)
            return;

        timerRunning = false;
    }

    public void ResetTimer()
    {
        if (!IsServer)
            return;

        timerRunning = false;
        matchEnded = false;
        remainingTime.Value = matchDuration;
    }

    public float GetRemainingTime()
    {
        return remainingTime.Value;
    }
}