using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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

	[Header("player One Stats")]
	public NetworkVariable<int> playerOneCurrentMoney = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneIncomeMoney = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneCurrentAlloys = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneIncomeAlloys = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneCurrentCrystals = new NetworkVariable<int>();
	public NetworkVariable<int> playerOneIncomeCrystals = new NetworkVariable<int>();

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

	[Header("PlayerOne Building Prefabs")]
	public List<GameObject> PlayerOneBuildingsList;

	[Header("PlayerOne Unit Prefabs")]
	public List<GameObject> PlayerOneUnitsList;

	[Header("PlayerTwo Building Prefabs")]
	public List<GameObject> PlayerTwoBuildingsList;

	[Header("PlayerTwo Unit Prefabs")]
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
	public List<BuildingManager> playerBuildingsList = new List<BuildingManager>();
	public List<UnitStateController> playerUnitsList = new List<UnitStateController>();

	public NetworkVariable<bool> playerOneReadyToStart = new NetworkVariable<bool>();
	public NetworkVariable<bool> playerTwoReadyToStart = new NetworkVariable<bool>();
	public bool isMultiplayerGame;

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
		GameManager.Instance.errorManager.CheckForErrorLogObj();
		InputManager.Instance.SetUpKeybindDictionary();

		GameManager.Instance.LoadPlayerData();

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

			ResetIncomeCountServerRPC();
			timer = 0;
		}
	}
	[ServerRpc(RequireOwnership = false)]
	public void ResetIncomeCountServerRPC()
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

	//EDITS TO NETWORKED GAMEOBJECTS 
	[ServerRpc(RequireOwnership = false)]
	public void RefundEntityCostServerRPC(ulong entityNetworkedObjId)
	{
		Entities entity = NetworkManager.SpawnManager.SpawnedObjects[entityNetworkedObjId].GetComponent<Entities>();

		int refundMoney = (int)(entity.moneyCost / 1.5);
		int refundAlloy = (int)(entity.alloyCost / 1.5);
		int refundCrystal = (int)(entity.crystalCost / 1.5);

		if (entity.isPlayerOneEntity)
		{
			GameManager.Instance.playerOneCurrentMoney.Value += refundMoney;
			GameManager.Instance.playerOneCurrentAlloys.Value += refundAlloy;
			GameManager.Instance.playerOneCurrentCrystals.Value += refundCrystal;
		}
		else if (!entity.isPlayerOneEntity)
		{
			GameManager.Instance.playerTwoCurrentMoney.Value += refundMoney;
			GameManager.Instance.playerTwoCurrentAlloys.Value += refundAlloy;
			GameManager.Instance.playerTwoCurrentCrystals.Value += refundCrystal;
		}
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
			//audio is saved when slider value is changed
			InputManager.Instance.SavePlayerKeybinds();

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
			CreatePlayerData();
		else
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream playerData = File.Open(playerDataPath + "/playerData.sav", FileMode.Open);

			LocalCopyOfPlayerData = (PlayerData)formatter.Deserialize(playerData);
			playerData.Close();

			AudioManager.Instance.LoadSoundSettings();
			InputManager.Instance.LoadPlayerKeybinds();
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
		NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
	}
	[ServerRpc(RequireOwnership = false)]
	public void LoadSceneServerRPC(string sceneName)
	{
		LoadSceneClientRPC(sceneName);
	}
	[ClientRpc]
	public void LoadSceneClientRPC(string sceneName)
	{
		NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
	}
	public IEnumerator WaitForSceneLoad(int sceneIndex)
	{
		var asyncLoadLevel = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
		while (!asyncLoadLevel.isDone)
			yield return null;
	}
	public void OnSceneLoad(int sceneIndex)
	{
		if (sceneIndex == 0)
		{
			MenuUIManager.Instance.SetUpKeybindButtonNames();
		}
		else if (sceneIndex == 1)
		{
			gameUIManager = FindObjectOfType<GameUIManager>();
			gameUIManager.gameManager = this;

			gameUIManager.ResetUi();
			gameUIManager.ResetUnitGroupUI();
			gameUIManager.SetUpUnitShopUi();
			gameUIManager.SetUpBuildingsShopUi();
			gameUIManager.techTreeManager.SetUpTechTrees();
		}
		GameManager.Instance.playerNotifsManager.CheckForPlayerNotifsObj();
		GameManager.Instance.errorManager.CheckForErrorLogObj();
		AudioManager.Instance.LoadSoundSettings();

		if(isMultiplayerGame)
		{
			Time.timeScale = 0;
			gameUIManager.isPlayerReadyObj.SetActive(true);
			gameUIManager.HideGameSpeedButtonsForMP();
		}
	}

	//SERVER FUNCTIONS AT RUNTIME
	[ServerRpc(RequireOwnership = false)]
	public void SetPlayerToReadyServerRPC(bool isPlayerOne, ServerRpcParams serverRpcParams = default)
	{
		if (isPlayerOne)
			playerOneReadyToStart.Value = true;
		else if (!isPlayerOne)
			playerTwoReadyToStart.Value = true;
		NotifyOtherPlayerIsReadyClientRPC(serverRpcParams.Receive.SenderClientId);

		if (playerOneReadyToStart.Value == true && playerTwoReadyToStart.Value == true)
			StartGameClientRPC();
	}
	[ClientRpc]
	public void NotifyOtherPlayerIsReadyClientRPC(ulong clientId)
	{
		if (clientId != NetworkManager.Singleton.LocalClientId)
			gameUIManager.isOtherPlayerReadyText.text = "Other Player Ready";
	}
	[ClientRpc]
	public void StartGameClientRPC()
	{
		gameUIManager.isGamePaused = true;
		gameUIManager.PauseGame();
		gameUIManager.isPlayerReadyObj.SetActive(false);
		playerNotifsManager.DisplayNotifisMessage("GAME STARTING", 3f);
	}

	//FUNCTIONS ON ENTITY DEATH
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

	//functions for tech
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
