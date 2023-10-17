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

public class MultiplayerManager: NetworkBehaviour
{
	public static MultiplayerManager Instance;

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
			AuthenticationService.Instance.SignedIn += () => { Debug.LogWarning($"Player Id: {AuthenticationService.Instance.PlayerId}"); };
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
		}
		ClientManager.Instance.clientId = AuthenticationService.Instance.PlayerId;
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
			Debug.LogWarning($"lobbies found: {queryResponse.Results.Count}");

			int index = 1;
			foreach (Lobby lobby in queryResponse.Results)
			{
				Debug.LogWarning($"lobby # {index} lobby Id; {lobby.Id}");
				index++;
			}

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
				//HostManager.Instance.connectedClientsList = new NetworkList<ClientDataInfo>();

				ClientDataInfo data = new ClientDataInfo(ClientManager.Instance.clientUsername, 
					ClientManager.Instance.clientId, ClientManager.Instance.clientNetworkedId);

				Debug.LogError(data.clientName);
				Debug.LogError(data.clientId);
				Debug.LogError(data.clientNetworkedId);

				//if (!HostManager.Instance.useNewList)
					HostManager.Instance.connectedClientsList.Add(data);
				//else if (HostManager.Instance.useNewList)
					//HostManager.Instance.connectedClientsListTwo.Add(data);
			}
			else //grab other clients data through lobby
			{
				int i = HostManager.Instance.connectedClientsList.Count;

				ClientDataInfo data = new ClientDataInfo(LobbyManager.Instance._Lobby.Players[i].Data["PlayerName"].Value,
					LobbyManager.Instance._Lobby.Players[i].Data["PlayerID"].Value, id);

				//if (!HostManager.Instance.useNewList)
					HostManager.Instance.connectedClientsList.Add(data);
				//else if (HostManager.Instance.useNewList)
					//HostManager.Instance.connectedClientsListTwo.Add(data);
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
			ClientManager.Instance.HandlePlayerDisconnectsAsClient(id);
	}
	
	public void ShutDownNetworkManagerIfActive()
	{
		if (NetworkManager.Singleton.isActiveAndEnabled)
			NetworkManager.Singleton.Shutdown();
	}

	//check if host from anywhere
	public bool CheckIfHost()
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
