using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
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
using UnityEngine.SceneManagement;
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

	public NetworkList<ClientData> connectedClientsList;

	public string localPlayerName;
	public string localPlayerId;
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
		connectedClientsList = new NetworkList<ClientData>();
	}
	public void Update()
	{
		HandleLobbyPollForUpdates();
	}
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
		Debug.LogError($"Player Connected, ID: {id}");
		Instance.localPlayerNetworkedId = NetworkManager.Singleton.LocalClientId.ToString(); //save network ids once connected to relay
		int i = connectedClientsList.Count;

		if (IsHost)
		{
			//lobby created after host creates relay, for host grab data locally
			if (id == 0)
			{
				connectedClientsList.Add(new ClientData(localPlayerName, localPlayerId, id.ToString()));
			}
			//for connecting clients grab data through lobby before it joins host relay
			else
			{
				connectedClientsList.Add(new ClientData(hostLobby.Players[i].Data["PlayerName"].Value,
					hostLobby.Players[i].Data["PlayerID"].Value, id.ToString()));
			}
		}
	}
	public void PlayerDisconnectedCallback(ulong id)
	{
		Debug.LogError($"Player Disconnected, ID: {id}");

		if (IsHost)
		{
			for (int i = 0; i < connectedClientsList.Count; i++)
			{
				if (connectedClientsList[i].clientNetworkedId == id.ToString())
				{
					connectedClientsList.RemoveAt(i);
					break;
				}
			}
		}

		//by checking if in a lobby and what player left work out if "instanced" client was disconnected or not
		HandlePlayerDisconnectTypes(id);
	}
	public void HandlePlayerDisconnectTypes(ulong id)
	{
		if (localPlayerNetworkedId != "0")
		{
			Debug.LogError($"active scene build index: {SceneManager.GetActiveScene().buildIndex}");

			if (SceneManager.GetActiveScene().buildIndex == 0) //check if in main menu scene
			{
				if (MenuUIManager.Instance.MpLobbyPanel.activeInHierarchy)
				{
					MenuUIManager.Instance.ShowLobbiesListUi();
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Removed From Lobby", 3f);
				}
			}

			hostLobby = null;
			NetworkManager.Singleton.Shutdown();
		}
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
		SubToEvents();
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
			Debug.LogWarning($"code to set as lobby data: {lobbyHostCode}");
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
		//delete lobby and kick playes from lobby
		try
		{
			await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}

		for (int i = connectedClientsList.Count - 1; i > 0; i++)
		{
			kickPlayerFromGame(connectedClientsList[i].clientId.ToString(), connectedClientsList[i].clientNetworkedId.ToString());
		}
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
	public async void kickPlayerFromGame(string playerId, string networkedId)
	{
		await RemoveClientFromLobby(playerId);
		RemoveClientFromRelay(networkedId);
	}
	public async Task RemoveClientFromLobby(string playerId)
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
	public void RemoveClientFromRelay(string networkedId)
	{
		ulong networkedIdulong = Convert.ToUInt64(networkedId);
		Debug.LogWarning($"Networked string ID: {networkedId}");
		Debug.LogWarning($"Networked ulong ID: {networkedIdulong}");

		NetworkManager.Singleton.DisconnectClient(networkedIdulong);
	}

	//FUNCTIONS FOR JOINING LOBBY
	//called when joininglobby from lobbylist
	public void StartClient(Lobby lobby)
	{
		JoinLobby(lobby);
		SubToEvents();
	}
	public async void JoinLobby(Lobby lobby)
	{
		try
		{
			JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
			{
				Player = GetPlayer()
			};
			await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);

			hostLobby = lobby;
			lobbyJoinCode = hostLobby.Data["joinCode"].Value;
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError(e.Message);
		}

		StartCoroutine(RelayConfigureTransportAsConnectingPlayer());
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

				if (IsHost)
					Debug.LogWarning($"connected Networked clients: {NetworkManager.Singleton.ConnectedClientsList.Count}");

				Debug.LogWarning($"connected clients count: {connectedClientsList.Count}");
				Debug.LogWarning($"client in lobby: {hostLobby.Players.Count}");
				Debug.LogWarning($"Networked ID: {localPlayerNetworkedId}");
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
					{ "PlayerID", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, localPlayerId.ToString())}
				}
		};
	}
}

//Custom datatype so it can be used as networked variable
public struct ClientData : INetworkSerializable, IEquatable<ClientData>
{
	public FixedString64Bytes clientName;
	public FixedString64Bytes clientId;
	public FixedString64Bytes clientNetworkedId;

	public ClientData(string playerName = "not set", string clientId = "0", string clientNetworkedId = "not set")
	{
		this.clientName = playerName;
		this.clientId = clientId;
		this.clientNetworkedId = clientNetworkedId;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref clientName);
		serializer.SerializeValue(ref clientId);
		serializer.SerializeValue(ref clientNetworkedId);
	}
	public bool Equals(ClientData other)
	{
		return clientName == other.clientName && clientId == other.clientId && clientNetworkedId == other.clientNetworkedId;
	}
}
