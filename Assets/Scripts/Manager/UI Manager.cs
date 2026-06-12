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
    }

    public void UpdateScore(int red, int blue)
    {
        scoreText.text =
            $"Red: {red}   Blue: {blue}";
    }

    public void ShowWinner(string text)
    {
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