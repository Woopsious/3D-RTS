using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MultiplayerManager : NetworkBehaviour
{
	public static MultiplayerManager Instance;

	private string lobbyName = "[LobbyName]";
	private int maxConnections = 2;

	public Lobby hostLobby;
	public string lobbyHostCode;
	public string lobbyJoinCode;
	public ILobbyEvents lobbyEvents;
	float lobbyTimer = 0;

	public string localPlayerId;
	public string localPlayerName;
	public string localPlayerNetworkedId;

	private Allocation HostAllocation;
	private JoinAllocation JoinAllocation;

	public int connectedPlayers;

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

		localPlayerNetworkedId = NetworkManager.Singleton.LocalClientId.ToString();

	}
	public void StartGameTest()
	{
		GameManager.Instance.LoadScene(GameManager.Instance.mapOneSceneName);
	}
	public void StartHostTest()
	{
		StartCoroutine(RelayConfigureTransportAsHostingPlayer());
	}
	public void StartClientTest()
	{
		StartCoroutine(RelayConfigureTransportAsConnectingPlayer());
	}

	//authed on start up
	public async Task AuthenticatePlayer()
	{
		await UnityServices.InitializeAsync();

		AuthenticationService.Instance.SignedIn += () => { Debug.LogWarning($"Player Id: {AuthenticationService.Instance.PlayerId}"); };
		await AuthenticationService.Instance.SignInAnonymouslyAsync();

		localPlayerId = AuthenticationService.Instance.PlayerId;
		Debug.LogWarning($"player Name: {Instance.localPlayerName}");
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
		StartCoroutine(RelayConfigureTransportAsHostingPlayer());

		CreateLobby();
	}
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
		Instance.localPlayerNetworkedId = NetworkManager.Singleton.LocalClientId.ToString();
		Debug.LogWarning($"player networked Id: {Instance.localPlayerNetworkedId}");
		Debug.LogWarning($"lobby Join Code: {Instance.lobbyHostCode}");
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
			Instance.lobbyHostCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			MenuUIManager.Instance.joinCodeText.text = $"JoinCode: {Instance.lobbyHostCode}";
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
			CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
			{
				IsPrivate = false,
				Player = GetPlayer(),
				Data = new Dictionary<string, DataObject>
				{
					{"joinCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, lobbyHostCode)}
				}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxConnections, createLobbyOptions);
			//SubToLobbyEvents(hostLobby);

			hostLobby = lobby;
			StartCoroutine(LobbyHeartBeat(5f));

			Debug.LogWarning($"Created lobby with name: {lobby.Name} and Id: {lobby.Id}");
			Debug.LogWarning($"lobby code: {lobby.Data["joinCode"].Value}");
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
	public void StartClient(Lobby lobby)
	{
		ConnectToLobbyAndRelay(lobby);
	}
	public async void ConnectToLobbyAndRelay(Lobby lobby)
	{
		await JoinLobby(lobby);
		StartCoroutine(RelayConfigureTransportAsConnectingPlayer());
	}
	public async Task JoinLobby(Lobby lobby)
	{
		try
		{
			JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
			{
				Player = GetPlayer()
			};

			await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);
			//SubToLobbyEvents(lobby);

			hostLobby = lobby;
			lobbyJoinCode = hostLobby.Data["joinCode"].Value;
			Debug.LogWarning($"joined lobby with Id:{lobby.Id}");
			Debug.LogWarning($"lobby join code: {lobby.Data["joinCode"].Value}");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}
	}
	IEnumerator RelayConfigureTransportAsConnectingPlayer()
	{
		// Populate RelayJoinCode beforehand through the UI
		var clientRelayUtilityTask = JoinRelayServerFromJoinCode(lobbyJoinCode);

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
		Instance.localPlayerNetworkedId = NetworkManager.Singleton.LocalClientId.ToString();
		Debug.LogWarning($"player networked Id: {Instance.localPlayerNetworkedId}");
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

				Debug.LogWarning($"lobby join code: {hostLobby.Data["joinCode"].Value}");
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
}
