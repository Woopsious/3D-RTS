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
	public HostManager hostManager;
	public ClientManager clientManager;

	public string localClientName = "PlayerName";
	public string localClientId;
	public string localClientNetworkedId;

	public NetworkVariable<bool> isHostClosingLobby;
	public NetworkList<ClientData> connectedClientsList;
	public int connectedPlayers;

	public float lobbyUpdatesTimer = 0;

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
	//Auth then get lobbies once Multiplayer is clicked
	public async void StartMultiplayer()
	{
		await AuthenticatePlayer();
		connectedClientsList = new NetworkList<ClientData>();
		GetLobbiesList();
	}
	public async Task AuthenticatePlayer()
	{
		await UnityServices.InitializeAsync();

		AuthenticationService.Instance.SignedIn += () => { Debug.Log($"Player Id: {AuthenticationService.Instance.PlayerId}"); };
		await AuthenticationService.Instance.SignInAnonymouslyAsync();

		localClientId = AuthenticationService.Instance.PlayerId;
		Debug.Log($"player Name: {localClientName}");
	}
	public async void GetLobbiesList()
	{
		MenuUIManager.Instance.ShowFetchingLobbiesListUi();
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
			Debug.Log($"lobbies found: {queryResponse.Results.Count}");

			int index = 1;
			foreach (Lobby lobby in queryResponse.Results)
			{
				Debug.Log($"lobby # {index} lobby Id; {lobby.Id}");
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

	//START-STOP HostManager
	public void StartHost()
	{
		StartCoroutine(hostManager.RelayConfigureTransportAsHostingPlayer());
		SubToEvents();

		GameManager.Instance.isPlayerOne = true;
		GameManager.Instance.isMultiplayerGame = true;
		isHostClosingLobby.Value = false;
	}
	public void StopHost()
	{
		isHostClosingLobby.Value = true;
		GameManager.Instance.isPlayerOne = true;
		GameManager.Instance.isMultiplayerGame = false;
		connectedClientsList.Clear();

		UnsubToEvents();
		ShutDownNetworkManagerIfActive();
		hostManager.DeleteLobby();
		hostManager.hostLobby = null;

		if (SceneManager.GetActiveScene().buildIndex == 0)
			GetLobbiesList();

		else
			GameManager.Instance.gameUIManager.ShowPlayerDisconnectedPanel();
	}

	//START-STOP ClientManager
	public void StartClient(Lobby lobby)
	{
		clientManager.JoinLobby(lobby);
		SubToEvents();

		GameManager.Instance.isPlayerOne = false;
		GameManager.Instance.isMultiplayerGame = true;
	}
	public void StopClient()
	{
		GameManager.Instance.isPlayerOne = true;
		GameManager.Instance.isMultiplayerGame = false;
		UnsubToEvents();
		ShutDownNetworkManagerIfActive();
		clientManager.joinedLobby = null;

		if (SceneManager.GetActiveScene().buildIndex == 0)
			GetLobbiesList();

		else
			GameManager.Instance.gameUIManager.ShowPlayerDisconnectedPanel();
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

	//Client data added to networked list of client data when ever client connects to a relay
	public void PlayerConnectedCallback(ulong id)
	{
		//Debug.LogError($"Player Connected, ID: {id}");
		int i = connectedClientsList.Count;
		Debug.LogError($"number of connected clients: {connectedClientsList.Count}");

		if (CheckIfHost())
		{
			//lobby created after host creates relay, for host save networked id (should always be 0) then grab player data locally
			if (id == 0)
			{
				localClientNetworkedId = NetworkManager.Singleton.LocalClientId.ToString();
				connectedClientsList.Add(new ClientData(localClientName, localClientId, id.ToString()));
			}
			//for connecting clients save networked id (should never be 0) then grab data through lobby before it joins host relay
			else
			{
				Debug.LogError($"number of players in lobby: {hostManager.hostLobby.Players.Count}");
				Debug.LogError($"player index 0 Id: {hostManager.hostLobby.Players[0].Data["PlayerID"].Value}");
				Debug.LogError($"player index 1 Id: {hostManager.hostLobby.Players[1].Data["PlayerID"].Value}");

				Debug.LogError("id != 0");
				localClientNetworkedId = NetworkManager.Singleton.LocalClientId.ToString();
				connectedClientsList.Add(new ClientData(hostManager.hostLobby.Players[i].Data["PlayerName"].Value,
					hostManager.hostLobby.Players[i].Data["PlayerID"].Value, id.ToString()));
			}
		}
		if (!MenuUIManager.Instance.MpLobbyPanel.activeInHierarchy)
			MenuUIManager.Instance.ShowLobbyUi();
	}
	//StopClient will always be called to handle intentional leaving or unintentional disconnects
	public void PlayerDisconnectedCallback(ulong id)
	{
		//Debug.LogError($"Player Disconnected, ID: {id}");

		if (CheckIfHost()) // if host remove client from lobby and relay
			HandleClientDisconnectsWhenHost(id);

		HandleClientDisconnects(id);
	}
	public void HandleClientDisconnectsWhenHost(ulong id)
	{
		for (int i = 0; i < connectedClientsList.Count; i++)
		{
			if (id != 0 && connectedClientsList[i].clientNetworkedId == id.ToString())
			{
				hostManager.RemoveClientFromLobby(connectedClientsList[i].clientId.ToString());

				if (hostManager.idOfKickedPlayer != id.ToString())	//if player left/kicked dont run
					hostManager.RemoveClientFromRelayServerRPC(connectedClientsList[i].clientNetworkedId.ToString());

				connectedClientsList.RemoveAt(i);
				break;
			}
		}
		//only stop host when not in main scene as host can still wait in lobby for new player
		if (SceneManager.GetActiveScene().buildIndex == 1)
		{
			StopHost();
		}
	}
	public void HandleClientDisconnects(ulong id)
	{
		if (localClientNetworkedId != "0" && localClientNetworkedId != id.ToString())
		{
			StopClient();
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Connection to Host Lost", 3f);
		}
	}

	//SHARED FUNCTIONS
	public void CloseOrLeaveGameSession()
	{
		if (CheckIfHost())
			StopHost();

		else
			StopClient();
	}
	public void ShutDownNetworkManagerIfActive()
	{
		if (NetworkManager.Singleton.isActiveAndEnabled)
			NetworkManager.Singleton.Shutdown();
	}
	public async void HandleLobbyPollForUpdates(Lobby lobby)
	{
		if (lobby != null)
		{
			lobbyUpdatesTimer -= Time.deltaTime;
			if (lobbyUpdatesTimer < 0)
			{
				lobbyUpdatesTimer = 1.5f;
				try
				{
					Lobby newlobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
					lobby = newlobby;

					if (lobby.HostId == localClientId)
						Debug.LogWarning($"is lobby host");
					else
						Debug.LogWarning($"is not lobby host");

					if (SceneManager.GetActiveScene().buildIndex == 0)
						MenuUIManager.Instance.SyncPlayerListforLobbyUi(lobby);

					if (CheckIfHost())
						Debug.LogWarning($"connected Networked clients: {NetworkManager.Singleton.ConnectedClientsList.Count}");

					Debug.LogWarning($"connected clients count: {connectedClientsList.Count}");
					Debug.LogWarning($"client in lobby: {lobby.Players.Count}");
					Debug.LogWarning($"Networked ID: {localClientNetworkedId}");
				}
				catch
				{
					Debug.LogError($"Lobby with id: {lobby.Id} no longer exists");
					lobby = null;
				}
			}
		}
	}
	public void ResetPlayerName()
	{
		localClientName = "PlayerName";
		MenuUIManager.Instance.playerNameText.text = $"Player Name: {localClientName}";
	}

	//set up player data when joining/creating lobby
	public Player GetPlayer()
	{
		return new Player
		{
			Data = new Dictionary<string, PlayerDataObject>
				{
					{ "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, localClientName) },
					{ "PlayerID", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, localClientId.ToString())}
				}
		};
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
