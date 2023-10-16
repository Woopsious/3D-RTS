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
		Instance = this;
	}
	public void StartClient(Lobby lobby)
	{
		//await CreateNewClientsList();
		LobbyManager.Instance.JoinLobby(lobby);
		Multiplayer.Instance.SubToEvents();

		GameManager.Instance.isPlayerOne = false;
		GameManager.Instance.isMultiplayerGame = true;
		ClientManager.Instance.clientNetworkedId = NetworkManager.Singleton.LocalClientId;
	}
	public void StopClient()
	{
		GameManager.Instance.isPlayerOne = true;
		GameManager.Instance.isMultiplayerGame = false;
		//hostLobby = null;

		Multiplayer.Instance.UnsubToEvents();
		Multiplayer.Instance.ShutDownNetworkManagerIfActive();

		if (SceneManager.GetActiveScene().buildIndex == 0)
			Multiplayer.Instance.GetLobbiesList();

		else
			GameManager.Instance.gameUIManager.ShowPlayerDisconnectedPanel();
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

		Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
		Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
		Debug.Log($"client: {allocation.AllocationId}");

		return new RelayServerData(allocation, "dtls");
	}

	//handle disconnects
	public void HandlePlayerDisconnectsAsClient(ulong id)
	{
		if (SceneManager.GetActiveScene().buildIndex == 0)
		{

		}
	}
}
