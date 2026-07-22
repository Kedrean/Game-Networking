using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerUI : MonoBehaviour
{
    public static MultiplayerUI Instance;

    [Header("Menu")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject endMenu;

    [Header("End Menu")]
    [SerializeField] private TMP_Text winnerText;

    [Header("GUI")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text statusText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (joinCodeText != null)
            joinCodeText.text = "";

        if (statusText != null)
            statusText.text = "Ready";

        if (startMenu != null)
            startMenu.SetActive(true);

        if (endMenu != null)
            endMenu.SetActive(false);

        MultiplayerManager.Instance.OnHostStarted += HideMenu;
        MultiplayerManager.Instance.OnClientJoined += HideMenu;

        MultiplayerManager.Instance.OnJoinCodeGenerated += UpdateJoinCode;
        MultiplayerManager.Instance.OnError += DisplayError;

        MultiplayerManager.Instance.OnSessionEnded += ReturnToStartMenu;
    }

    private void OnDestroy()
    {
        if (MultiplayerManager.Instance == null)
            return;

        MultiplayerManager.Instance.OnHostStarted -= HideMenu;
        MultiplayerManager.Instance.OnClientJoined -= HideMenu;

        MultiplayerManager.Instance.OnJoinCodeGenerated -= UpdateJoinCode;
        MultiplayerManager.Instance.OnError -= DisplayError;

        MultiplayerManager.Instance.OnSessionEnded -= ReturnToStartMenu;
    }

    //========================================================
    // HOST / JOIN
    //========================================================

    public void HostGame()
    {
        if (statusText != null)
            statusText.text = "Creating Host...";

        MultiplayerManager.Instance.HostGame();
    }

    public void JoinGame()
    {
        string code = joinCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code))
        {
            if (statusText != null)
                statusText.text = "Enter a Join Code";

            return;
        }

        if (statusText != null)
            statusText.text = "Joining...";

        MultiplayerManager.Instance.JoinGame(code);
    }

    //========================================================
    // END MENU
    //========================================================

    public void ShowEndMenu(ulong winnerId)
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (startMenu != null)
            startMenu.SetActive(false);

        if (endMenu != null)
            endMenu.SetActive(true);

        if (winnerText == null)
            return;

        if (winnerId == ulong.MaxValue)
        {
            winnerText.text = "Draw!";
        }
        else if (NetworkManager.Singleton.LocalClientId == winnerId)
        {
            winnerText.text = "You Win!";
        }
        else
        {
            winnerText.text = "Opponent Wins!";
        }
    }

    public void HideEndMenu()
    {
        if (endMenu != null)
            endMenu.SetActive(false);

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    //========================================================
    // REMATCH / END SESSION
    //========================================================

    public void Rematch()
    {
        GameManager.Instance.RequestRematch();
    }

    public void EndSession()
    {
        MultiplayerManager.Instance.EndSession();
    }

    //========================================================
    // EVENTS
    //========================================================
    private void UpdateJoinCode(string code)
    {
        if (joinCodeText != null)
            joinCodeText.text = $"Join Code: {code}";

        if (statusText != null)
            statusText.text = "Hosting";
    }

    private void DisplayError(string error)
    {
        if (statusText != null)
            statusText.text = error;
    }

    private void HideMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    private void ReturnToStartMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (startMenu != null)
            startMenu.SetActive(true);

        if (endMenu != null)
            endMenu.SetActive(false);

        if (winnerText != null)
            winnerText.text = "";

        if (joinCodeInput != null)
            joinCodeInput.text = "";

        if (joinCodeText != null)
            joinCodeText.text = "";

        if (statusText != null)
            statusText.text = "Ready";
    }
}