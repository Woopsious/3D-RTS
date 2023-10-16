using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostManager : NetworkBehaviour
{
	public static HostManager Instance;

	public float kickPlayerFromLobbyOnFailedToConnectTimer = 10f;

	public NetworkList<ClientDataInfo> connectedClientsList;

	public int connectedPlayers;
	public string idOfKickedPlayer;
	public string networkIdOfKickedPlayer;

	public void Awake()
	{
		Instance = this;
	}
	public void StartHost()
	{
		/*
		await CreateNewClientsList();
		try
		{
			connectedClientsList.Clear();
		}
		catch
		{
			//under try catch cause it throws an error, even though without this the list keeps clients from previous sessions
			Debug.Log("ignore");
		}
		*/
		StartCoroutine(RelayConfigureTransportAsHostingPlayer());
		Multiplayer.Instance.SubToEvents();

		GameManager.Instance.isPlayerOne = true;
		GameManager.Instance.isMultiplayerGame = true;
		ClientManager.Instance.clientNetworkedId = NetworkManager.Singleton.LocalClientId;
	}
	public void StopHost()
	{
		GameManager.Instance.isPlayerOne = true;
		GameManager.Instance.isMultiplayerGame = false;

		ClearPlayers();
		LobbyManager.Instance.DeleteLobby();
		Multiplayer.Instance.UnsubToEvents();
		Multiplayer.Instance.ShutDownNetworkManagerIfActive();

		if (SceneManager.GetActiveScene().buildIndex == 0)
			Multiplayer.Instance.GetLobbiesList();
		else
			GameManager.Instance.gameUIManager.ShowPlayerDisconnectedPanel();
	}

	//create relay server
	IEnumerator RelayConfigureTransportAsHostingPlayer()
	{
		var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(2);
		while (!serverRelayUtilityTask.IsCompleted)
		{
			yield return null;
		}
		if (serverRelayUtilityTask.IsFaulted)
		{
			Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
			yield break;
		}

		var relayServerData = serverRelayUtilityTask.Result;
		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
		yield return null;

		NetworkManager.Singleton.StartHost();
		LobbyManager.Instance.CreateLobby();
	}
	public static async Task<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
	{
		Allocation allocation;
		try
		{
			allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
		}
		catch (Exception e)
		{
			Debug.LogError($"Relay create allocation request failed {e.Message}");
			throw;
		}

		Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
		Debug.Log($"server: {allocation.AllocationId}");

		try
		{
			LobbyManager.Instance.lobbyJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		}
		catch
		{
			Debug.LogError("Relay create join code request failed");
			throw;
		}

		return new RelayServerData(allocation, "dtls");
	}


	public void RemoveClientFromRelay(string networkedId)
	{
		networkIdOfKickedPlayer = networkedId;
		ulong networkedIdulong = Convert.ToUInt64(networkedId);
		Debug.LogWarning($"Networked string ID: {networkedId}");
		Debug.LogWarning($"Networked ulong ID: {networkedIdulong}");

		NetworkManager.Singleton.DisconnectClient(networkedIdulong);
	}
	public async void RemoveClientFromLobby(string playerId)
	{
		try
		{
			await LobbyService.Instance.RemovePlayerAsync(LobbyManager.Instance._Lobby.Id, playerId);
			Debug.LogWarning($"player with Id: {playerId} kicked from lobby");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}
	public void KickPlayerFromLobbyIfFailedToConnectToRelay()
	{
		kickPlayerFromLobbyOnFailedToConnectTimer -= Time.deltaTime;
		if (kickPlayerFromLobbyOnFailedToConnectTimer < 0)
		{
			if (LobbyManager.Instance._Lobby.Players.Count != connectedClientsList.Count)
				RemoveClientFromLobby(LobbyManager.Instance._Lobby.Players[1].Id);

			kickPlayerFromLobbyOnFailedToConnectTimer = 11f;
		}
	}

	//handle disconnects
	public void HandlePlayerDisconnectsAsHost(ulong id)
	{
		if (SceneManager.GetActiveScene().buildIndex == 0)
		{
			foreach ( ClientDataInfo clientData in connectedClientsList)
			{
				if (clientData.clientNetworkedId == id)
				{
					RemoveClientFromLobby(clientData.clientId.ToString());
				}
			}
		}
	}
	private void ClearPlayers()
	{
		try
		{
			connectedClientsList.Clear();
		}
		catch
		{
			Debug.LogError("failed to clear connectedClientsList");
		}
	}
}
