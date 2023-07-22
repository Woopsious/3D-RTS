using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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

	public string mainMenuSceneName = "MainMenu";
	public string mapOneSceneName = "MapOne";

	[Header("player One Stats")]
	public int playerOneCurrentMoney;
	public int playerOneIncomeMoney;
	public int playerOneCurrentAlloys;
	public int playerOneIncomeAlloys;
	public int playerOneCurrentCrystals;
	public int playerOneIncomeCrystals;

	[Header("player Two Stats")]
	public int aiCurrentMoney;
	public int aiIncomeMoney;
	public int aiCurrentAlloys;
	public int aiIncomeAlloys;
	public int aiCurrentCrystals;
	public int aiIncomeCrystals;

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
			health = PlayerOneBuildingsList[6].GetComponent<BuildingManager>().maxHealth,
			armour = PlayerOneBuildingsList[6].GetComponent<BuildingManager>().armour
		};
		buildingEnergyGenStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[0].GetComponent<BuildingManager>().maxHealth,
			armour = PlayerOneBuildingsList[0].GetComponent<BuildingManager>().armour
		};
		buildingRefineryStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[1].GetComponent<BuildingManager>().maxHealth,
			armour = PlayerOneBuildingsList[1].GetComponent<BuildingManager>().armour
		};
		buildingLightVehProdStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[3].GetComponent<BuildingManager>().maxHealth,
			armour = PlayerOneBuildingsList[3].GetComponent<BuildingManager>().armour
		};
		buildingHeavyVehProdStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[4].GetComponent<BuildingManager>().maxHealth,
			armour = PlayerOneBuildingsList[4].GetComponent<BuildingManager>().armour
		};
		buildingVtolVehProdStats = new BaseBuildingStats
		{
			health = PlayerOneBuildingsList[5].GetComponent<BuildingManager>().maxHealth,
			armour = PlayerOneBuildingsList[5].GetComponent<BuildingManager>().armour
		};

		//assign base unit stats
		unitScoutVehStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[0].GetComponent<UnitStateController>().maxHealth,
			armour = PlayerOneUnitsList[0].GetComponent<UnitStateController>().armour,
			speed = PlayerOneUnitsList[0].GetComponent<UnitStateController>().agentNav.speed
		};
		unitRadarVehStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[1].GetComponent<UnitStateController>().maxHealth,
			armour = PlayerOneUnitsList[1].GetComponent<UnitStateController>().armour,
			speed = PlayerOneUnitsList[1].GetComponent<UnitStateController>().agentNav.speed
		};
		unitMechLightStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[2].GetComponent<UnitStateController>().maxHealth,
			armour = PlayerOneUnitsList[2].GetComponent<UnitStateController>().armour,
			mainWeaponDamage = PlayerOneUnitsList[2].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage,
			secondaryWeaponDamage = PlayerOneUnitsList[2].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage,
			attackRange = PlayerOneUnitsList[2].GetComponent<UnitStateController>().attackRange,
			speed = PlayerOneUnitsList[2].GetComponent<UnitStateController>().agentNav.speed
		};
		unitMechHvyKnightStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[3].GetComponent<UnitStateController>().maxHealth,
			armour = PlayerOneUnitsList[3].GetComponent<UnitStateController>().armour,
			mainWeaponDamage = PlayerOneUnitsList[3].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage,
			secondaryWeaponDamage = PlayerOneUnitsList[3].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage,
			attackRange = PlayerOneUnitsList[3].GetComponent<UnitStateController>().attackRange,
			speed = PlayerOneUnitsList[3].GetComponent<UnitStateController>().agentNav.speed
		};
		unitMechHvyTankStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[4].GetComponent<UnitStateController>().maxHealth,
			armour = PlayerOneUnitsList[4].GetComponent<UnitStateController>().armour,
			mainWeaponDamage = PlayerOneUnitsList[4].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage,
			secondaryWeaponDamage = PlayerOneUnitsList[4].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage,
			attackRange = PlayerOneUnitsList[4].GetComponent<UnitStateController>().attackRange,
			speed = PlayerOneUnitsList[4].GetComponent<UnitStateController>().agentNav.speed
		};
		unitVtolGunshipStats = new BaseUnitStats
		{
			health = PlayerOneUnitsList[5].GetComponent<UnitStateController>().maxHealth,
			armour = PlayerOneUnitsList[5].GetComponent<UnitStateController>().armour,
			mainWeaponDamage = PlayerOneUnitsList[5].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage,
			secondaryWeaponDamage = PlayerOneUnitsList[5].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage,
			attackRange = PlayerOneUnitsList[5].GetComponent<UnitStateController>().attackRange,
			speed = PlayerOneUnitsList[5].GetComponent<UnitStateController>().agentNav.speed
		};
		unitTurretStats = new BaseUnitStats
		{
			health = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().maxHealth,
			armour = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().armour,
			mainWeaponDamage = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().weaponSystem.mainWeaponDamage,
			secondaryWeaponDamage = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().weaponSystem.secondaryWeaponDamage,
			attackRange = PlayerOneBuildingsList[2].GetComponent<UnitStateController>().attackRange,
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
			int playerOneMoneyPerSecond = playerOneIncomeMoney / 60;
			int playerOneAlloysPerSecond = playerOneIncomeAlloys / 60;
			int playerOneCrystalsPerSecond = playerOneIncomeCrystals / 60;

			int aiMoneyPerSecond = aiIncomeMoney / 60;
			int aiAlloysPerSecond = aiIncomeAlloys / 60;
			int aiCrystalsPerSecond = aiIncomeCrystals / 60;

			gameUIManager.UpdateIncomeResourcesUI(playerOneMoneyPerSecond, playerOneAlloysPerSecond, playerOneCrystalsPerSecond,
				aiMoneyPerSecond, aiAlloysPerSecond, aiCrystalsPerSecond);

			playerOneIncomeMoney = 0;
			playerOneIncomeAlloys = 0;
			playerOneIncomeCrystals = 0;

			aiIncomeMoney = 0;
			aiIncomeAlloys = 0;
			aiIncomeCrystals = 0;

			timer = 0;
		}
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
			gameUIManager.gameManager = this;
			GameManager.Instance.playerNotifsManager.CheckForPlayerNotifsObj();
			gameUIManager.ResetUi();
			gameUIManager.ResetUnitGroupUI();
			gameUIManager.SetUpUnitShopUi();
			gameUIManager.SetUpBuildingsShopUi();
			gameUIManager.techTreeManager.SetUpTechTrees();
		}
		GameManager.Instance.errorManager.CheckForErrorLogObj();
		AudioManager.Instance.LoadSoundSettings();
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
