using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
	public static GameUIManager Instance;

	public PlayerController playerController;

	public MiniMapManager miniMap;

	[Header("User UI Refs")]
	public GameObject playerUiInfoObj;
	public GameObject settingsObj;
	public GameObject buildingsUiShopObj;
	public GameObject unitUiShopOneObj;
	public GameObject unitUiShopTwoObj;

	public GameObject unitGroupsObj;
	public GameObject unitProdQueuesObj;
	public Button audioBackButton;

	public Text gameTimerText;

	[Header("User Resource Refs")]
	public Text CurrentMoneyText;
	public Text IncomeMoneyText;
	public Text CurrentAlloysText;
	public Text IncomeAlloysText;
	public Text CurrentCrystalsText;
	public Text IncomeCrystalsText;

	[Header("Building Shop Buy Button Refs")]
	public Button buyEnergyGenBuilding;
	public Button buyRefineryBuilding;
	public Button buyLightVehProdBuilding;
	public Button buyHeavyVehProdBuilding;
	public Button buyVTOLVehProdBuilding;

	[Header("Light Veh Shop Buy Button Refs")]
	public Button buyScoutVehicle;
	public Button buyRadarVehicle;
	public Button buyLightMechVehicle;

	[Header("Heavy Veh Shop Buy Button Refs")]
	public Button buyHeavyMechKnightVehicle;
	public Button buyHeavyMechTankVehicle;
	public Button buyVTOLVehicle;
	public Button buyDefenseTurret;

	[Header("Unit Group Ui Refs")]
	public Text groupOneInfoUI;
	public Text groupTwoInfoUI;
	public Text groupThreeInfoUI;
	public Text groupFourtInfoUI;
	public Text groupFiveInfoUI;

	[Header("Game Speed Refs")]
	public Text gameSpeedText;
	public bool isGamePaused;
	public float gameSpeed;

	//may need to delegate on click functions for buy buttons depending on if PlayerController isPlayerOne or !isPlayerOne
	public void Awake()
	{
		GameManager.Instance.gameUIManager = this;
		playerController.gameUIManager = this;
		audioBackButton.onClick.AddListener(delegate { AudioManager.Instance.AdjustAudioVolume(); });
		//WIP soulution for multiplayer
		PlayerController[] playerControllers = FindObjectsOfType<PlayerController>();
	}
	public void Start()
	{
		GameManager.Instance.OnSceneLoad(1);
		GameManager.Instance.LoadPlayerData();
		ResetUi();
	}
	public void Update()
	{
		UpdateInGameTimerUI();
		GameManager.Instance.GetResourcesPerSecond();
		GameManager.Instance.GameClock();
	}
	public void ResetUi()
	{
		gameSpeed = 1;
		UpdateGameSpeedUi();
		UpdateCurrentResourcesUI();
		UpdateIncomeResourcesUI(0, 0, 0, 0, 0, 0);
	}
	public void PlayButtonSound()
	{
		AudioManager.Instance.menuSFX.Play();
	}

	//MAIN GAME MENU FUNCTIONS
	public void ExitGame()
	{
		AudioManager.Instance.menuSFX.Play();
		GameManager.Instance.SavePlayerData();
		StartCoroutine(GameManager.Instance.WaitForSceneLoad(0));
	}
	public void SaveAndExitGame()
	{
		AudioManager.Instance.menuSFX.Play();
		GameManager.Instance.SavePlayerData();
		//save game data function
		StartCoroutine(GameManager.Instance.WaitForSceneLoad(0));
	}
	public void OpenSettings()
	{
		AudioManager.Instance.menuSFX.Play();
		settingsObj.SetActive(true);
		Time.timeScale = 0;
	}
	public void CloseSettings()
	{
		AudioManager.Instance.menuSFX.Play();
		GameManager.Instance.SavePlayerData();
		Time.timeScale = 1;
		settingsObj.SetActive(false);
	}
	//show tab menu functions
	public void ShowBuildingShop()
	{
		buildingsUiShopObj.SetActive(true);
		unitUiShopOneObj.SetActive(false);
		unitUiShopTwoObj.SetActive(false);
	}
	public void ShowUnitShopUnarmed()
	{
		buildingsUiShopObj.SetActive(false);
		unitUiShopOneObj.SetActive(true);
		unitUiShopTwoObj.SetActive(false);
	}
	public void ShowUnitShopArmed()
	{
		buildingsUiShopObj.SetActive(false);
		unitUiShopOneObj.SetActive(false);
		unitUiShopTwoObj.SetActive(true);
	}
	public void ShowGroupedUnits()
	{
		if (unitProdQueuesObj.transform.position == new Vector3(0, 575, 0))
			unitProdQueuesObj.transform.position = new Vector3(-500, 575, 0);

		if (unitGroupsObj.transform.position == new Vector3(0, 575, 0))
			unitGroupsObj.transform.position = new Vector3(-500, 575, 0);
		else if (unitGroupsObj.transform.position != new Vector3(0, 575, 0))
			unitGroupsObj.transform.position = new Vector3(0, 575, 0);
	}
	public void ShowUnitProdQueues()
	{
		if (unitGroupsObj.transform.position == new Vector3(0, 575, 0))
			unitGroupsObj.transform.position = new Vector3(-500, 575, 0);

		if (unitProdQueuesObj.transform.position == new Vector3(0, 575, 0))
			unitProdQueuesObj.transform.position = new Vector3(-500, 575, 0);
		else if (unitProdQueuesObj.transform.position != new Vector3(0, 575, 0))
			unitProdQueuesObj.transform.position = new Vector3(0, 575, 0);
	}
	public void ShowUnitProdQueuesWhenBuyingUnit()
	{
		if (unitGroupsObj.transform.position == new Vector3(0, 575, 0))
			unitGroupsObj.transform.position = new Vector3(-500, 575, 0);
		if (unitProdQueuesObj.transform.position != new Vector3(0, 575, 0))
			unitProdQueuesObj.transform.position = new Vector3(0, 575, 0);
	}

	//UI UPDATES
	public void UpdateInGameTimerUI()
	{
		int seconds = (int)GameManager.Instance.secondsCount;
		gameTimerText.text = GameManager.Instance.hourCount.ToString() + ":" + GameManager.Instance.minuteCount.ToString() + 
			":" + seconds.ToString() + "s";
	}
	public void UpdateGameSpeedUi()
	{
		gameSpeedText.text = "x" + gameSpeed.ToString() + " speed";
	}
	public void UpdateCurrentResourcesUI()
	{
		if (playerController != null)
		{
			CurrentMoneyText.text = GameManager.Instance.playerOneCurrentMoney.ToString();
			CurrentAlloysText.text = GameManager.Instance.playerOneCurrentAlloys.ToString();
			CurrentCrystalsText.text = GameManager.Instance.playerOneCurrentCrystals.ToString();
		}
	}
	public void UpdateIncomeResourcesUI(int playerOneMoneyPerSecond, int playerOneAlloysPerSecond, int playerOneCrystalsPerSecond,
		int aiMoneyPerSecond, int aiAlloysPerSecond, int aiCrystalsPerSecond)
	{
		if(playerController.isPlayerOne)
		{
			IncomeMoneyText.text = playerOneMoneyPerSecond.ToString() + "s";
			IncomeAlloysText.text = playerOneAlloysPerSecond.ToString() + "s";
			IncomeCrystalsText.text = playerOneCrystalsPerSecond.ToString() + "s";
		}
		else if (!playerController.isPlayerOne)
		{
			IncomeMoneyText.text = aiMoneyPerSecond.ToString() + "s";
			IncomeAlloysText.text = aiAlloysPerSecond.ToString() + "s";
			IncomeCrystalsText.text = aiCrystalsPerSecond.ToString() + "s";
		}
	}
	public void UpdateGroupUi(List<UnitStateController> unitGroup, int groupToUpdate)
	{
		int heavyMechCount = 0;
		int lightMechCount = 0;
		int vtolCount = 0;
		int radarVehicleCount = 0;
		int scoutVehicleCount = 0;

		foreach (UnitStateController unit in unitGroup)
		{
			if (unit.moneyCost == 800)
				heavyMechCount++;
			else if (unit.moneyCost == 400)
				lightMechCount++;
			else if (unit.moneyCost == 700)
				vtolCount++;
			else if (unit.moneyCost == 600)
				radarVehicleCount++;
			else if (unit.moneyCost == 200)
				scoutVehicleCount++;
		}

		string info = heavyMechCount.ToString() + "x Heavy Mech\n" + lightMechCount.ToString() + "x Light Mech\n" + vtolCount.ToString() + "x VTOL\n"
			+ radarVehicleCount.ToString() + "x Radar Vehicle\n" + scoutVehicleCount.ToString() + "x Scout Vehicle";

		if (groupToUpdate == 1)
			groupOneInfoUI.text = info;
		else if (groupToUpdate == 2)
			groupTwoInfoUI.text = info;
		else if (groupToUpdate == 3)
			groupThreeInfoUI.text = info;
		else if (groupToUpdate == 4)
			groupFourtInfoUI.text = info;
		else if (groupToUpdate == 5)
			groupFiveInfoUI.text = info;
	}
	public void ResetGroupUI()
	{
		groupOneInfoUI.text = string.Empty;
		groupTwoInfoUI.text = string.Empty;
		groupThreeInfoUI.text = string.Empty;
		groupFourtInfoUI.text = string.Empty;
		groupFiveInfoUI.text = string.Empty;
	}

	//game speed and pause functions
	public void IncreaseGameSpeed()
	{
		gameSpeed *= 2;

		if (gameSpeed > 4)
			gameSpeed = 4;

		ChangeGameSpeed(gameSpeed);
		isGamePaused = false;
	}
	public void DecreaseGameSpeed()
	{
		gameSpeed /= 2f;
		if (gameSpeed < 0.25f)
			gameSpeed = 0.25f;

		ChangeGameSpeed(gameSpeed);
	}
	public void PauseGame()
	{
		if (!isGamePaused)
		{
			ChangeGameSpeed(0);
			isGamePaused = true;
		}
		else if (isGamePaused)
		{
			ChangeGameSpeed(gameSpeed);
			isGamePaused = false;
		}
	}
	public void ChangeGameSpeed(float speed)
	{
		AudioManager.Instance.menuSFX.Play();
		Time.timeScale = speed;
		UpdateGameSpeedUi();
	}
}
