using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;

public class MultiplayerManager : NetworkBehaviour
{
	public static MultiplayerManager Instance;

	public GameObject NetworkManagerPrefab;

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
	//run when player clicks button
	public async void StartMultiplayer()
	{
		await AuthenticatePlayer();
		GetLobbiesList();
	}
	public async Task AuthenticatePlayer()
	{
		await UnityServices.InitializeAsync();

		if (!AuthenticationService.Instance.IsAuthorized)
		{
			AuthenticationService.Instance.SignedIn += () => { Debug.Log($"Player Id: {AuthenticationService.Instance.PlayerId}"); };
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
		}
		ClientManager.Instance.clientId = AuthenticationService.Instance.PlayerId;
	}
	public void UnAuthenticatePlayer()
	{
		AuthenticationService.Instance.SignOut();
	}
	public async void GetLobbiesList()
	{
		MenuUIManager.Instance.FetchLobbiesListUi();
		try
		{
			QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
			{
				Count = 25,
				Filters = new List<QueryFilter> {
					new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "1", QueryFilter.OpOptions.EQ)
				}
			};

			QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
			MenuUIManager.Instance.SetUpLobbyListUi(queryResponse);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
		MenuUIManager.Instance.ShowLobbiesListUi();
	}

	//Functions to handle connected/disconnected call back events
	public void SubToEvents()
	{
		NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnectedCallback;
	}
	public void UnsubToEvents()
	{
		NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnectedCallback;
	}
	public void PlayerConnectedCallback(ulong id)
	{
		if (CheckIfHost())
		{
			if (id == 0) //grab host data locally as lobby is not yet made
			{
				ClientDataInfo data = new ClientDataInfo(ClientManager.Instance.clientUsername,
					ClientManager.Instance.clientId, ClientManager.Instance.clientNetworkedId);

				HostManager.Instance.connectedClientsList.Add(data);
			}
			else //grab other clients data through lobby
			{
				int i = HostManager.Instance.connectedClientsList.Count;

				ClientDataInfo data = new ClientDataInfo(LobbyManager.Instance._Lobby.Players[i].Data["PlayerName"].Value,
					LobbyManager.Instance._Lobby.Players[i].Data["PlayerID"].Value, id);

				HostManager.Instance.connectedClientsList.Add(data);
			}
		}
		if (!MenuUIManager.Instance.MpLobbyPanel.activeInHierarchy) //enable lobby ui once connected to relay
			MenuUIManager.Instance.ShowLobbyUi();
	}
	public void PlayerDisconnectedCallback(ulong id)
	{
		if (CheckIfHost())
			HostManager.Instance.HandlePlayerDisconnectsAsHost(id);
		else
			ClientManager.Instance.HandlePlayerDisconnectsAsClient();
	}
	[ServerRpc(RequireOwnership = false)]
	public void SendClientDataToHostServerRPC(string clientUserName, string clientId, ulong clientNetworkId)
	{
		HostManager.Instance.connectedClientsInfoList.Add(new ClientDataInfo(clientUserName, clientId, clientNetworkId));
	}
	public void ShutDownNetworkManagerIfActive()
	{
		if (NetworkManager.Singleton.isActiveAndEnabled)
			NetworkManager.Singleton.Shutdown();
	}
	//check if host from anywhere
	public bool CheckIfHost()
	{
		if (NetworkManager.Singleton != null)
		{
			if (NetworkManager.Singleton.IsHost)
			{
				//Debug.LogError("CLIENT IS HOST");
				return true;
			}
			else
			{
				//Debug.LogError("CLIENT IS NOT HOST");
				return false;
			}
		}
		return false;
	}

	//SYNC WEATHER FOR PLAYERS
	[ServerRpc(RequireOwnership = false)]
	public void SyncWeatherServerRPC()
	{
		SyncWeatherClientRPC();
	}
	[ClientRpc]
	public void SyncWeatherClientRPC()
	{
		GameManager.Instance.gameUIManager.weatherSystem.ChangeWeather();
	}
}
