using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : NetworkBehaviour
{
	public static MenuUIManager Instance;
	//references
	[Header("MenuObj UI Refs")]
	public GameObject mainMenuPanelObj;
	public GameObject singlePlayerScreenObj;
	public GameObject highScorePanelObj;
	public GameObject settingsPanelObj;
	public GameObject settingsVolumePanelObj;
	public GameObject settingsKeybindsPanelObj;

	[Header("Multiplayer Ui Refs")]
	public GameObject LobbyItemPrefab;
	public GameObject PlayerItemPrefab;
	public GameObject MpLobbiesListPanel;
	public Transform LobbyListParentTransform;
	public GameObject MpLobbyPanel;
	public Transform LobbyScreenParentTransform;
	public GameObject leaveLobbyButtonObj;
	public GameObject closeLobbyButtonObj;
	public GameObject startGameButtonObj;

	public Text joinCodeText;
	public Text joinCodeInputField;
	public Text localPlayerNameText;
	public Text localPlayerIdText;
	public Text localPlayerHostText;
	public Text localNetworkedIdText;

	[Header("keybinds Ui")]
	public GameObject KeybindParentObj;
	public GameObject keybindPanelPrefab;
	public string keybindDictionaryString;

	[Header("EasyMode refs")]
	public Text highscoreEasyOne;
	public Text highscoreEasyTwo;
	public Text highscoreEasyThree;
	public Text highscoreEasyFour;
	public Text highscoreEasyFive;
	public Text highscoreEasyOneInfo;
	public Text highscoreEasyTwoInfo;
	public Text highscoreEasyThreeInfo;
	public Text highscoreEasyFourInfo;
	public Text highscoreEasyFiveInfo;

	public void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}
	public void Start()
	{
		GameManager.Instance.OnSceneLoad(0);
	}
	public void Update()
	{
		if (!IsHost && joinCodeInputField.text != MultiplayerManager.Instance.lobbyJoinCode)
			MultiplayerManager.Instance.lobbyJoinCode = joinCodeInputField.text;

		localPlayerNameText.text = $"Player Name: {MultiplayerManager.Instance.localPlayerName}";
		localPlayerIdText.text = $"Player ID: {MultiplayerManager.Instance.localPlayerId}";
		localPlayerHostText.text = "UNSET";
		localNetworkedIdText.text = $"Player Networked ID: {MultiplayerManager.Instance.localPlayerNetworkedId}";
	}
	public void PlayButtonSound()
	{
		AudioManager.Instance.menuSFX.Play();
	}
	//MAIN MENU BUTTON FUNCTIONS
	public void BackToMainMenuButton()
	{
		ShowMainMenuUi();
	}
	public void ShowHighScoreButton()
	{
		ShowHighScoreUi();
	}
	public void ShowSettingsButton()
	{
		ShowSettingsUi();
	}
	public void ShowSettingsKeybindsButton()
	{
		settingsPanelObj.SetActive(false);
		settingsKeybindsPanelObj.SetActive(true);
		UpdateKeybindButtonDisplay();
	}
	public void ShowSettingsVolumeButton()
	{
		settingsPanelObj.SetActive(false);
		settingsVolumePanelObj.SetActive(true);
	}
	public void BackToSettingsButton()
	{
		ShowSettingsUi();
		GameManager.Instance.SavePlayerData();
	}

	//MAIN MENU UI UPDATES
	public void ShowMainMenuUi()
	{
		GameManager.Instance.isPlayerOne = true;
		mainMenuPanelObj.SetActive(true);
		settingsPanelObj.SetActive(false);
		highScorePanelObj.SetActive(false);
		singlePlayerScreenObj.SetActive(false);
		MpLobbiesListPanel.SetActive(false);
		GameManager.Instance.SavePlayerData();
	}
	public void ShowHighScoreUi()
	{
		mainMenuPanelObj.SetActive(false);
		highScorePanelObj.SetActive(true);
	}
	public void ShowSettingsUi()
	{
		mainMenuPanelObj.SetActive(false);
		settingsPanelObj.SetActive(true);
		settingsKeybindsPanelObj.SetActive(false);
		settingsVolumePanelObj.SetActive(false);
	}
	public void QuitGame()
	{
		GameManager.Instance.SavePlayerData();
		Application.Quit();
	}

	//MP BUTTON FUNCTIONS
	public void PlayMultiplayerButton()
	{
		GameManager.Instance.isPlayerOne = false;
		GameManager.Instance.isMultiplayerGame = true;
		ShowLobbiesListUi();
	}
	public void RefreshLobbiesListButton()
	{
		MultiplayerManager.Instance.GetLobbiesList();
	}
	public void CreateLobbyButton()
	{
		GameManager.Instance.isPlayerOne = true;
		MultiplayerManager.Instance.StartHost();
		ShowLobbyUi();
	}
	public void LeaveLobbyButton()
	{
		MultiplayerManager.Instance.LeaveLobby();
		ShowLobbiesListUi();
		ClearPlayersList();
	}
	public void CloseLobbyButton()
	{
		MultiplayerManager.Instance.CloseLobby();
		ShowLobbiesListUi();
		ClearPlayersList();
	}
	public void StartMultiplayerGameButton()
	{
		if (MultiplayerManager.Instance.hostLobby.Players.Count == MultiplayerManager.Instance.hostLobby.MaxPlayers)
			GameManager.Instance.LoadScene(GameManager.Instance.mapOneSceneName);

		else
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Two players are needed to start the game", 3f);
	}

	//MP UI UPDATES
	public void ShowLobbiesListUi()
	{
		mainMenuPanelObj.SetActive(false);
		MpLobbiesListPanel.SetActive(true);
		MpLobbyPanel.SetActive(false);
		closeLobbyButtonObj.SetActive(false);
		leaveLobbyButtonObj.SetActive(false);
		startGameButtonObj.SetActive(false);
		MultiplayerManager.Instance.GetLobbiesList();
	}
	public void ShowLobbyUi()
	{
		MpLobbiesListPanel.SetActive(false);
		MpLobbyPanel.SetActive(true);
		if (GameManager.Instance.isPlayerOne)
		{
			closeLobbyButtonObj.SetActive(true);
			startGameButtonObj.SetActive(true);
		}
		else
			leaveLobbyButtonObj.SetActive(true);
	}
	//Set up lobby list and Player list
	public void SetUpLobbyListUi(QueryResponse queryResponse)
	{
		ClearLobbiesList();

		foreach (Lobby lobby in queryResponse.Results)
		{
			GameObject obj = Instantiate(LobbyItemPrefab, LobbyListParentTransform);
			obj.GetComponent<LobbyItemManager>().Initialize(lobby);
		}
	}
	public void SyncPlayerListforLobbyUi(Lobby lobby)
	{
		if (NetworkManager.Singleton.ConnectedClientsList.Count < LobbyScreenParentTransform.childCount)
		{
			Transform childTransform = LobbyScreenParentTransform.GetChild(LobbyScreenParentTransform.childCount - 1);
			Destroy(childTransform.gameObject);
		}
		else if (NetworkManager.Singleton.ConnectedClientsList.Count > LobbyScreenParentTransform.childCount)
		{
			Instantiate(PlayerItemPrefab, LobbyScreenParentTransform);
			UpdatePlayerList(lobby);
		}
		else
			UpdatePlayerList(lobby);
	}
	public void UpdatePlayerList(Lobby lobby)
	{
		int index = 0;
		foreach (Transform child in LobbyScreenParentTransform.transform)
		{
			PlayerItemManager playerItem = child.GetComponent<PlayerItemManager>();
			playerItem.Initialize(
				MultiplayerManager.Instance.connectedClientsList[index].clientName.ToString(),
				MultiplayerManager.Instance.connectedClientsList[index].clientId.ToString(),
				MultiplayerManager.Instance.connectedClientsList[index].clientNetworkedId.ToString()
				);

			if (!GameManager.Instance.isPlayerOne && playerItem.kickPlayerButton.activeInHierarchy)
				playerItem.kickPlayerButton.SetActive(false);

			else if (GameManager.Instance.isPlayerOne && !playerItem.kickPlayerButton.activeInHierarchy)
			{
				if (playerItem.playerId == lobby.HostId)
					playerItem.kickPlayerButton.SetActive(false);

				else
					playerItem.kickPlayerButton.SetActive(true);
			}
			index++;
		}
	}
	public void ShowPlayersInLobby()
	{
		if (MultiplayerManager.Instance.hostLobby != null)
		{
			Debug.LogWarning($"lobby join code: {MultiplayerManager.Instance.hostLobby.Data["joinCode"].Value}");
			Debug.LogWarning($"players in hosted lobby: {MultiplayerManager.Instance.hostLobby.Players.Count}");
			foreach (Player player in MultiplayerManager.Instance.hostLobby.Players)
			{
				Debug.LogWarning($"player Id: {player.Id} " +
					$"player name: {player.Data["PlayerName"].Value}, networked Id {player.Data["NetworkedId"].Value}");
			}
		}
	}
	public void ClearLobbiesList()
	{
		foreach (Transform child in LobbyListParentTransform)
			Destroy(child.gameObject);
	}
	public void ClearPlayersList()
	{
		foreach (Transform child in LobbyScreenParentTransform)
			Destroy(child.gameObject);
	}

	//FUNCTIONS TO CHANGE KEYBINDS
	public void SetUpKeybindButtonNames()
	{
		for (int i = 0; i < InputManager.Instance.keyBindDictionary.Count; i++)
		{
			int closureIndex = i;
			GameObject keybindPanelObj = Instantiate(keybindPanelPrefab, KeybindParentObj.transform);
			keybindPanelObj.transform.GetChild(0).GetComponent<Text>().text = "Keybind for: " + InputManager.Instance.keybindNames[closureIndex];

			keybindPanelObj.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
			{ KeyToRebind(InputManager.Instance.keybindNames[closureIndex]); });
			keybindPanelObj.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { KeyToRebindButtonNum(closureIndex); });
			keybindPanelObj.transform.GetChild(1).GetComponentInChildren<Text>().text =
				InputManager.Instance.keyBindDictionary[InputManager.Instance.keybindNames[closureIndex]].ToString();
		}
	}
	public void UpdateKeybindButtonDisplay()
	{
		int i = 0;
		foreach (Transform child in KeybindParentObj.transform)
		{
			Text buttonText = child.GetChild(1).GetComponentInChildren<Text>();
			KeyCode keyCode = InputManager.Instance.keyBindDictionary[InputManager.Instance.keybindNames[i]];
			buttonText.text = InputManager.Instance.keyBindDictionary[InputManager.Instance.keybindNames[i]].ToString();
			i++;
		}
	}
	public void KeyToRebind(string buttonName)
	{
		InputManager.Instance.keyToRebind = buttonName;
	}
	public void KeyToRebindButtonNum(int buttonNum)
	{
		InputManager.Instance.buttonNumToRebind = buttonNum;
		KeybindParentObj.transform.GetChild(buttonNum).GetChild(1).GetComponentInChildren<Text>().text = "Press Key To Rebind";
	}
	public void UpdateKeybindButtonDisplay(int buttonNum, KeyCode key)
	{
		KeybindParentObj.transform.GetChild(buttonNum).GetChild(1).GetComponentInChildren<Text>().text = key.ToString();
	}
	public void CancelKeybindButtonDisplay(int buttonNum, KeyCode key)
	{
		KeybindParentObj.transform.GetChild(buttonNum).GetChild(1).GetComponentInChildren<Text>().text = key.ToString();
	}
	public void KeyBindsSave()
	{
		GameManager.Instance.SavePlayerData();
	}
	public void KeyBindsReset()
	{
		InputManager.Instance.ResetKeybindsToDefault();
		UpdateKeybindButtonDisplay();
	}
}
