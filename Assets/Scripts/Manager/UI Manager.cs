using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject gameUI;
    public GameObject hostingUI;

    public TMP_Text scoreText;
    public TMP_Text winnerText;
    public TMP_Text joinCodeText;
    public TMP_InputField joinInput;

    void Awake()
    {
        Instance = this;

        winnerText.gameObject.SetActive(false);
    }

    public void UpdateScore(int blue, int red)
    {
        scoreText.text =
            $"Blue: {blue}   Red: {red}";
    }

    public void ShowWinner(string text)
    {
        winnerText.gameObject.SetActive(true);
        winnerText.text = text;
    }

    public void ShowJoinCode(string code)
    {
        joinCodeText.text =
            "CODE: " + code;
    }

    public void HostGame()
    {
        RelayManager.Instance.CreateRelay();
        gameUI.SetActive(true);
        hostingUI.SetActive(false);
    }

    public void JoinGame()
    {
        RelayManager.Instance.JoinRelay(
            joinInput.text);
        gameUI.SetActive(true);
        hostingUI.SetActive(false);
    }
}