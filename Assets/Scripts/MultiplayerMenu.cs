using UnityEngine;
using Unity.Netcode;

public class MultiplayerMenu : MonoBehaviour
{
    public GameObject connectionPanel;

	public void StartHost()
    {
        NetworkManager.Singleton.StartHost();

        connectionPanel.SetActive(false);
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();

		connectionPanel.SetActive(false);
	}
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();

		connectionPanel.SetActive(false);
	}
}
