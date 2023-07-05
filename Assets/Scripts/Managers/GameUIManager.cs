using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
	public static GameUIManager Instance;

	public PlayerController playerController;

	public MiniMapManager miniMap;

	[Header("User UI Refs")]
	public GameObject playerUiInfoObj;
	public GameObject settingsObj;
	public GameObject buildingsBaseUiShopObj;
	public GameObject buildingsVehicleProdUiShopObj;
	public GameObject unitsLightUiShopOneObj;
	public GameObject unitsHeavyUiShopTwoObj;

	public GameObject unitGroupsObj;
	public GameObject unitProdQueuesObj;
	public Button audioBackButton;

	public Text gameTimerText;

	public GameObject entityInfoTemplatePrefab;

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

	//SHOW UI ELEMENTS
	public void ShowBuildingsBaseShop()
	{
		buildingsBaseUiShopObj.SetActive(true);
		buildingsVehicleProdUiShopObj.SetActive(false);
		unitsLightUiShopOneObj.SetActive(false);
		unitsHeavyUiShopTwoObj.SetActive(false);
	}
	public void ShowBuildingsVehicleProdShop()
	{
		buildingsBaseUiShopObj.SetActive(false);
		buildingsVehicleProdUiShopObj.SetActive(true);
		unitsLightUiShopOneObj.SetActive(false);
		unitsHeavyUiShopTwoObj.SetActive(false);
	}
	public void ShowUnitsLightShop()
	{
		buildingsBaseUiShopObj.SetActive(false);
		buildingsVehicleProdUiShopObj.SetActive(false);
		unitsLightUiShopOneObj.SetActive(true);
		unitsHeavyUiShopTwoObj.SetActive(false);
	}
	public void ShowUnitsHeavyShop()
	{
		buildingsBaseUiShopObj.SetActive(false);
		buildingsVehicleProdUiShopObj.SetActive(false);
		unitsLightUiShopOneObj.SetActive(false);
		unitsHeavyUiShopTwoObj.SetActive(true);
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
	public void ShowGroupedUnitsWhenCreatingGroup()
	{
		if (unitProdQueuesObj.transform.position == new Vector3(0, 575, 0))
			unitProdQueuesObj.transform.position = new Vector3(-500, 575, 0);

		if (unitGroupsObj.transform.position == new Vector3(-500, 575, 0))
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

	//SETUP UI ELEMENTS
	public void SetUpBuildingsShopUi()
	{
		for (int i = 0; i < GameManager.Instance.PlayerOneBuildingsList.Count - 1; i++)
		{
			if (i < 3)
			{
				SetUpEntityTemplate(i, GameManager.Instance.PlayerOneBuildingsList, buildingsBaseUiShopObj);
			}
			else if (i >= 3)
			{
				SetUpEntityTemplate(i, GameManager.Instance.PlayerOneBuildingsList, buildingsVehicleProdUiShopObj);
			}
		}
	}
	public void SetUpUnitShopUi()
	{
		for (int i = 0; i < GameManager.Instance.PlayerOneUnitsList.Count; i++)
		{
			if (i < 3)
			{
				SetUpEntityTemplate(i, GameManager.Instance.PlayerOneUnitsList, unitsLightUiShopOneObj);
			}
			else if (i >= 3)
			{
				SetUpEntityTemplate(i, GameManager.Instance.PlayerOneUnitsList, unitsHeavyUiShopTwoObj);
			}
		}
	}
	public void SetUpEntityTemplate(int i, List<GameObject> entitysList, GameObject parent)
	{
		GameObject go = Instantiate(entityInfoTemplatePrefab, parent.transform);
		Text entityTitleText = go.transform.GetChild(0).GetComponent<Text>();
		Image entityImage = go.transform.GetChild(1).GetComponent<Image>();
		Button entityBuyButton = go.transform.GetChild(2).GetComponent<Button>();
		Text entityInfoText = go.transform.GetChild(3).GetComponent<Text>();

		entityTitleText.text = entitysList[i].GetComponent<Entities>().entityName;
		//entityImage = List of images in GameManager at some point

		GrabUnitInfo(i, entityInfoText);

		LinkBuyButtons(i, entityBuyButton, entitysList);
	}
	public void GrabBuildingInfo(int i, Text textInfo)
	{

	}
	public void GrabUnitInfo(int i, Text textInfo)
	{
		UnitStateController unit = GameManager.Instance.PlayerOneUnitsList[i].GetComponent<UnitStateController>();
		NavMeshAgent unitNavMesh = unit.GetComponent<NavMeshAgent>(); 

		string costInfo = "Cost:\n Money " + unit.moneyCost + "Alloys " + unit.alloyCost + ", Crystals " + unit.crystalCost + "\n";
		string healthInfo = "Stats:\n Health " + unit.maxHealth + ", Armour " + unit.armour + ", View Range " + unit.ViewRange + "\n";
		string specialInfo = "Is Armed: NO, Can Fly: NO, Speed " + unitNavMesh.speed * 5 + "MPH \n";
		if (unit.isFlying)
			specialInfo = "Is Armed: NO, Can Fly: Yes, Speed " + unitNavMesh.speed * 5 + "MPH \n";

		string combatInfo = "";
		if (unit.isUnitArmed)
		{
			WeaponSystem weaponSystem = unit.GetComponent<WeaponSystem>();
			float mainWeaponDPS = weaponSystem.mainWeaponDamage / weaponSystem.mainWeaponAttackSpeed;
			float SecondaryWeaponDPS = weaponSystem.secondaryWeaponDamage / weaponSystem.secondaryWeaponAttackSpeed;
			float DPS = mainWeaponDPS + SecondaryWeaponDPS;

			combatInfo = "Total DPS ignoring armour " + DPS + ", Attack Range " + unit.attackRange;
		}
		textInfo.text = costInfo + healthInfo + specialInfo + combatInfo;
	}

	public void LinkBuyButtons(int i, Button buttonToLink, List<GameObject> listType)
	{
		if (listType == GameManager.Instance.PlayerOneBuildingsList) //|| listType = GameManager.Instance. PLAYERTWOLIST
		{
			switch(i)
			{
				case 0:
				buyEnergyGenBuilding = buttonToLink;
				break;
				case 1:
				buyRefineryBuilding = buttonToLink;
				break;
				case 2:
				buyEnergyGenBuilding = buttonToLink; //needs to be defense turrent in future
				break;
				case 3:
				buyLightVehProdBuilding = buttonToLink;
				break;
				case 4:
				buyHeavyVehProdBuilding = buttonToLink;
				break;
				case 5:
				buyVTOLVehProdBuilding = buttonToLink;
				break;
			}
		}
		else if (listType == GameManager.Instance.PlayerOneUnitsList) //|| listType = GameManager.Instance. PLAYERTWOLIST
		{
			switch (i)
			{
				case 0:
				buyScoutVehicle = buttonToLink;
				break;
				case 1:
				buyRadarVehicle = buttonToLink;
				break;
				case 2:
				buyLightMechVehicle = buttonToLink;
				break;
				case 3:
				buyHeavyMechKnightVehicle = buttonToLink;
				break;
				case 4:
				buyHeavyMechTankVehicle = buttonToLink;
				break;
				case 5:
				buyVTOLVehicle = buttonToLink;
				break;
			}
		}
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
	public void UpdateUnitGroupUi(List<UnitStateController> unitGroup, int groupToUpdate)
	{
		int heavyMechCount = 0;
		int lightMechCount = 0;
		int vtolCount = 0;
		int radarVehicleCount = 0;
		int scoutVehicleCount = 0;

		foreach (UnitStateController unit in unitGroup)
		{
			if (unit.moneyCost == GameManager.Instance.unitScoutVehiclePlayerOne.GetComponent<UnitStateController>().moneyCost)
				scoutVehicleCount++;
			else if (unit.moneyCost == GameManager.Instance.unitRadarVehiclePlayerOne.GetComponent<UnitStateController>().moneyCost)
				radarVehicleCount++;
			else if (unit.moneyCost == GameManager.Instance.unitLightMechPlayerOne.GetComponent<UnitStateController>().moneyCost)
				lightMechCount++;
			else if (unit.moneyCost == GameManager.Instance.unitHeavyMechKnightPlayerOne.GetComponent<UnitStateController>().moneyCost || 
				unit.moneyCost == GameManager.Instance.unitHeavyMechTankPlayerOne.GetComponent<UnitStateController>().moneyCost)
				heavyMechCount++;
			else if (unit.moneyCost == GameManager.Instance.unitVTOLPlayerOne.GetComponent<UnitStateController>().moneyCost)
				vtolCount++;
		}

		string info = lightMechCount + "x Light Mech\n" + heavyMechCount + "x Heavy Mech\n" + vtolCount + "x VTOL\n"
			+ radarVehicleCount + "x Radar Vehicle\n" + scoutVehicleCount + "x Scout Vehicle";

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
	public void ResetUnitGroupUI()
	{
		string info = 0 + "x Heavy Mech\n" + 0 + "x Light Mech\n" + 0 + "x VTOL\n" + 0 + "x Radar Vehicle\n" + 0 + "x Scout Vehicle";

		groupOneInfoUI.text = info;
		groupTwoInfoUI.text = info;
		groupThreeInfoUI.text = info;
		groupFourtInfoUI.text = info;
		groupFiveInfoUI.text = info;
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
