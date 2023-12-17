using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : NetworkBehaviour
{
	public static GameManager Instance;

	public GameUIManager gameUIManager;
	public ErrorLogManager errorManager;
	public PlayerNotifsManager playerNotifsManager;

	//references
	public float timer;
	public float secondsCount;
	public int minuteCount;
	public int hourCount;

	[Header("Data Refs")]
	public PlayerData LocalCopyOfPlayerData;
	public GameData LocalCopyOfGameData;
	[Header("Volume Refs")]
	public float backgroundSliderVolume, menuSFXSliderVolume, gameSFXSliderVolume;

	//file Path Locations
	static string playerDataPath;
	static string playerGameDataPath;

	public string PlayerControllerTag = "PlayerController";
	public bool isPlayerOne;

	public string mainMenuSceneName = "MainMenu";
	public string mapOneSceneName = "MapOne";

	[Header("Default GameSceneStats")]
	public int defaultMoney = 5000000;
	public int defaultAlloys = 50000;
	public int defaultCrystals = 5000;

	[Header("player One Stats")]
	public NetworkVariable<int> playerOneCurrentMoney = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneIncomeMoney = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneCurrentAlloys = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneIncomeAlloys = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneCurrentCrystals = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneIncomeCrystals = new NetworkVariable<int>();

	public int trackPlayerOneMoneyChanges;

	[Header("Player One Tech Bonus")]
	public NetworkVariable<float> playerOneBuildingHealthPercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<float> playerOneBuildingArmourPercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<float> playerOneBuildingBonusToResourceIncome = new NetworkVariable<float>();
	public NetworkVariable<bool> playerOneBuildingHasUnlockedHeavyMechs = new NetworkVariable<bool>();
	public NetworkVariable<bool> playerOneBuildingHasUnlockedVtols = new NetworkVariable<bool>();
	public NetworkVariable<float> playerOneUnitHealthPercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<float> playerOneUnitArmourPercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<float> playerOneUnitDamagePercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<int> playerOneUnitAttackRangeBonus = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneUnitSpeedBonus = new NetworkVariable<int>();

	[Header("player Two Stats")]
	public NetworkVariable<int> playerTwoCurrentMoney = new NetworkVariable<int>();
	public NetworkVariable<int> playerTwoIncomeMoney = new NetworkVariable<int>();
	public NetworkVariable<int> playerTwoCurrentAlloys = new NetworkVariable<int>();
	public NetworkVariable<int> playerTwoIncomeAlloys = new NetworkVariable<int>();
	public NetworkVariable<int> playerTwoCurrentCrystals = new NetworkVariable<int>();
	public NetworkVariable<int> playerTwoIncomeCrystals = new NetworkVariable<int>();

	public int trackPlayerTwoMoneyChanges;

	[Header("Player Two Tech Bonus")]
	public NetworkVariable<float> playerTwoBuildingHealthPercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<float> playerTwoBuildingArmourPercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<float> playerTwoBuildingBonusToResourceIncome = new NetworkVariable<float>();
	public NetworkVariable<bool> playerTwoBuildingHasUnlockedHeavyMechs = new NetworkVariable<bool>();
	public NetworkVariable<bool> playerTwoBuildingHasUnlockedVtols = new NetworkVariable<bool>();
	public NetworkVariable<float> playerTwoUnitHealthPercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<float> playerTwoUnitArmourPercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<float> playerTwoUnitDamagePercentageBonus = new NetworkVariable<float>();
	public NetworkVariable<int> playerTwoUnitAttackRangeBonus = new NetworkVariable<int>();
	public NetworkVariable<int> playerTwoUnitSpeedBonus = new NetworkVariable<int>();

	[Header("Player Entity Prefabs")]
	public List<Sprite> buildingImageList;
	public List<Sprite> unitImageList;
	public List<GameObject> PlayerOneBuildingsList;
	public List<GameObject> PlayerOneUnitsList;
	public List<GameObject> PlayerTwoBuildingsList;
	public List<GameObject> PlayerTwoUnitsList;

	[Header("Base Building Stats")]
	public BaseBuildingStats buildingHQStats;
	public BaseBuildingStats buildingEnergyGenStats;
	public BaseBuildingStats buildingRefineryStats;
	public BaseBuildingStats buildingLightVehProdStats;
	public BaseBuildingStats buildingHeavyVehProdStats;
	public BaseBuildingStats buildingVtolVehProdStats;

	[Header("Base Unit Stats")]
	public BaseUnitStats unitScoutVehStats;
	public BaseUnitStats unitRadarVehStats;
	public BaseUnitStats unitMechLightStats;
	public BaseUnitStats unitMechHvyKnightStats;
	public BaseUnitStats unitMechHvyTankStats;
	public BaseUnitStats unitVtolGunshipStats;
	public BaseUnitStats unitTurretStats;

	public UnitStateController testUnit;

	[Header("MP Refs")]
	public NetworkVariable<bool> hasGameStarted = new NetworkVariable<bool>();
	public NetworkVariable<bool> hasGameEnded = new NetworkVariable<bool>();
	public List<BuildingManager> playerBuildingsList = new List<BuildingManager>();
	public List<UnitStateController> playerUnitsList = new List<UnitStateController>();

	public NetworkVariable<bool> playerOneReadyToStart = new NetworkVariable<bool>();
	public NetworkVariable<bool> playerTwoReadyToStart = new NetworkVariable<bool>();
	public bool isMultiplayerGame;

	public int timeOutCounter;

	public GameObject playerOneHQ;
	public GameObject playerTwoHQ;

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
	public void Start()
	{
		LocalCopyOfPlayerData = new PlayerData();
		LocalCopyOfGameData = new GameData();
		playerDataPath = Application.persistentDataPath;
		playerGameDataPath = Path.Combine(Application.persistentDataPath, "Saves");

		GameManager.Instance.errorManager.OnStartUpHandleLogFiles();

		//assign base building stats
		buildingHQStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[6].GetComponent<BuildingManager>().maxHealth.Value,
			armour = PlayerOneBuildingsList[6].GetComponent<BuildingManager>().armour.Value
		};
		buildingEnergyGenStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[0].GetComponent<BuildingManager>().maxHealth.Value,
			armour = PlayerOneBuildingsList[0].GetComponent<BuildingManager>().armour.Value
		};
		buildingRefineryStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[1].GetComponent<BuildingManager>().maxHealth.Value,
			armour = PlayerOneBuildingsList[1].GetComponent<BuildingManager>().armour.Value
		};
		buildingLightVehProdStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[3].GetComponent<BuildingManager>().maxHealth.Value,
			armour = PlayerOneBuildingsList[3].GetComponent<BuildingManager>().armour.Value
		};
		buildingHeavyVehProdStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[4].GetComponent<BuildingManager>().maxHealth.Value,
			armour = PlayerOneBuildingsList[4].GetComponent<BuildingManager>().armour.Value
		};
		buildingVtolVehProdStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[5].GetComponent<BuildingManager>().maxHealth.Value,
			armour = PlayerOneBuildingsList[5].GetComponent<BuildingManager>().armour.Value
		};

		//assign base unit stats
		unitScoutVehStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[0].GetComponent<UnitStateController>().maxHealth.Value,
			armour = PlayerOneUnitsList[0].GetComponent<UnitStateController>().armour.Value,
			speed = PlayerOneUnitsList[0].GetComponent<UnitStateController>().agentNav.speed
		};
		unitRadarVehStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[1].GetComponent<UnitStateController>().maxHealth.Value,
			armour = PlayerOneUnitsList[1].GetComponent<UnitStateController>().armour.Value,
			speed = PlayerOneUnitsList[1].GetComponent<UnitStateController>().agentNav.speed
		};
		unitMechLightStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[2].GetComponent<UnitStateController>().maxHealth.Value,
			armour = PlayerOneUnitsList[2].GetComponent<UnitStateController>().armour.Value,
			mainWeaponDamage = PlayerOneUnitsList[2].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage.Value,
			secondaryWeaponDamage = PlayerOneUnitsList[2].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage.Value,
			attackRange = PlayerOneUnitsList[2].GetComponent<UnitStateController>().attackRange.Value,
			speed = PlayerOneUnitsList[2].GetComponent<UnitStateController>().agentNav.speed
		};
		unitMechHvyKnightStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[3].GetComponent<UnitStateController>().maxHealth.Value,
			armour = PlayerOneUnitsList[3].GetComponent<UnitStateController>().armour.Value,
			mainWeaponDamage = PlayerOneUnitsList[3].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage.Value,
			secondaryWeaponDamage = PlayerOneUnitsList[3].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage.Value,
			attackRange = PlayerOneUnitsList[3].GetComponent<UnitStateController>().attackRange.Value,
			speed = PlayerOneUnitsList[3].GetComponent<UnitStateController>().agentNav.speed
		};
		unitMechHvyTankStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[4].GetComponent<UnitStateController>().maxHealth.Value,
			armour = PlayerOneUnitsList[4].GetComponent<UnitStateController>().armour.Value,
			mainWeaponDamage = PlayerOneUnitsList[4].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage.Value,
			secondaryWeaponDamage = PlayerOneUnitsList[4].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage.Value,
			attackRange = PlayerOneUnitsList[4].GetComponent<UnitStateController>().attackRange.Value,
			speed = PlayerOneUnitsList[4].GetComponent<UnitStateController>().agentNav.speed
		};
		unitVtolGunshipStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[5].GetComponent<UnitStateController>().maxHealth.Value,
			armour = PlayerOneUnitsList[5].GetComponent<UnitStateController>().armour.Value,
			mainWeaponDamage = PlayerOneUnitsList[5].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage.Value,
			secondaryWeaponDamage = PlayerOneUnitsList[5].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage.Value,
			attackRange = PlayerOneUnitsList[5].GetComponent<UnitStateController>().attackRange.Value,
			speed = PlayerOneUnitsList[5].GetComponent<UnitStateController>().agentNav.speed
		};
		unitTurretStats = new BaseUnitStats
		{
			health = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().maxHealth.Value,
			armour = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().armour.Value,
			mainWeaponDamage = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage.Value,
			secondaryWeaponDamage = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage.Value,
			attackRange = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().attackRange.Value,
			speed = 0
		};

		testUnit = PlayerOneUnitsList[4].GetComponent<UnitStateController>();

		//Debug.Log(testUnit.maxHealth);
		//Debug.Log(testUnit.armour);
		//Debug.Log(testUnit.weaponSystem.mainWeaponDamage);
		//Debug.Log(testUnit.weaponSystem.mainWeaponAttackSpeed);
		//Debug.Log(testUnit.weaponSystem.secondaryWeaponDamage);
		//Debug.Log(testUnit.weaponSystem.secondaryWeaponAttackSpeed);
		//Debug.Log(testUnit.agentNav.speed);
	}

	//track resource income per minuite
	public void GetResourcesPerSecond()
	{
		timer += Time.deltaTime;
		if (timer >= 60)
		{
			int playerOneMoneyPerSecond = playerOneIncomeMoney.Value / 60;
			int playerOneAlloysPerSecond = playerOneIncomeAlloys.Value / 60;
			int playerOneCrystalsPerSecond = playerOneIncomeCrystals.Value / 60;

			int playerTwoMoneyPerSecond = playerTwoIncomeMoney.Value / 60;
			int playerTwoAlloysPerSecond = playerTwoIncomeAlloys.Value / 60;
			int playerTwoCrystalsPerSecond = playerTwoIncomeCrystals.Value / 60;

			gameUIManager.UpdateIncomeResourcesUI(playerOneMoneyPerSecond, playerOneAlloysPerSecond, playerOneCrystalsPerSecond,
				playerTwoMoneyPerSecond, playerTwoAlloysPerSecond, playerTwoCrystalsPerSecond);

			ResetResourceIncomeCountServerRPC();
			timer = 0;
		}
	}
	[ServerRpc(RequireOwnership = false)]
	public void ResetResourceIncomeCountServerRPC()
	{
		playerOneIncomeMoney.Value = 0;
		playerOneIncomeAlloys.Value = 0;
		playerOneIncomeCrystals.Value = 0;

		playerTwoIncomeMoney.Value = 0;
		playerTwoIncomeAlloys.Value = 0;
		playerTwoIncomeCrystals.Value = 0;
	}
	public void GameClock() //game timer in hrs, mins and secs
	{
		if (hasGameStarted.Value == false || hasGameEnded.Value == true) return;

		secondsCount += Time.deltaTime;
		if (secondsCount >= 60)
		{
			minuteCount++;
			secondsCount %= 60;
			if (minuteCount >= 60)
			{
				hourCount++;
				minuteCount %= 60;
			}
		}
	}
	public void ResetGameClock()
	{
		Instance.hourCount = 0;
		Instance.minuteCount = 0;
		Instance.secondsCount = 0;
	}

	//save/load player and game data
	public void CreatePlayerData()
	{
		//create file
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream playerData = File.Create(playerDataPath + "/playerData.sav");

		InputManager.Instance.SavePlayerKeybinds();

		formatter.Serialize(playerData, GameManager.Instance.LocalCopyOfPlayerData);
		playerData.Close();
	}
	public void SavePlayerData()
	{
		if (!File.Exists(playerDataPath + "/playerData.sav"))
			CreatePlayerData();
		else
		{
			Instance.LocalCopyOfPlayerData.PlayerName = ClientManager.Instance.clientUsername;
			//audio is saved when slider value is changed
			InputManager.Instance.SavePlayerKeybinds();
			ResolutionManager.Instance.SaveScreenResolution();

			BinaryFormatter formatter = new BinaryFormatter();
			FileStream playerData = File.Open(playerDataPath + "/playerData.sav", FileMode.Create);

			formatter.Serialize(playerData, GameManager.Instance.LocalCopyOfPlayerData);
			playerData.Close();
		}
	}
	public void LoadPlayerData()
	{
		//on start up load file if it exists
		if (!File.Exists(playerDataPath + "/playerData.sav"))
		{
			CreatePlayerData();
		}
		else
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream playerData = File.Open(playerDataPath + "/playerData.sav", FileMode.Open);

			LocalCopyOfPlayerData = (PlayerData)formatter.Deserialize(playerData);
			playerData.Close();

			ClientManager.Instance.clientUsername = Instance.LocalCopyOfPlayerData.PlayerName;
			AudioManager.Instance.LoadSoundSettings();
			InputManager.Instance.LoadPlayerKeybinds();
			ResolutionManager.Instance.LoadScreenResolution();
		}
	}

	public void SaveGameData(string filePath)
	{
		//create directory if it doesnt exist
		if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
			Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

		//format to binary and create file
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream gameData = File.Create(Application.persistentDataPath + filePath);

		formatter.Serialize(gameData, GameManager.Instance.LocalCopyOfGameData);
		gameData.Close();
	}
	public void LoadGameData(string filePath)
	{
		//create directory if it doesnt exist
		if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
			Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

		//format from binary
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream gameData = File.Open(Application.persistentDataPath + filePath, FileMode.Open);

		LocalCopyOfGameData = (GameData)formatter.Deserialize(gameData);
		gameData.Close();
	}

	//scene changes functions
	public void LoadScene(string sceneName)
	{
		if (isMultiplayerGame)
			NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

		else if (!isMultiplayerGame)
			SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
	}
	public void OnSceneLoad(int sceneIndex)
	{
		GameManager.Instance.LoadPlayerData();
		GameManager.Instance.playerNotifsManager.CheckForPlayerNotifsObj();
		GameManager.Instance.errorManager.CheckForErrorLogObj();

		if (sceneIndex == 0)
			LoadMainMenuScene();
		else if (sceneIndex == 1)
			LoadMapOneScene();
	}
	public async void LoadMainMenuScene()
	{
		Time.timeScale = 1f;
		await InputManager.Instance.CreateKeyBindDictionary();
		await InputManager.Instance.CheckForKeyBindChangesInData();
		MenuUIManager.Instance.SetUpKeybindButtonNames();
		MenuUIManager.Instance.GetPlayerNameUi();
	}
	public void LoadMapOneScene()
	{
		Instance.ResetGameClock();
		Instance.ResetGameSceneValuesServerRPC();
		Instance.ResetResourceIncomeCountServerRPC();

		gameUIManager = FindObjectOfType<GameUIManager>();
		gameUIManager.gameManager = this;

		gameUIManager.ResetUi();
		gameUIManager.ResetUnitGroupUI();
		gameUIManager.SetUpUnitShopUi();
		gameUIManager.SetUpBuildingsShopUi();
		gameUIManager.techTreeManager.SetUpTechTrees();

		if (isMultiplayerGame)
		{
			gameUIManager.exitAndSaveGameButtonObj.SetActive(false);
			gameUIManager.playerReadyUpPanelObj.SetActive(true);
			gameUIManager.HideGameSpeedButtonsForMP();
		}
	}

	//SERVER FUNCTIONS AT GAME START
	[ServerRpc(RequireOwnership = false)]
	public void SetPlayerToReadyServerRPC(bool isPlayerOne)
	{
		if (isPlayerOne)
			playerOneReadyToStart.Value = true;
		else
			playerTwoReadyToStart.Value = true;

		if (playerOneReadyToStart.Value == true || playerTwoReadyToStart.Value == true)
			NotifyOtherPlayerIsReadyClientRPC(isPlayerOne, 1);
		else if (playerOneReadyToStart.Value == true && playerTwoReadyToStart.Value == true)
			NotifyOtherPlayerIsReadyClientRPC(isPlayerOne, 2);

		//start game first time in scene
		if (hasGameEnded.Value == false && playerOneReadyToStart.Value == true && playerTwoReadyToStart.Value == true)
		{
			playerOneReadyToStart.Value = false;
			playerTwoReadyToStart.Value = false;
			hasGameStarted.Value = true;
			StartGameClientRPC();
		}
		//restart game after a game has ended
		else if (hasGameEnded.Value == true && playerOneReadyToStart.Value == true && playerTwoReadyToStart.Value == true)
		{
			playerOneReadyToStart.Value = false;
			playerTwoReadyToStart.Value = false;
			ResetSceneObjectsServerRPC();
			GameManager.Instance.LoadScene(GameManager.Instance.mapOneSceneName);
		}
	}
	[ClientRpc]
	public void NotifyOtherPlayerIsReadyClientRPC(bool isPlayerOne, int num)
	{
		if (hasGameEnded.Value == false)
		{
			if (isPlayerOne)
				gameUIManager.isPlayerOneReadyText.text = "Player One Ready";
			else if (!isPlayerOne)
				gameUIManager.isPlayerTwoReadyText.text = "Player Two Ready";
		}
		else if (hasGameEnded.Value == true)
			gameUIManager.playAgainUiText.text = num + "/2";
	}
	[ClientRpc]
	public void StartGameClientRPC()
	{
		gameUIManager.playerReadyUpPanelObj.SetActive(false);
		playerNotifsManager.DisplayNotifisMessage("GAME STARTING", 3f);

		if (!IsServer) return;

		foreach (CapturePointController capturePoint in gameUIManager.playerController.capturePointsList)
		{
			if (capturePoint.isPlayerOneSpawn)
				GameManager.Instance.SpawnPlayerHQsServerRPC(true,
					capturePoint.playerHQSpawnPoint.transform.position, capturePoint.playerHQSpawnPoint.transform.rotation);

			else if (capturePoint.isPlayerTwoSpawn)
				GameManager.Instance.SpawnPlayerHQsServerRPC(false,
					capturePoint.playerHQSpawnPoint.transform.position, capturePoint.playerHQSpawnPoint.transform.rotation);
		}
	}

	//reset scene
	[ServerRpc]
	public void ResetSceneObjectsServerRPC()
	{
		List<ulong> networkObjIdList = new List<ulong>();

		foreach (NetworkObject Obj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
			networkObjIdList = NetworkManager.Singleton.SpawnManager.SpawnedObjects.Keys.ToList();

		for (int i = NetworkManager.Singleton.SpawnManager.SpawnedObjects.Count - 1; i > 0; i--)
		{
			if (NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjIdList[i]].GetComponent<MultiplayerManager>()) return;
			else
				NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjIdList[i]].Despawn();
		}
	}
	[ServerRpc]
	public void ResetGameSceneValuesServerRPC()
	{
		Instance.hasGameEnded.Value = false;
		Instance.hasGameStarted.Value = false;

		Instance.playerOneCurrentMoney.Value = defaultMoney;
		Instance.playerOneCurrentAlloys.Value = defaultAlloys;
		Instance.playerOneCurrentCrystals.Value = defaultCrystals;

		Instance.playerTwoCurrentMoney.Value = defaultMoney;
		Instance.playerTwoCurrentAlloys.Value = defaultAlloys;
		Instance.playerTwoCurrentCrystals.Value = defaultCrystals;
	}

	//spawn player HQ's
	[ServerRpc]
	public void SpawnPlayerHQsServerRPC(bool spawnPlayerOneHq, Vector3 position, Quaternion rotation)
	{
		if (spawnPlayerOneHq)
		{
			GameObject obj = Instantiate(GameManager.Instance.PlayerOneBuildingsList[6], position, rotation);
			obj.GetComponent<NetworkObject>().SpawnWithOwnership(HostManager.Instance.connectedClientsList[0].clientNetworkedId);
			SpawnPlayerHQsClientRPC(obj.GetComponent<NetworkObject>().NetworkObjectId);
		}
		else
		{
			GameObject obj = Instantiate(GameManager.Instance.PlayerTwoBuildingsList[6], position, rotation);
			obj.GetComponent<NetworkObject>().SpawnWithOwnership(HostManager.Instance.connectedClientsList[1].clientNetworkedId);
			SpawnPlayerHQsClientRPC(obj.GetComponent<NetworkObject>().NetworkObjectId);
		}
	}
	[ClientRpc]
	public void SpawnPlayerHQsClientRPC(ulong networkObjId)
	{
		NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].GetComponent<BuildingManager>().OnBuildingStartUp();
	}

	//SERVER FUNCTIONS WHEN GAME OVER
	[ServerRpc(RequireOwnership = false)]
	public void GameOverPlayerHQDestroyedServerRPC(bool isPlayerOneCall)
	{
		GameManager.Instance.playerOneReadyToStart.Value = false;
		GameManager.Instance.playerTwoReadyToStart.Value = false;
		GameManager.Instance.hasGameEnded.Value = true;
		GameOverPlayerHQDestroyedClientRPC(isPlayerOneCall);
	}
	[ClientRpc]
	public void GameOverPlayerHQDestroyedClientRPC(bool isPlayerOneCall)
	{
		if (isPlayerOneCall && isPlayerOne)
			gameUIManager.gameOverUiText.text = "HQ Destroyed You Lost";
		else if (isPlayerOneCall && !isPlayerOne)
			gameUIManager.gameOverUiText.text = "Enemy HQ Destroyed You Win";

		else if (!isPlayerOneCall && isPlayerOne)
			gameUIManager.gameOverUiText.text = "Enemy HQ Destroyed You Win";
		else if (!isPlayerOneCall && !isPlayerOne)
			gameUIManager.gameOverUiText.text = "HQ Destroyed You Lost";

		gameUIManager.playAgainUiText.text = "0/2";
		gameUIManager.gameOverUiPanel.SetActive(true);
		GameManager.Instance.gameUIManager.playAgainButtonObj.SetActive(true);
	}

	//FUNCTIONS ON ENTITY DEATHS
	[ServerRpc(RequireOwnership = false)]
	public void RemoveEntityServerRPC(ulong networkObjId)
	{
		RemoveEntityUiClientRPC(networkObjId);
		NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].GetComponent<NetworkObject>().Despawn();
	}
	[ClientRpc]
	public void RemoveEntityUiClientRPC(ulong networkObjId)
	{
		GameObject entityObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].gameObject;
		entityObj.GetComponent<Entities>().RemoveEntityRefs();

		Instantiate(entityObj.GetComponent<Entities>().DeathObj, entityObj.transform.position, Quaternion.identity);
		Destroy(entityObj.GetComponent<Entities>().UiObj);
	}

	//SERVER FUNCTIONS FOR TECH
	[ServerRpc(RequireOwnership = false)]
	public void UpdateTechBonusesServerRPC(bool isBuildingTech, int index, ServerRpcParams serverRpcParams = default)
	{
		if (isBuildingTech && serverRpcParams.Receive.SenderClientId == 0)
		{
			if (index == 0)
			{
				playerOneBuildingHealthPercentageBonus.Value += 0.1f;
			}
			if (index == 1)
			{
				playerOneBuildingArmourPercentageBonus.Value += 0.1f;
			}
			if (index == 2)
			{
				playerOneBuildingBonusToResourceIncome.Value += 0.1f;
			}
			if (index == 3)
			{
				playerOneBuildingHasUnlockedHeavyMechs.Value = true;
			}
			if (index == 4)
			{
				playerOneBuildingHasUnlockedVtols.Value = true;
			}
			if (index == 5)
			{
				playerOneBuildingHealthPercentageBonus.Value += 0.15f;
			}
			if (index == 6)
			{
				playerOneBuildingArmourPercentageBonus.Value += 0.15f;
			}
		}
		else if (!isBuildingTech && serverRpcParams.Receive.SenderClientId == 0)
		{
			if (index == 0)
			{
				playerOneUnitHealthPercentageBonus.Value += 0.05f;
			}
			if (index == 1)
			{
				playerOneUnitArmourPercentageBonus.Value += 0.1f;
			}
			if (index == 2)
			{
				playerOneUnitSpeedBonus.Value = 1;
			}
			if (index == 3)
			{
				playerOneUnitHealthPercentageBonus.Value += 0.1f;
			}
			if (index == 4)
			{
				playerOneUnitAttackRangeBonus.Value += 1;
			}
			if (index == 6)
			{
				playerOneUnitAttackRangeBonus.Value += 1;
			}
			if (index == 5)
			{
				playerOneUnitDamagePercentageBonus.Value += 0.05f;
			}
			if (index == 7)
			{
				playerOneUnitDamagePercentageBonus.Value += 0.1f;
			}
		}
		else if (isBuildingTech && serverRpcParams.Receive.SenderClientId == 1)
		{
			if (index == 0)
			{
				playerTwoBuildingHealthPercentageBonus.Value += 0.1f;
			}
			if (index == 1)
			{
				playerTwoBuildingArmourPercentageBonus.Value += 0.1f;
			}
			if (index == 2)
			{
				playerTwoBuildingBonusToResourceIncome.Value += 0.1f;
			}
			if (index == 3)
			{
				playerTwoBuildingHasUnlockedHeavyMechs.Value = true;
			}
			if (index == 4)
			{
				playerTwoBuildingHasUnlockedVtols.Value = true;
			}
			if (index == 5)
			{
				playerTwoBuildingHealthPercentageBonus.Value += 0.15f;
			}
			if (index == 6)
			{
				playerTwoBuildingArmourPercentageBonus.Value += 0.15f;
			}
		}
		else if (!isBuildingTech && serverRpcParams.Receive.SenderClientId == 1)
		{
			if (index == 0)
			{
				playerTwoUnitHealthPercentageBonus.Value += 0.05f;
			}
			if (index == 1)
			{
				playerTwoUnitArmourPercentageBonus.Value += 0.1f;
			}
			if (index == 2)
			{
				playerTwoUnitSpeedBonus.Value = 1;
			}
			if (index == 3)
			{
				playerTwoUnitHealthPercentageBonus.Value += 0.1f;
			}
			if (index == 4)
			{
				playerTwoUnitAttackRangeBonus.Value += 1;
			}
			if (index == 6)
			{
				playerTwoUnitAttackRangeBonus.Value += 1;
			}
			if (index == 5)
			{
				playerTwoUnitDamagePercentageBonus.Value += 0.05f;
			}
			if (index == 7)
			{
				playerTwoUnitDamagePercentageBonus.Value += 0.1f;
			}
		}

		if (isBuildingTech)
			ApplyTechUpgradesToExistingBuildingsServerRPC();
		else if (!isBuildingTech)
			ApplyTechUpgradesToExistingUnitsServerRPC();
	}
	//using list of all units/buildings, reset values to base then recalculate values to correct player entities
	[ServerRpc(RequireOwnership = false)]
	public void ApplyTechUpgradesToExistingBuildingsServerRPC()
	{
		playerBuildingsList = playerBuildingsList.Where(item => item != null).ToList();

		foreach (BuildingManager building in playerBuildingsList)
		{
			if (building.entityName == "Energy Generator")
			{
				if (building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingEnergyGenStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingEnergyGenStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingEnergyGenStats.armour * playerOneBuildingArmourPercentageBonus.Value);
				}
				else if (!building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingEnergyGenStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingEnergyGenStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingEnergyGenStats.armour * playerTwoBuildingArmourPercentageBonus.Value);
				}
			}
			if (building.entityName == "Refinery Building")
			{
				if (building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingRefineryStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingRefineryStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingRefineryStats.armour * playerOneBuildingArmourPercentageBonus.Value);
				}
				else if (!building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingRefineryStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingRefineryStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingRefineryStats.armour * playerTwoBuildingArmourPercentageBonus.Value);
				}
			}
			if (building.entityName == "Light Vehicle Production Building")
			{
				if (building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingLightVehProdStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingLightVehProdStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingLightVehProdStats.armour * playerOneBuildingArmourPercentageBonus.Value);
				}
				else if (!building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingLightVehProdStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingLightVehProdStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingLightVehProdStats.armour * playerTwoBuildingArmourPercentageBonus.Value);
				}
			}
			if (building.entityName == "Heavy Vehicle Production Building")
			{
				if (building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingHeavyVehProdStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingHeavyVehProdStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingHeavyVehProdStats.armour * playerOneBuildingArmourPercentageBonus.Value);
				}
				else if (!building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingHeavyVehProdStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingHeavyVehProdStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingHeavyVehProdStats.armour * playerTwoBuildingArmourPercentageBonus.Value);
				}
			}
			if (building.entityName == "VTOL Production Pad")
			{
				if (building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingVtolVehProdStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingVtolVehProdStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingVtolVehProdStats.armour * playerOneBuildingArmourPercentageBonus.Value);
				}
				else if (!building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingVtolVehProdStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingVtolVehProdStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingVtolVehProdStats.armour * playerTwoBuildingArmourPercentageBonus.Value);
				}
			}
			if (building.entityName == "Player HQ")
			{
				if (building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingHQStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingHQStats.health * playerOneBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingHQStats.armour * playerOneBuildingArmourPercentageBonus.Value);
				}
				else if (!building.isPlayerOneEntity)
				{
					building.maxHealth.Value = (int)(buildingHQStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.currentHealth.Value = (int)(buildingHQStats.health * playerTwoBuildingHealthPercentageBonus.Value);
					building.armour.Value = (int)(buildingHQStats.armour * playerTwoBuildingArmourPercentageBonus.Value);
				}
			}
			building.UpdateHealthBar();
		}
	}
	[ServerRpc(RequireOwnership = false)]
	public void ApplyTechUpgradesToExistingUnitsServerRPC()
	{
		playerUnitsList = playerUnitsList.Where(item => item != null).ToList();

		foreach (UnitStateController unit in playerUnitsList)
		{
			if (unit.entityName == "Scout Vehicle")
			{
				if (unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitScoutVehStats.health * playerOneUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitScoutVehStats.armour * playerOneUnitHealthPercentageBonus.Value);
					unit.agentNav.speed = unitScoutVehStats.speed + playerOneUnitSpeedBonus.Value;
				}
				else if (!unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitScoutVehStats.health * playerTwoUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitScoutVehStats.armour * playerTwoUnitHealthPercentageBonus.Value);
					unit.agentNav.speed = unitScoutVehStats.speed + playerTwoUnitSpeedBonus.Value;
				}
			}
			if (unit.entityName == "Radar Vehicle")
			{
				if (unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitRadarVehStats.health * playerOneUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitRadarVehStats.armour * playerOneUnitHealthPercentageBonus.Value);
					unit.agentNav.speed = unitRadarVehStats.speed + playerOneUnitSpeedBonus.Value;
				}
				else if (!unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitRadarVehStats.health * playerTwoUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitRadarVehStats.armour * playerTwoUnitHealthPercentageBonus.Value);
					unit.agentNav.speed = unitRadarVehStats.speed + playerTwoUnitSpeedBonus.Value;
				}
			}
			if (unit.entityName == "Light Mech")
			{
				if (unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitMechLightStats.health * playerOneUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitMechLightStats.armour * playerOneUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitMechLightStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitMechLightStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitMechLightStats.attackRange + playerOneUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitMechLightStats.speed + playerOneUnitSpeedBonus.Value;
				}
				else if (!unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitMechLightStats.health * playerTwoUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitMechLightStats.armour * playerTwoUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitMechLightStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitMechLightStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitMechLightStats.attackRange + playerTwoUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitMechLightStats.speed + playerTwoUnitSpeedBonus.Value;
				}
			}
			if (unit.entityName == "Heavy Mech Knight")
			{
				if (unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitMechHvyKnightStats.health * playerOneUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitMechHvyKnightStats.armour * playerOneUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitMechHvyKnightStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitMechHvyKnightStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitMechHvyKnightStats.attackRange + playerOneUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitMechHvyKnightStats.speed + playerOneUnitSpeedBonus.Value;
				}
				else if (!unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitMechHvyKnightStats.health * playerTwoUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitMechHvyKnightStats.armour * playerTwoUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitMechHvyKnightStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitMechHvyKnightStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitMechHvyKnightStats.attackRange + playerTwoUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitMechHvyKnightStats.speed + playerTwoUnitSpeedBonus.Value;
				}
			}
			if (unit.entityName == "Heavy Mech Support")
			{
				if (unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitMechHvyTankStats.health * playerOneUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitMechHvyTankStats.armour * playerOneUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitMechHvyTankStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitMechHvyTankStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitMechHvyTankStats.attackRange + playerOneUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitMechHvyTankStats.speed + playerOneUnitSpeedBonus.Value;
				}
				else if (!unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitMechHvyTankStats.health * playerTwoUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitMechHvyTankStats.armour * playerTwoUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitMechHvyTankStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitMechHvyTankStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitMechHvyTankStats.attackRange + playerTwoUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitMechHvyTankStats.speed + playerTwoUnitSpeedBonus.Value;
				}
			}
			if (unit.entityName == "VTOL Gunship")
			{
				if (unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitVtolGunshipStats.health * playerOneUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitVtolGunshipStats.armour * playerOneUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitVtolGunshipStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitVtolGunshipStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitVtolGunshipStats.attackRange + playerOneUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitVtolGunshipStats.speed + playerOneUnitSpeedBonus.Value;
				}
				else if (!unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitVtolGunshipStats.health * playerTwoUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitVtolGunshipStats.armour * playerTwoUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitVtolGunshipStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitVtolGunshipStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitVtolGunshipStats.attackRange + playerTwoUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitVtolGunshipStats.speed + playerTwoUnitSpeedBonus.Value;
				}
			}
			if (unit.entityName == "Turret")
			{
				if (unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitTurretStats.health * playerOneUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitTurretStats.armour * playerOneUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitTurretStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitTurretStats.mainWeaponDamage * playerOneUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitTurretStats.attackRange + playerOneUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitTurretStats.speed + playerOneUnitSpeedBonus.Value;
				}
				else if (!unit.isPlayerOneEntity)
				{
					unit.maxHealth.Value = (int)(unitTurretStats.health * playerTwoUnitHealthPercentageBonus.Value);
					unit.armour.Value = (int)(unitTurretStats.armour * playerTwoUnitHealthPercentageBonus.Value);
					unit.weaponSystem.mainWeaponDamage.Value = (int)(unitTurretStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.weaponSystem.secondaryWeaponDamage.Value = (int)(unitTurretStats.mainWeaponDamage * playerTwoUnitDamagePercentageBonus.Value);
					unit.attackRange.Value = unitTurretStats.attackRange + playerTwoUnitAttackRangeBonus.Value;
					unit.agentNav.speed = unitTurretStats.speed + playerTwoUnitSpeedBonus.Value;
				}
			}
			unit.UpdateHealthBar();
		}
	}

	//SERVER FUNCTIONS FOR RESOURCE CHANGES THEN RELAYING TO PLAYER UI

	[ServerRpc(RequireOwnership = false)]
	public void UpdateResourcesServerRPC(bool isPlayerOneCall, bool isBuying , bool isRefunding, bool isCancellingUnit,bool isMined,
		ulong entityNetworkedObjId, int moneyAmount, int alloyAmount, int crystalAmount)
	{
		if (isBuying)
		{
			PlayerBuyingThing(isPlayerOneCall, moneyAmount, alloyAmount, crystalAmount);
		}
		else if (isRefunding)
		{
			PlayerRefundingThing(isPlayerOneCall, entityNetworkedObjId);
		}
		else if (isCancellingUnit)
		{
			PlayerCancellingUnit(isPlayerOneCall, moneyAmount, alloyAmount, crystalAmount);
		}
		else if (isMined)
		{
			PlayerMinedResources(isPlayerOneCall, moneyAmount, alloyAmount, crystalAmount);
		}
		if (isPlayerOneCall)
			UpdateClientUiClientRPC(isPlayerOneCall, playerOneCurrentMoney.Value);
		else
			UpdateClientUiClientRPC(isPlayerOneCall, playerTwoCurrentMoney.Value);
	}
	public void PlayerBuyingThing(bool isPlayerOneCall, int moneyAmount, int alloyAmount, int crystalAmount)
	{
		if (isPlayerOneCall)
		{
			GameManager.Instance.playerOneCurrentMoney.Value -= moneyAmount;
			GameManager.Instance.playerOneCurrentAlloys.Value -= alloyAmount;
			GameManager.Instance.playerOneCurrentCrystals.Value -= crystalAmount;
		}
		else if (!isPlayerOneCall)
		{
			GameManager.Instance.playerTwoCurrentMoney.Value -= moneyAmount;
			GameManager.Instance.playerTwoCurrentAlloys.Value -= alloyAmount;
			GameManager.Instance.playerTwoCurrentCrystals.Value -= crystalAmount;
		}
	}
	public void PlayerRefundingThing(bool isPlayerOneCall, ulong entityNetworkedObjId)
	{
		Entities entity = NetworkManager.SpawnManager.SpawnedObjects[entityNetworkedObjId].GetComponent<Entities>();
		int refundMoney = (int)(entity.moneyCost / 1.5);
		int refundAlloy = (int)(entity.alloyCost / 1.5);
		int refundCrystal = (int)(entity.crystalCost / 1.5);

		if (isPlayerOneCall)
		{
			GameManager.Instance.playerOneCurrentMoney.Value += refundMoney;
			GameManager.Instance.playerOneCurrentAlloys.Value += refundAlloy;
			GameManager.Instance.playerOneCurrentCrystals.Value += refundCrystal;
		}
		else if (!isPlayerOneCall)
		{
			GameManager.Instance.playerTwoCurrentMoney.Value += refundMoney;
			GameManager.Instance.playerTwoCurrentAlloys.Value += refundAlloy;
			GameManager.Instance.playerTwoCurrentCrystals.Value += refundCrystal;
		}
	}
	public void PlayerCancellingUnit(bool isPlayerOneCall, int refundMoney, int refundAlloy, int refundCrystal)
	{
		if (isPlayerOneCall)
		{
			GameManager.Instance.playerOneCurrentMoney.Value += refundMoney;
			GameManager.Instance.playerOneCurrentAlloys.Value += refundAlloy;
			GameManager.Instance.playerOneCurrentCrystals.Value += refundCrystal;
		}
		else if (!isPlayerOneCall)
		{
			GameManager.Instance.playerTwoCurrentMoney.Value += refundMoney;
			GameManager.Instance.playerTwoCurrentAlloys.Value += refundAlloy;
			GameManager.Instance.playerTwoCurrentCrystals.Value += refundCrystal;
		}
	}
	public void PlayerMinedResources(bool isPlayerOneCall, int moneyToAdd, int alloysToAdd, int crystalsToAdd)
	{
		if (isPlayerOneCall)
		{
			GameManager.Instance.playerOneCurrentMoney.Value += moneyToAdd;
			GameManager.Instance.playerOneCurrentAlloys.Value += alloysToAdd;
			GameManager.Instance.playerOneCurrentCrystals.Value += crystalsToAdd;

			GameManager.Instance.playerOneIncomeMoney.Value += moneyToAdd;
			GameManager.Instance.playerOneIncomeAlloys.Value += alloysToAdd;
			GameManager.Instance.playerOneIncomeCrystals.Value += crystalsToAdd;
		}
		else if (!isPlayerOneCall)
		{
			GameManager.Instance.playerTwoCurrentMoney.Value += moneyToAdd;
			GameManager.Instance.playerTwoCurrentAlloys.Value += alloysToAdd;
			GameManager.Instance.playerTwoCurrentCrystals.Value += crystalsToAdd;

			GameManager.Instance.playerTwoIncomeMoney.Value += moneyToAdd;
			GameManager.Instance.playerTwoIncomeAlloys.Value += alloysToAdd;
			GameManager.Instance.playerTwoIncomeCrystals.Value += crystalsToAdd;
		}
	}
	[ClientRpc]
	public void UpdateClientUiClientRPC(bool isPlayerOneCall, int oldMoneyValue)
	{
		StartCoroutine(CheckForResourceValueChanges(isPlayerOneCall, oldMoneyValue));
	}
	public IEnumerator CheckForResourceValueChanges(bool isPlayerOneCall, int oldMoneyValue)
	{
		if (isPlayerOne != isPlayerOneCall)
			yield return new WaitForSeconds(0);

		else
		{
			if (gameUIManager.CheckResourceCountMatches() && timeOutCounter < 5) //whilst they do match loop till they dont
			{
				timeOutCounter++;
				yield return new WaitForSeconds(0.25f);
				StartCoroutine(CheckForResourceValueChanges(isPlayerOneCall, oldMoneyValue));
			}
			else if (!gameUIManager.CheckResourceCountMatches())
			{
				timeOutCounter = 0;
				gameUIManager.UpdateCurrentResourcesUI();
			}
			else
				timeOutCounter = 0;
		}
	}

	[System.Serializable]
	public class BaseBuildingStats
	{
		public float health;
		public float armour;
	}
	[System.Serializable]
	public class BaseUnitStats
	{
		public float health;
		public float armour;
		public float mainWeaponDamage;
		public float secondaryWeaponDamage;
		public int attackRange;
		public float speed;
	}
}
