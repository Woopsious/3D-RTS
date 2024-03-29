using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : NetworkBehaviour
{
	public static ClientManager Instance;

	public string clientUsername = "PlayerName";
	public string clientId;
	public ulong clientNetworkedId;

	public void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(Instance);
		}
		else
			Destroy(gameObject);
	}
	public void StartClient(Lobby lobby)
	{
		LobbyManager.Instance.JoinLobby(lobby);
		MultiplayerManager.Instance.SubToEvents();

		GameManager.Instance.isPlayerOne = false;
		GameManager.Instance.isMultiplayerGame = true;
	}
	public void StopClient()
	{
		GameManager.Instance.isPlayerOne = true;
		GameManager.Instance.isMultiplayerGame = false;

		LobbyManager.Instance._Lobby = null;
		MultiplayerManager.Instance.UnsubToEvents();
		MultiplayerManager.Instance.ShutDownNetworkManagerIfActive();
	}
	//join relay server
	public IEnumerator RelayConfigureTransportAsConnectingPlayer()
	{
		// Populate RelayJoinCode beforehand through the UI
		var clientRelayUtilityTask = JoinRelayServerFromJoinCode(LobbyManager.Instance.lobbyJoinCode);

		while (!clientRelayUtilityTask.IsCompleted)
		{
			yield return null;
		}

		if (clientRelayUtilityTask.IsFaulted)
		{
			Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
			yield break;
		}

		var relayServerData = clientRelayUtilityTask.Result;

		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
		yield return null;

		NetworkManager.Singleton.StartClient();
		ClientManager.Instance.clientNetworkedId = NetworkManager.Singleton.LocalClientId;
	}
	public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
	{
		JoinAllocation allocation;
		try
		{
			allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
		}
		catch
		{
			Debug.LogError("Relay create join code request failed");
			throw;
		}
		return new RelayServerData(allocation, "dtls");
	}

	//handle disconnects
	public void HandlePlayerDisconnectsAsClient()
	{
		GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Connection to Host Lost", 3f);
		StopClient();

		if (SceneManager.GetActiveScene().buildIndex == 0)
			StartCoroutine(MenuUIManager.Instance.DelayLobbyListRefresh());
		else
		{
			if (GameManager.Instance.hasGameEnded.Value == false)
				GameManager.Instance.gameUIManager.ShowPlayerDisconnectedPanel();
			else if (GameManager.Instance.hasGameEnded.Value == true)
			{
				GameManager.Instance.gameUIManager.playAgainButtonObj.SetActive(false);
				GameManager.Instance.gameUIManager.playAgainUiText.text = "Other Player Left";
			}
		}
	}
}
