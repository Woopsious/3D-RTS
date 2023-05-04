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
	public GameObject unitScoutVehiclePlayerOne;
	public GameObject unitRadarVehiclePlayerOne;
	public GameObject unitLightMechPlayerOne;
	public GameObject unitHeavyMechKnightPlayerOne;
	public GameObject unitHeavyMechTankPlayerOne;
	public GameObject unitVTOLPlayerOne;

	[Header("PlayerTwo Building Prefabs")]
	public GameObject buildingHQPlayerTwo;
	public GameObject buildingEnergyGenPlayerTwo;
	public GameObject buildingRefineryPlayerTwo;
	public GameObject buildingLightVehProdPlayerTwo;
	public GameObject buildingHeavyVehProdPlayerTwo;
	public GameObject buildingVTOLProdPlayerTwo;
	public GameObject buildingTurretPlayerTwo;

	[Header("PlayerTwo Unit Prefabs")]
	public GameObject unitScoutVehiclePlayerTwo;
	public GameObject unitRadarVehiclePlayerTwo;
	public GameObject unitLightMechPlayerTwo;
	public GameObject unitHeavyMechKnightPlayerTwo;
	public GameObject unitHeavyMechTankPlayerTwo;
	public GameObject unitVTOLPlayerTwo;

	public int testAlloysAmount;
	public int testCrystalsAmount;

	public void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(Instance);
		}
		else
			Destroy(gameObject);
		LocalCopyOfPlayerData = new PlayerData();
		LocalCopyOfGameData = new GameData();

		try
		{
			GameManager.Instance.LoadPlayerData();
		}
		catch
		{
			GameManager.Instance.SavePlayerData();
		}
	}

	public void Update()
	{
		if(SceneManager.GetActiveScene().buildIndex == 1)
		{
			GetResourcesPerSecond();
			GameClock();
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
	public void SavePlayerData()
	{
		//create directory if it doesnt exist
		if (!Directory.Exists("/Saves"))
			Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

		BinaryFormatter formatter = new BinaryFormatter();

		FileStream playerData = File.Create(Application.persistentDataPath + "/Saves/playerData.sav");
		formatter.Serialize(playerData, GameManager.Instance.LocalCopyOfPlayerData);
		playerData.Close();
	}
	public void LoadPlayerData()
	{
		//create directory if it doesnt exist
		if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
			Directory.CreateDirectory(Application.persistentDataPath + "/Saves/playerData.sav");

		BinaryFormatter formatter = new BinaryFormatter();
		FileStream playerData = File.Open(Application.persistentDataPath + "/Saves/playerData.sav", FileMode.Open);

		LocalCopyOfPlayerData = (PlayerData)formatter.Deserialize(playerData);
		playerData.Close();
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
