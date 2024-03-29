using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;
using static UnityEngine.UI.CanvasScaler;

public class GameUIManager : MonoBehaviour
{
	public GameManager gameManager;
	public PlayerController playerController;
	public TechTreeManager techTreeManager;
	public WeatherSystem weatherSystem;

	public MiniMapManager miniMap;

	[Header("User UI Refs")]
	public GameObject playerUiInfoObj;
	public GameObject settingsObj;
	public GameObject buildingsBaseUiShopObj;
	public GameObject buildingsVehicleProdUiShopObj;
	public GameObject unitsLightUiShopOneObj;
	public GameObject unitsHeavyUiShopTwoObj;
	public GameObject techTreeParentObj;
	public GameObject unitGroupsParentObj;
	public GameObject unitProdQueuesParentObj;

	public Button audioBackButton;

	public GameObject entityInfoTemplatePrefab;
	public Text gameTimerText;

	[Header("Buttons With changable Keybinds")]
	public Text techTreeButtonText;
	public Text unitGroupsButtonText;
	public Text unitProductionButtonText;

	public Text baseBuildingsButtonText;
	public Text vehProdBuildingsButtonText;
	public Text lightVehiclesButtonText;
	public Text heavyVehiclesButtonText;

	[Header("User ReadyUp Refs")]
	public GameObject playerReadyUpPanelObj;
	public Text isPlayerOneReadyText;
	public Text isPlayerTwoReadyText;

	[Header("User Disconnect/Leave Refs")]
	public GameObject playerDisconnectedUiPanel;
	public GameObject exitAndSaveGameButtonObj;

	[Header("User GameOver Refs")]
	public GameObject gameOverUiPanel;
	public Text gameOverUiText;
	public GameObject playAgainButtonObj;
	public Text playAgainUiText;

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
	public Button buyDefenseTurret;
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

	[Header("Unit Group Ui Refs")]
	public Text groupOneInfoUI;
	public Text groupTwoInfoUI;
	public Text groupThreeInfoUI;
	public Text groupFourInfoUI;
	public Text groupFiveInfoUI;

	[Header("Game Speed Refs")]
	public GameObject gameSpeedIncreaseObj;
	public GameObject gameSpeedDecreaseObj;
	public GameObject gameSpeedPauseObj;
	public Text gameSpeedText;
	public bool isGamePaused;
	public float gameSpeed;

	public void Start()
	{
		GameManager.Instance.OnSceneLoad(1);
		GameManager.Instance.LoadPlayerData();

		audioBackButton.onClick.AddListener(delegate { AudioManager.Instance.AdjustAudioVolumes(); });
		techTreeManager.currentResearchInfoText.text = "No Tech Currently Researching";
	}
	public void Update()
	{
		UpdateInGameTimerUI();
		GameManager.Instance.GetResourcesPerSecond();
		GameManager.Instance.GameClock();

		if(techTreeManager.isCurrentlyReseaching)
			UpdateTechUiComplete();
	}
	public void PlayButtonSound()
	{
		AudioManager.Instance.menuSFX.Play();
	}

	//MAIN GAME MENU FUNCTIONS
	public void ExitGame()
	{
		if (MultiplayerManager.Instance.CheckIfHost())
			HostManager.Instance.StopHost();
		else
			ClientManager.Instance.StopClient();

		GameManager.Instance.SavePlayerData();
		GameManager.Instance.LoadScene(GameManager.Instance.mainMenuSceneName);
	}
	public void SaveAndExitGame()
	{
		GameManager.Instance.SavePlayerData();
		//save game data function
		GameManager.Instance.LoadScene(GameManager.Instance.mainMenuSceneName);
	}
	public void OpenSettings()
	{
		AudioManager.Instance.menuSFX.Play();
		settingsObj.SetActive(true);
		if (!gameManager.isMultiplayerGame)
			Time.timeScale = 0;
	}
	public void CloseSettings()
	{
		AudioManager.Instance.AdjustAudioVolumes();
		AudioManager.Instance.menuSFX.Play();
		GameManager.Instance.SavePlayerData();
		settingsObj.SetActive(false);
		if (!gameManager.isMultiplayerGame)
			Time.timeScale = 1;
	}
	public void ShowPlayerDisconnectedPanel()
	{
		playerDisconnectedUiPanel.SetActive(true);
		PauseGame();
	}
	public void ReturnToMainMenuAfterPlayerDisconnect()
	{
		GameManager.Instance.LoadScene(GameManager.Instance.mainMenuSceneName);
	}
	public void HideGameSpeedButtonsForMP()
	{
		gameSpeedIncreaseObj.SetActive(false);
		gameSpeedDecreaseObj.SetActive(false);
		gameSpeedPauseObj.SetActive(false);
	}
	public void ResetUi()
	{
		UpdateGameSpeedUi();
		UpdateCurrentResourcesUI();
		UpdateIncomeResourcesUI(0, 0, 0, 0, 0, 0);
		UpdateUiKeyBindsDisplay();
	}
	public void RefundSelectedUnitsButton()
	{
		playerController.unitSelectionManager.RefundSelectedUnits();
	}

	//Unit Group Functions
	public void SelectGroupButton(int groupIndex)
	{
		if (groupIndex == 1)
			playerController.unitSelectionManager.SelectUnitsFromGroup(playerController.unitSelectionManager.unitGroupOne);
		if (groupIndex == 2)
			playerController.unitSelectionManager.SelectUnitsFromGroup(playerController.unitSelectionManager.unitGroupTwo);
		if (groupIndex == 3)
			playerController.unitSelectionManager.SelectUnitsFromGroup(playerController.unitSelectionManager.unitGroupThree);
		if (groupIndex == 4)
			playerController.unitSelectionManager.SelectUnitsFromGroup(playerController.unitSelectionManager.unitGroupFour);
		if (groupIndex == 5)
			playerController.unitSelectionManager.SelectUnitsFromGroup(playerController.unitSelectionManager.unitGroupFive);
	}
	public void RemoveUnitsFromGroupButton(int groupIndex)
	{
		playerController.unitSelectionManager.RemoveUnitsFromGroup(groupIndex);
	}
	public void AddUnitsToGroupButton(int groupIndex)
	{
		if (groupIndex == 1)
			playerController.unitSelectionManager.AddSelectedUnitsToNewGroup(playerController.unitSelectionManager.unitGroupOne, 1);
		if (groupIndex == 2)
			playerController.unitSelectionManager.AddSelectedUnitsToNewGroup(playerController.unitSelectionManager.unitGroupTwo, 2);
		if (groupIndex == 3)
			playerController.unitSelectionManager.AddSelectedUnitsToNewGroup(playerController.unitSelectionManager.unitGroupThree, 3);
		if (groupIndex == 4)
			playerController.unitSelectionManager.AddSelectedUnitsToNewGroup(playerController.unitSelectionManager.unitGroupFour, 4);
		if (groupIndex == 5)
			playerController.unitSelectionManager.AddSelectedUnitsToNewGroup(playerController.unitSelectionManager.unitGroupFive, 5);
	}
	public void RemoveSelectedUnitsFromAllGroupsButton()
	{
		playerController.unitSelectionManager.RemoveSelectedUnitsFromAllGroups();
	}

	//MP Button functions
	public void SetPlayerReady()
	{
		gameManager.SetPlayerToReadyServerRPC(playerController.isPlayerOne);
	}
	public void DebugEndGameButton()
	{
		GameManager.Instance.GameOverPlayerHQDestroyedServerRPC(playerController.isPlayerOne);
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
	public void ShowTechTree()
	{
		if (techTreeParentObj.activeInHierarchy)
			techTreeParentObj.SetActive(false);
		else if (!techTreeParentObj.activeInHierarchy)
			techTreeParentObj.SetActive(true);

		if (unitProdQueuesParentObj.transform.position == new Vector3(0, 575, 0))
			unitProdQueuesParentObj.transform.position = new Vector3(-500, 575, 0);

		if (unitGroupsParentObj.transform.position == new Vector3(0, 575, 0))
			unitGroupsParentObj.transform.position = new Vector3(-500, 575, 0);
	}
	public void ShowGroupedUnits()
	{
		if (techTreeParentObj.activeInHierarchy)
			techTreeParentObj.SetActive(false);

		if (unitProdQueuesParentObj.transform.position == new Vector3(0, 575, 0))
			unitProdQueuesParentObj.transform.position = new Vector3(-500, 575, 0);

		if (unitGroupsParentObj.transform.position == new Vector3(0, 575, 0))
			unitGroupsParentObj.transform.position = new Vector3(-500, 575, 0);
		else if (unitGroupsParentObj.transform.position != new Vector3(0, 575, 0))
			unitGroupsParentObj.transform.position = new Vector3(0, 575, 0);
	}
	public void ShowGroupedUnitsWhenCreatingGroup()
	{
		if (techTreeParentObj.activeInHierarchy)
			techTreeParentObj.SetActive(false);

		if (unitProdQueuesParentObj.transform.position == new Vector3(0, 575, 0))
			unitProdQueuesParentObj.transform.position = new Vector3(-500, 575, 0);

		if (unitGroupsParentObj.transform.position == new Vector3(-500, 575, 0))
			unitGroupsParentObj.transform.position = new Vector3(0, 575, 0);
	}
	public void ShowUnitProdQueues()
	{
		if (techTreeParentObj.activeInHierarchy)
			techTreeParentObj.SetActive(false);

		if (unitGroupsParentObj.transform.position == new Vector3(0, 575, 0))
			unitGroupsParentObj.transform.position = new Vector3(-500, 575, 0);

		if (unitProdQueuesParentObj.transform.position == new Vector3(0, 575, 0))
			unitProdQueuesParentObj.transform.position = new Vector3(-500, 575, 0);
		else if (unitProdQueuesParentObj.transform.position != new Vector3(0, 575, 0))
			unitProdQueuesParentObj.transform.position = new Vector3(0, 575, 0);
	}
	public void ShowUnitProdQueuesWhenBuyingUnit()
	{
		if (techTreeParentObj.activeInHierarchy)
			techTreeParentObj.SetActive(false);

		if (unitGroupsParentObj.transform.position == new Vector3(0, 575, 0))
			unitGroupsParentObj.transform.position = new Vector3(-500, 575, 0);

		if (unitProdQueuesParentObj.transform.position != new Vector3(0, 575, 0))
			unitProdQueuesParentObj.transform.position = new Vector3(0, 575, 0);
	}

	//SETUP UI SHOP ELEMENTS
	public void SetUpBuildingsShopUi()
	{
		for (int i = 0; i < GameManager.Instance.PlayerOneBuildingsList.Count; i++)
		{
			if (i == 6) return;

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
			if (i == 6) return;

			if (i < 3)
				SetUpEntityTemplate(i, GameManager.Instance.PlayerOneUnitsList, unitsLightUiShopOneObj);
			else if (i >= 3)
				SetUpEntityTemplate(i, GameManager.Instance.PlayerOneUnitsList, unitsHeavyUiShopTwoObj);
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
		ToolTips toolTip = go.GetComponent<ToolTips>();

		if (entitysList == GameManager.Instance.PlayerOneBuildingsList)
			GrabBuildingInfo(i, entityInfoText, entityImage, toolTip);
		else if (entitysList == GameManager.Instance.PlayerOneUnitsList)
			GrabUnitInfo(i, entityInfoText, entityImage, toolTip);

		LinkBuyButtons(i, entityBuyButton, entitysList);
	}
	public void GrabBuildingInfo(int i, Text textInfo, Image buildingImage, ToolTips toolTip)
	{
		buildingImage.sprite = gameManager.buildingImageList[i];

		if (i == 2)
		{
			TurretController building = GameManager.Instance.PlayerOneBuildingsList[i].GetComponent<TurretController>();

			string costInfo = "Cost:\n Money: " + building.moneyCost + ", Alloys: " + building.alloyCost + ", Crystals: " + building.crystalCost + "\n";
			string healthInfo = "Stats:\n Health: " + building.maxHealth.Value + ", Armour: " + building.armour.Value + "\n";

			WeaponSystem weaponSystem = building.GetComponent<WeaponSystem>();
			float mainWeaponDPS = weaponSystem.mainWeaponDamage.Value / weaponSystem.mainWeaponAttackSpeed;
			float SecondaryWeaponDPS = weaponSystem.secondaryWeaponDamage.Value / weaponSystem.secondaryWeaponAttackSpeed;
			float DPS = mainWeaponDPS + SecondaryWeaponDPS;

			string combatInfo = "DPS: " + DPS + ", Attack Range: " + building.attackRange.Value + "\n";
			string specialInfo = "A Defensive Turret to protect captured areas under your control";

			toolTip.tipToShow = specialInfo;
			textInfo.text = costInfo + healthInfo + combatInfo;
		}
		else
		{
			BuildingManager building = GameManager.Instance.PlayerOneBuildingsList[i].GetComponent<BuildingManager>();

			string costInfo = "Cost:\n Money: " + building.moneyCost + ", Alloys: " + building.alloyCost + ", Crystals: " + building.crystalCost + "\n";
			string healthInfo = "Stats:\n Health: " + building.maxHealth.Value + ", Armour: " + building.armour.Value + "\n";
			string specialInfo = "";

			if (building.isGeneratorBuilding)
				specialInfo = "Provides power to other buildings";
			if (building.isRefineryBuilding)
				specialInfo = "Houses two cargoships to collect resources from nodes, cargoships will automatically respawn here when destroyed" +
					"\n\nCargoships can be reassigned to nodes by clicking on them then clicking on a resource node";
			if (building.isLightVehProdBuilding)
				specialInfo = "Allows the production of light units";
			if (building.isHeavyVehProdBuilding)
				specialInfo = "Allows the production of heavy units";
			if (building.isVTOLProdBuilding)
				specialInfo = "Allows the production of VTOL Gunship";

			toolTip.tipToShow = specialInfo;
			textInfo.text = costInfo + healthInfo;
		}
	}
	public void GrabUnitInfo(int i, Text textInfo, Image unitImage, ToolTips toolTip)
	{
		unitImage.sprite = gameManager.unitImageList[i];

		UnitStateController unit = GameManager.Instance.PlayerOneUnitsList[i].GetComponent<UnitStateController>();
		NavMeshAgent unitNavMesh = unit.GetComponent<NavMeshAgent>(); 

		string costInfo = "Cost:\n Money: " + unit.moneyCost + ", Alloys: " + unit.alloyCost + ", Crystals: " + unit.crystalCost + "\n";
		string healthInfo = "Stats:\n Health: " + unit.maxHealth.Value + ", Armour: " + unit.armour.Value + 
			", View Range " + unit.ViewRange + "\n" + "Speed: " + unitNavMesh.speed * 5 + "MPH \n";

		string specialInfo = "An Unarmed unit that cant fly, and is great for scouting and finding the enemy";
		string prodInfo = "Needs a Light Vehicle Production Building to be able to buy and place";

		string combatInfo = "";
		if (unit.isUnitArmed)
		{
			WeaponSystem weaponSystem = unit.GetComponent<WeaponSystem>();
			float mainWeaponDPS = weaponSystem.mainWeaponDamage.Value / weaponSystem.mainWeaponAttackSpeed;
			float SecondaryWeaponDPS = 0;
			float DPS = mainWeaponDPS + SecondaryWeaponDPS;

			combatInfo = "DPS:" + DPS + ", Attack Range: " + unit.attackRange.Value + "\n";
			specialInfo = "An Armed unit that cant fly and is fairly cheap to build in mass numbers, the standerd go to unit with a light autocannon " +
				"great at dealing with light and some medium armored units";

			if (weaponSystem.hasSecondaryWeapon)
			{
				SecondaryWeaponDPS = weaponSystem.secondaryWeaponDamage.Value / weaponSystem.secondaryWeaponAttackSpeed;
				DPS = mainWeaponDPS + SecondaryWeaponDPS;
				combatInfo = "DPS:" + DPS + ", Attack Range: " + unit.attackRange.Value + "\n";

				//Heavy Mech Support
				if (unit.entityName == "Heavy Mech Tank")
				{
					specialInfo = "An Armed unit that cant fly and excells at taking damage and shredding armored units up close with a " +
						"powerful but slow firing plasma bolt, later models now come equipped with a support laser mini gun for lighter armed units." +
						"A big downside is its heavy weight and size, limiting its speed compared to other units of its size";
				}
				else if (unit.entityName == "Heavy Mech Knight")
				{
					specialInfo = "An Armed unit that cant fly and excells at dealing consistent damage from medium to long range with dual heavy " +
						"autocannons, it also has decent armor to shrug off shots from most units and has many backup systems" +
						"Has a suprising amount of speed for its size but still lacks it compared to smaller units";
				}
				prodInfo = "\nNeeds a Heavy Vehicle Production Building to be able to buy and place";
			}
		}
		if (unit.isFlying)
		{
			specialInfo = "An Armed unit that can fly and easily pass over most canyons and mountains for ambushes, comes equipped with a powerful " +
				"gauss cannon, capable of punching small holes in most heavily armored units. It does however lack the armor compared to most units";
			prodInfo = "\nNeeds a VTOL Vehicle Production Building to be able to buy and place";
		}

		toolTip.tipToShow = specialInfo + "\n" + prodInfo;
		textInfo.text = costInfo + healthInfo + combatInfo;
	}
	public void LinkBuyButtons(int i, Button buttonToLink, List<GameObject> listType)
	{
		if (listType == GameManager.Instance.PlayerOneBuildingsList || listType == GameManager.Instance.PlayerTwoBuildingsList)
		{
			switch(i)
			{
				case 0:
				buyEnergyGenBuilding = buttonToLink;
				buyEnergyGenBuilding.onClick.AddListener(delegate { playerController.buildingPlacementManager.PlaceEnergyGenBuilding(); });
				buyEnergyGenBuilding.transform.GetChild(0).GetComponent<Text>().text = "Buy \"1\"";
				break;
				case 1:
				buyRefineryBuilding = buttonToLink;
				buyRefineryBuilding.onClick.AddListener(delegate { playerController.buildingPlacementManager.PlaceRefineryBuilding(); });
				buyRefineryBuilding.transform.GetChild(0).GetComponent<Text>().text = "Buy \"2\"";
				break;
				case 2:
				buyDefenseTurret = buttonToLink;
				buyDefenseTurret.onClick.AddListener(delegate { playerController.buildingPlacementManager.PlaceDefenseTurret(); });
				buyDefenseTurret.transform.GetChild(0).GetComponent<Text>().text = "Buy \"3\"";
				break;
				case 3:
				buyLightVehProdBuilding = buttonToLink;
				buyLightVehProdBuilding.onClick.AddListener(delegate { playerController.buildingPlacementManager.PlaceLightVehProdBuilding(); });
				buyLightVehProdBuilding.transform.GetChild(0).GetComponent<Text>().text = "Buy \"1\"";
				break;
				case 4:
				buyHeavyVehProdBuilding = buttonToLink;
				buyHeavyVehProdBuilding.onClick.AddListener(delegate { playerController.buildingPlacementManager.PlaceHeavyVehProdBuilding(); });
				buyHeavyVehProdBuilding.transform.GetChild(0).GetComponent<Text>().text = "Buy \"2\"";
				break;
				case 5:
				buyVTOLVehProdBuilding = buttonToLink;
				buyVTOLVehProdBuilding.onClick.AddListener(delegate { playerController.buildingPlacementManager.PlaceVTOLProdBuilding(); });
				buyVTOLVehProdBuilding.transform.GetChild(0).GetComponent<Text>().text = "Buy \"3\"";
				break;
			}
		}
		else if (listType == GameManager.Instance.PlayerOneUnitsList || listType == GameManager.Instance.PlayerTwoUnitsList)
		{
			switch (i)
			{
				case 0:
				buyScoutVehicle = buttonToLink;
				buyScoutVehicle.onClick.AddListener(delegate { playerController.unitProductionManager.AddScoutVehToBuildQueue(0); });
				buyScoutVehicle.transform.GetChild(0).GetComponent<Text>().text = "Buy \"1\"";
				break;
				case 1:
				buyRadarVehicle = buttonToLink;
				buyRadarVehicle.onClick.AddListener(delegate { playerController.unitProductionManager.AddRadarVehToBuildQueue(1); });
				buyRadarVehicle.transform.GetChild(0).GetComponent<Text>().text = "Buy \"2\"";
				break;
				case 2:
				buyLightMechVehicle = buttonToLink;
				buyLightMechVehicle.onClick.AddListener(delegate { playerController.unitProductionManager.AddLightMechToBuildQueue(2); });
				buyLightMechVehicle.transform.GetChild(0).GetComponent<Text>().text = "Buy \"3\"";
				break;
				case 3:
				buyHeavyMechKnightVehicle = buttonToLink;
				buyHeavyMechKnightVehicle.onClick.AddListener(delegate { playerController.unitProductionManager.AddHeavyMechKnightToBuildQueue(3); });
				buyHeavyMechKnightVehicle.transform.GetChild(0).GetComponent<Text>().text = "Buy \"1\"";
				break;
				case 4:
				buyHeavyMechTankVehicle = buttonToLink;
				buyHeavyMechTankVehicle.onClick.AddListener(delegate { playerController.unitProductionManager.AddHeavyMechTankToBuildQueue(4); });
				buyHeavyMechTankVehicle.transform.GetChild(0).GetComponent<Text>().text = "Buy \"2\"";
				break;
				case 5:
				buyVTOLVehicle = buttonToLink;
				buyVTOLVehicle.onClick.AddListener(delegate { playerController.unitProductionManager.AddVTOLToBuildQueue(5); });
				buyVTOLVehicle.transform.GetChild(0).GetComponent<Text>().text = "Buy \"3\"";
				break;
			}
		}
	}

	//UI UPDATES
	public void UpdateUiKeyBindsDisplay()
	{
		techTreeButtonText.text = "Tech Tree \"" + 
			InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTechTreeName] + "\"";
		unitGroupsButtonText.text = "Unit Groups \"" +
			InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindUnitGroupsList] + "\"";
		unitProductionButtonText.text = "Unit Production Queues \"" + 
			InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindUnitProdQueue] + "\"";

		baseBuildingsButtonText.text = "Basic Buildings \"" +
			InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopBaseBuildingsName] + "\"";
		vehProdBuildingsButtonText.text = "Veh Prod Buildings \"" +
			InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopVehProdBuildingsName] + "\"";
		lightVehiclesButtonText.text = "Light Vehicles \"" +
			InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopLightUnitsName] + "\"";
		heavyVehiclesButtonText.text = "Heavy Vehicles \"" +
			InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopHeavyUnitsName] + "\"";
	}
	public IEnumerator ResetTechUi(float timeToWaitSeconds)
	{
		yield return new WaitForSeconds(timeToWaitSeconds);
		if (!techTreeManager.isCurrentlyReseaching)
		{
			techTreeManager.currentResearchInfoText.text = "No Tech Currently Researching";
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("SELECT A NEW RESEARCH", 2f);
		}
	}
	public void UpdateTechUiComplete()
	{
		if (techTreeManager.currentReseachingTech.TimeToResearchSec > 0)
		{
			techTreeManager.currentReseachingTech.TimeToResearchSec -= Time.deltaTime;
			techTreeManager.currentResearchInfoText.text = techTreeManager.currentReseachingTech.TechName + 
				"\n Complete In: " + techTreeManager.currentReseachingTech.TimeToResearchSec + "s";
		}
		else
		{
			techTreeManager.currentResearchInfoText.text = techTreeManager.currentReseachingTech.TechName + "\n COMPLETE";
			StartCoroutine(ResetTechUi(10f));
		}
	}
	public void UpdateInGameTimerUI()
	{
		int seconds = (int)GameManager.Instance.secondsCount;
		gameTimerText.text = GameManager.Instance.hourCount.ToString() + ":" + GameManager.Instance.minuteCount.ToString() + 
			":" + seconds.ToString() + "s";
	}
	public void UpdateGameSpeedUi()
	{
		if (isGamePaused)
			gameSpeedText.text = "Game Paused";
		else
			gameSpeedText.text = "x" + gameSpeed.ToString() + " speed";
	}
	public void UpdateCurrentResourcesUI()
	{
		if (playerController.isPlayerOne)
		{
			CurrentMoneyText.text = GameManager.Instance.playerOneCurrentMoney.Value.ToString();
			CurrentAlloysText.text = GameManager.Instance.playerOneCurrentAlloys.Value.ToString();
			CurrentCrystalsText.text = GameManager.Instance.playerOneCurrentCrystals.Value.ToString();
		}
		else if (!playerController.isPlayerOne)
		{
			CurrentMoneyText.text = GameManager.Instance.playerTwoCurrentMoney.Value.ToString();
			CurrentAlloysText.text = GameManager.Instance.playerTwoCurrentAlloys.Value.ToString();
			CurrentCrystalsText.text = GameManager.Instance.playerTwoCurrentCrystals.Value.ToString();
		}
	}
	public bool CheckResourceCountMatches()
	{
		if (gameManager.isPlayerOne)
		{
			if (CurrentMoneyText.text == GameManager.Instance.playerOneCurrentMoney.Value.ToString())
				return true;
			else
				return false;
		}
		else
		{
			if (CurrentMoneyText.text == GameManager.Instance.playerTwoCurrentMoney.Value.ToString())
				return true;
			else
				return false;
		}
	}
	public void UpdateIncomeResourcesUI(int playerOneMoneyPerSecond, int playerOneAlloysPerSecond, int playerOneCrystalsPerSecond,
		int playerTwoMoneyPerSecond, int playerTwoAlloysPerSecond, int playerTwoCrystalsPerSecond)
	{
		if(playerController.isPlayerOne)
		{
			IncomeMoneyText.text = playerOneMoneyPerSecond.ToString() + "s";
			IncomeAlloysText.text = playerOneAlloysPerSecond.ToString() + "s";
			IncomeCrystalsText.text = playerOneCrystalsPerSecond.ToString() + "s";
		}
		else if (!playerController.isPlayerOne)
		{
			IncomeMoneyText.text = playerTwoMoneyPerSecond.ToString() + "s";
			IncomeAlloysText.text = playerTwoAlloysPerSecond.ToString() + "s";
			IncomeCrystalsText.text = playerTwoCrystalsPerSecond.ToString() + "s";
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
			if (unit.moneyCost == playerController.unitProductionManager.unitScoutVehicle.GetComponent<UnitStateController>().moneyCost)
				scoutVehicleCount++;
			else if (unit.moneyCost == playerController.unitProductionManager.unitRadarVehicle.GetComponent<UnitStateController>().moneyCost)
				radarVehicleCount++;
			else if (unit.moneyCost == playerController.unitProductionManager.unitLightMech.GetComponent<UnitStateController>().moneyCost)
				lightMechCount++;
			else if (unit.moneyCost == playerController.unitProductionManager.unitHeavyMechKnight.GetComponent<UnitStateController>().moneyCost || 
				unit.moneyCost == playerController.unitProductionManager.unitHeavyMechTank.GetComponent<UnitStateController>().moneyCost)
				heavyMechCount++;
			else if (unit.moneyCost == playerController.unitProductionManager.unitVTOL.GetComponent<UnitStateController>().moneyCost)
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
			groupFourInfoUI.text = info;
		else if (groupToUpdate == 5)
			groupFiveInfoUI.text = info;
	}
	public void ResetUnitGroupUI()
	{
		string info = 0 + "x Heavy Mech\n" + 0 + "x Light Mech\n" + 0 + "x VTOL\n" + 0 + "x Radar Vehicle\n" + 0 + "x Scout Vehicle";

		groupOneInfoUI.text = info;
		groupTwoInfoUI.text = info;
		groupThreeInfoUI.text = info;
		groupFourInfoUI.text = info;
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
