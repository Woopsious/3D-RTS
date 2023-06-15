using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public GameUIManager gameUIManager;
	public ErrorManager errorManager;

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
	public GameObject buildingHQPlayerOne;
	public GameObject buildingEnergyGenPlayerOne;
	public GameObject buildingRefineryPlayerOne;
	public GameObject buildingLightVehProdPlayerOne;
	public GameObject buildingHeavyVehProdPlayerOne;
	public GameObject buildingVTOLProdPlayerOne;
	public GameObject buildingTurretPlayerOne;

	[Header("PlayerOne Unit Prefabs")]
	public GameObject unitVTOLPlayerOne;
	public GameObject unitHeavyMechTankPlayerOne;
	public GameObject unitHeavyMechKnightPlayerOne;
	public GameObject unitLightMechPlayerOne;
	public GameObject unitRadarVehiclePlayerOne;
	public GameObject unitScoutVehiclePlayerOne;

	[Header("PlayerTwo Building Prefabs")]
	public GameObject buildingHQPlayerTwo;
	public GameObject buildingEnergyGenPlayerTwo;
	public GameObject buildingRefineryPlayerTwo;
	public GameObject buildingLightVehProdPlayerTwo;
	public GameObject buildingHeavyVehProdPlayerTwo;
	public GameObject buildingVTOLProdPlayerTwo;
	public GameObject buildingTurretPlayerTwo;

	[Header("PlayerTwo Unit Prefabs")]
	public GameObject unitVTOLPlayerTwo;
	public GameObject unitHeavyMechTankPlayerTwo;
	public GameObject unitHeavyMechKnightPlayerTwo;
	public GameObject unitLightMechPlayerTwo;
	public GameObject unitRadarVehiclePlayerTwo;
	public GameObject unitScoutVehiclePlayerTwo;

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

		Instance.errorManager.OnStartUpHandleLogFiles();
		Instance.errorManager.CheckForErrorMessageObj();

		InputManager.Instance.SetUpKeybindDictionary();
		MenuUIManager.Instance.SetUpKeybindButtonNames();

		GameManager.Instance.LoadPlayerData();
	}
	public void Update()
	{
		if(SceneManager.GetActiveScene().buildIndex == 1)
		{
			GetResourcesPerSecond();
			GameClock();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTestOneName]))
		{
			Debug.Log(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTestOneName]
				+ " key was Pressed | default key is 1");
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTestTwoName]))
		{
			Debug.Log(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTestTwoName]
				+ " key was Pressed | default key is 2");
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTestThreeName]))
		{
			Debug.Log(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTestThreeName]
				+ " key was Pressed | default key is 3");
		}
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
	public IEnumerator WaitForSceneLoad(int sceneIndex)
	{
		var asyncLoadLevel = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
		while (!asyncLoadLevel.isDone)
		{
			Debug.Log("LoadingNewScene");
			yield return null;
		}
	}
}
