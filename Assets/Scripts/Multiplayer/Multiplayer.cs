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

public class Multiplayer : NetworkBehaviour
{
	public static Multiplayer Instance;

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

	public async void StartMultiplayer()
	{
		HostManager.Instance.connectedClientsList = new NetworkList<ClientDataInfo>();
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

	public void ShutDownNetworkManagerIfActive()
	{
		if (NetworkManager.Singleton.isActiveAndEnabled)
			NetworkManager.Singleton.Shutdown();
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
			int i = HostManager.Instance.connectedClientsList.Count;
			if (id == 0) //grab host data as lobby is not yet made
			{
				ClientDataInfo data = new ClientDataInfo(ClientManager.Instance.clientUsername, ClientManager.Instance.clientId,
					ClientManager.Instance.clientNetworkedId);

				Debug.LogError(data);
				HostManager.Instance.connectedClientsList.Add(data);
			}
			else //grab other clients data through lobby
			{
				HostManager.Instance.connectedClientsList.Add(new ClientDataInfo(
					LobbyManager.Instance._Lobby.Players[i].Data["PlayerName"].Value,
					LobbyManager.Instance._Lobby.Players[i].Data["PlayerID"].Value,
					id));
			}
		}

		if (!MenuUIManager.Instance.MpLobbyPanel.activeInHierarchy)
			MenuUIManager.Instance.ShowLobbyUi();
	}
	public void PlayerDisconnectedCallback(ulong id)
	{
		Debug.LogError(id);

		if (CheckIfHost())
			HostManager.Instance.HandlePlayerDisconnectsAsHost(id);
		else
			ClientManager.Instance.HandlePlayerDisconnectsAsClient(id);
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
