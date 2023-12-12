using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyDataItem : MonoBehaviour
{
	public Lobby lobby;
	public string lobbyId;
	public string lobbyName;

	public Text lobbyNameUi;
	public Text playerCountUi;

	public void Initialize(Lobby lobby)
	{
		this.lobby = lobby;
		lobbyId = lobby.Id;
		lobbyName = lobby.Name;
		lobbyNameUi.text = lobby.Name;
		playerCountUi.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
	}

	public void JoinLobby()
	{
		ClientManager.Instance.StartClient(lobby);
		MenuUIManager.Instance.ShowConnectingToLobbyUi();
	}
}
