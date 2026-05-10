using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerCount : NetworkBehaviour
{
	public TMP_Text playerCountText;

	private NetworkVariable<int> playerCount = new NetworkVariable<int>();

	public override void OnNetworkSpawn()
	{
		playerCount.OnValueChanged += OnPlayerCountChanged;

		if (IsServer)
		{
			UpdatePlayerCount();

			NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
		}

		playerCountText.text = "Players: " + playerCount.Value;
	}

	private void OnClientConnected(ulong clientId)
	{
		UpdatePlayerCount();
	}

	private void OnClientDisconnected(ulong clientId)
	{
		UpdatePlayerCount();
	}

	private void UpdatePlayerCount()
	{
		playerCount.Value = NetworkManager.Singleton.ConnectedClientsList.Count;
	}

	private void OnPlayerCountChanged(int oldValue, int newValue)
	{
		playerCountText.text = "Players: " + newValue;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		if (NetworkManager.Singleton == null) return;

		if (IsServer)
		{
			NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
		}
	}
}
