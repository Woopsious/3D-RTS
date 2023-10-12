using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostManager : NetworkBehaviour
{
	public static HostManager Instance;
	//private Allocation HostAllocation;

	public string lobbyName = "LobbyName";
	private int maxConnections = 2;

	public Lobby hostLobby;
	public string hostLobbyRelayCode;

	public float kickPlayerFromLobbyOnFailedToConnectTimer = 10f;
	public string idOfKickedPlayer;

	public void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}
	public void Update()
	{
		MultiplayerManager.Instance.HandleLobbyPollForUpdates(hostLobby);

		if (hostLobby != null && Instance.hostLobby.Players.Count == 2)
			KickPlayerFromLobbyIfFailedToConnectToRelay();
	}

	public IEnumerator RelayConfigureTransportAsHostingPlayer()
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
		CreateLobby();
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
			Instance.hostLobbyRelayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		}
		catch
		{
			Debug.LogError("Relay create join code request failed");
			throw;
		}

		return new RelayServerData(allocation, "dtls");
	}
	public async void CreateLobby()
	{
		try
		{
			Debug.Log($"code to set as lobby data: {hostLobbyRelayCode}");
			CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
			{
				IsPrivate = false,
				Player = MultiplayerManager.Instance.GetPlayer(),
				IsLocked = false,
				Data = new Dictionary<string, DataObject>
				{
					{"joinCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, hostLobbyRelayCode)}
				}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxConnections, createLobbyOptions);
			hostLobby = lobby;
			StartCoroutine(LobbyHeartBeat(20f));

			Debug.Log($"Created lobby with name: {lobby.Name} and Id: {lobby.Id}");
			Debug.Log($"lobby code: {lobby.Data["joinCode"].Value}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}
	private IEnumerator LobbyHeartBeat(float waitTime)
	{
		while (true)
		{
			if (hostLobby != null)
				LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
			yield return new WaitForSeconds(waitTime);
		}
	}
	public void KickPlayerFromLobbyIfFailedToConnectToRelay()
	{
		kickPlayerFromLobbyOnFailedToConnectTimer -= Time.deltaTime;
		if (kickPlayerFromLobbyOnFailedToConnectTimer < 0)
		{
			if (Instance.hostLobby.Players.Count != MultiplayerManager.Instance.connectedClientsList.Count)
				RemoveClientFromLobby(Instance.hostLobby.Players[1].Id);

			kickPlayerFromLobbyOnFailedToConnectTimer = 11f;
		}
	}
	public async void RemoveClientFromLobby(string playerId)
	{
		try
		{
			await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, playerId);
			Debug.LogWarning($"player with Id: {playerId} kicked from lobby");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void RemoveClientFromRelayServerRPC(string networkedId)
	{
		idOfKickedPlayer = networkedId;
		ulong networkedIdulong = Convert.ToUInt64(networkedId);
		Debug.LogWarning($"Networked string ID: {networkedId}");
		Debug.LogWarning($"Networked ulong ID: {networkedIdulong}");

		NetworkManager.Singleton.DisconnectClient(networkedIdulong);
	}
	public async void DeleteLobby()
	{
		await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
	}
}
