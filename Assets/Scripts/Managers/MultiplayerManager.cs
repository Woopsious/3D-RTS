using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MultiplayerManager : NetworkBehaviour
{
	public static MultiplayerManager Instance;

	private string lobbyName = "[LobbyName]";
	private int maxConnections = 2;
	private string joinCode;

	public Lobby hostLobby;
	public ILobbyEvents lobbyEvents;
	float lobbyTimer = 0;

	public string localPlayerId;
	public string localPlayerName;
	public string localPlayerNetworkedId;

	public void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(Instance);
		}
		else
			Destroy(gameObject);

		localPlayerName = $"Player{UnityEngine.Random.Range(1000, 9999)}";
	}
	public async void Start()
	{
		await AuthenticatePlayer();
	}
	public void Update()
	{
		HandleLobbyPollForUpdates();
	}
	/*
	public async void SubToLobbyEvents(Lobby lobbyToSubTo)
	{
		var callbacks = new LobbyEventCallbacks();
		callbacks.PlayerJoined += PlayerJoinedEvent();
		callbacks.PlayerLeft += PlayerLeftEvent();
		callbacks.KickedFromLobby += PlayerKickedEvent();

		try
		{
			lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobbyToSubTo.Id, callbacks);
		}
		catch (LobbyServiceException ex)
		{
			switch (ex.Reason)
			{
				case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{lobbyToSubTo.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
				case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
				case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
				default: throw;
			}
		}
	}
	public async void UnSubToLobbyEvents(Lobby lobbyToSubTo)
	{
		var callbacks = new LobbyEventCallbacks();
		callbacks.PlayerJoined -= PlayerJoinedEvent();
		callbacks.PlayerLeft -= PlayerLeftEvent();
		callbacks.KickedFromLobby -= PlayerKickedEvent();

		try
		{
			lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobbyToSubTo.Id, callbacks);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError($"failed to unsub from lobby events{e}");
		}
	}
	public Action<List<LobbyPlayerJoined>> PlayerJoinedEvent()
	{
		Debug.LogWarning("player joined");

		return null;
	}
	public Action<List<int>> PlayerLeftEvent()
	{
		Debug.LogWarning("player kicked");
		UnSubToLobbyEvents(hostLobby);

		return null;
	}
	public void PlayerKickedEvent()
	{
		Debug.LogWarning("player kicked");
		UnSubToLobbyEvents(hostLobby);

		this.lobbyEvents = null;
	}
	*/
	//authed on start up
	public async Task AuthenticatePlayer()
	{
		await UnityServices.InitializeAsync();

		AuthenticationService.Instance.SignedIn += () => { Debug.LogWarning($"Player Id: {AuthenticationService.Instance.PlayerId}"); };
		await AuthenticationService.Instance.SignInAnonymouslyAsync();

		localPlayerId = AuthenticationService.Instance.PlayerId;
		Debug.LogWarning($"player Name: {localPlayerName}");
	}
	public async void GetLobbiesList()
	{
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
	}

	//FUNCTIONS FOR HOSTING LOBBY
	//called on creating a lobby from MainMenu
	public void StartHost()
	{
		NetworkManager.Singleton.StartHost();
		localPlayerNetworkedId = NetworkManager.Singleton.LocalClientId.ToString();
		Debug.LogWarning($"player networked Id: {localPlayerNetworkedId}");

		CreateLobby();

		//CreateRelay();
	}
	public async void CreateLobby()
	{
		try
		{
			CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
			{
				IsPrivate = false,
				Player = GetPlayer()
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxConnections, createLobbyOptions);
			//SubToLobbyEvents(hostLobby);

			hostLobby = lobby;
			StartCoroutine(LobbyHeartBeat(5f));

			Debug.LogWarning($"Created lobby with name: {lobby.Name} and Id: {lobby.Id}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}
	public async void CloseLobby()
	{
		try
		{
			HostClosedLobbyServerRPC();

			await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
			Debug.LogWarning($"closed lobby with Id: {hostLobby.Id} and kicked all players");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}

		//UnSubToLobbyEvents(hostLobby);
		hostLobby = null;
	}
	[ServerRpc(RequireOwnership = false)]
	public void HostClosedLobbyServerRPC()
	{
		HostClosedLobbyClientRPC();
	}
	[ClientRpc]
	public void HostClosedLobbyClientRPC()
	{
		MenuUIManager.Instance.ShowLobbiesListUi();
		GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Lobby Closed By Host", 2f);

		hostLobby = null;
	}
	public async void kickPlayerFromLobby(string playerId, string networkedId)
	{
		try
		{
			PlayerKickedFromLobbyServerRPC(networkedId);

			await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, playerId);
			Debug.LogWarning($"player with Id: {playerId} kicked from lobby");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}
	[ServerRpc(RequireOwnership = false)]
	public void PlayerKickedFromLobbyServerRPC(string clientNetworkedId)
	{
		PlayerKickedFromLobbyClientRPC(clientNetworkedId);
	}
	[ClientRpc]
	public void PlayerKickedFromLobbyClientRPC(string clientNetworkedId)
	{
		if (localPlayerNetworkedId == clientNetworkedId)
		{
			MenuUIManager.Instance.ShowLobbiesListUi();
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Kicked From Lobby by Host", 2f);

			hostLobby = null;
		}
	}

	//FUNCTIONS FOR JOINING LOBBY
	//called when joininglobby from lobbylist
	public void StartClient()
	{
		NetworkManager.Singleton.StartClient();
		localPlayerNetworkedId = NetworkManager.Singleton.LocalClientId.ToString();
		Debug.LogWarning($"player networked Id: {localPlayerNetworkedId}");

		//JoinRelay();
	}
	public async void JoinLobby(Lobby lobby)
	{
		StartClient();

		try
		{
			JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
			{
				Player = GetPlayer()
			};

			await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);
			//SubToLobbyEvents(lobby);

			hostLobby = lobby;
			Debug.LogWarning($"joined lobby with Id:{lobby.Id}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}
	public async void LeaveLobby()
	{
		try
		{
			await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, localPlayerId);
			Debug.LogWarning($"player with Id: {localPlayerId} left lobby");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}

		//UnSubToLobbyEvents(hostLobby);
		hostLobby = null;
	}

	//SHARED FUNCTIONS
	private IEnumerator LobbyHeartBeat(float waitTime)
	{
		while (true)
		{
			if (hostLobby != null)
				LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
			yield return new WaitForSeconds(waitTime);
		}
	}
	private async void HandleLobbyPollForUpdates()
	{
		if (hostLobby != null)
		{
			lobbyTimer -= Time.deltaTime;
			if (lobbyTimer < 0)
			{
				lobbyTimer = 1.5f;
				Lobby lobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
				hostLobby = lobby;
				MenuUIManager.Instance.SyncPlayerListforLobbyUi(hostLobby);
			}
		}
	}
	//set up player data when joining/creating lobby
	private Player GetPlayer()
	{
		return new Player
		{
			Data = new Dictionary<string, PlayerDataObject>
				{
					{ "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, localPlayerName) },
					{ "NetworkedId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, localPlayerNetworkedId.ToString())}
				}
		};
	}

	//Unused Relay Functions
	public async void CreateRelay()
	{
		Allocation allocation;
		try
		{
			allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
			joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
		}
		catch (RelayServiceException e)
		{
			Debug.LogError("Failed to allocate relay" + e);
		}
	}
	public async void JoinRelay()
	{
		JoinAllocation joinAllocation;
		try
		{
			Debug.LogWarning($"Joining relay with code: {joinCode}");
			joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

			RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
		}
		catch (RelayServiceException e)
		{
			Debug.LogError($"Failed to join relay with code: {e}");
		}
	}
}
