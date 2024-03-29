using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
	public LayerMask ignoreMe;

	[Header("Game Ui Refs")]
	public Camera miniMapCameraRenderer;
	public CameraController mainCameraParent;
	public GameUIManager gameUIManager;

	[Header("Refs")]
	public UnitSelectionManager unitSelectionManager;
	public BuildingPlacementManager buildingPlacementManager;
	public UnitProductionManager unitProductionManager;
	public List<CapturePointController> capturePointsList;

	public bool isPlayerOne;
	public bool isInTacticalView;

	[Header("Dynamic Refs")]
	public List<UnitStateController> SpottedUnitsList;
	public List<BuildingManager> buildingListForPlayer;
	public List<UnitStateController> unitListForPlayer;
	public List<TurretController> turretDefensesList;
	public List<BuildingManager> lightVehProdBuildingsList;
	public List<BuildingManager> heavyVehProdBuildingsList;
	public List<BuildingManager> vtolVehProdBuildingsList;

	public void Start()
	{
		isPlayerOne = GameManager.Instance.isPlayerOne;
		isInTacticalView = false;

		if (isPlayerOne)
		{
			int playerOneMiniMapLayer = LayerMask.NameToLayer("PlayerOneMiniMapRender");
			miniMapCameraRenderer.cullingMask |= (1 << playerOneMiniMapLayer);
		}
		else if (!isPlayerOne)
		{
			int playerTwoMiniMapLayer = LayerMask.NameToLayer("PlayerTwoMiniMapRender");
			miniMapCameraRenderer.cullingMask |=  (1 << playerTwoMiniMapLayer);
		}
	}
	public void Update()
	{
		PlayerInputs();
		IsMouseOverUI();
	}
	public void PlayerInputs()
	{
		if (GameManager.Instance.hasGameStarted.Value == false || GameManager.Instance.hasGameEnded.Value == true) return;

		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTacViewName]))
			TacticalViewMode();

		MenuHotkeys();
		BuyShopItemHotkeys();
		GameSpeedHotkeys();

		buildingPlacementManager.BuildingFollowsMouseCursor();
		buildingPlacementManager.PlaceBuildingManager();

		unitProductionManager.ShowUnitBuildGhostProjections();
		unitProductionManager.PlaceUnitManager();

		unitSelectionManager.EntitySelectionAndDeselection();
		unitSelectionManager.ManageSelectedUnitsAndGroups();
		unitSelectionManager.ManageUnitGhostProjections();
		unitSelectionManager.TrackIfGhostProjectionsAreTouchingNavMesh();
	}
	public void TacticalViewMode()
	{
		if (!isInTacticalView)
			ShowTacticalView();
		else
			HideTacticalView();
	}
	public void ShowTacticalView()
	{
		isInTacticalView = true;
		ShowAllHealthBars();
		ShowAllCapturpointAreas();
		ShowAllResourceAmountInNodes();
	}
	public void HideTacticalView()
	{
		isInTacticalView = false;
		HideAllHealthBars();
		HideAllCapturpointAreas();
		HideAllResourceAmountInNodes();
	}
	public void ShowAllHealthBars()
	{
		Entities[] entities = FindObjectsOfType<Entities>();
		foreach (Entities entity in entities)
			entity.ShowUIHealthBar();
	}
	public void ShowAllCapturpointAreas()
	{
		foreach (CapturePointController capturePoint in capturePointsList)
			capturePoint.ShowCapturePointArea();
	}
	public void ShowAllResourceAmountInNodes()
	{
		foreach (CapturePointController capturePoint in capturePointsList)
		{
			foreach (ResourceNodes resourceNode in capturePoint.resourceNodes)
				resourceNode.ShowResourceCounterUi();
		}
	}
	public void HideAllHealthBars()
	{
		Entities[] entities = FindObjectsOfType<Entities>();
		foreach (Entities entity in entities)
			entity.HideUIHealthBar();
	}
	public void HideAllCapturpointAreas()
	{
		foreach (CapturePointController capturePoint in capturePointsList)
			capturePoint.HideCapturePointArea();
	}
	public void HideAllResourceAmountInNodes()
	{
		foreach (CapturePointController capturePoint in capturePointsList)
		{
			foreach (ResourceNodes resourceNode in capturePoint.resourceNodes)
				resourceNode.HideResourceCounterUi();
		}
	}
	public void GameSpeedHotkeys()
	{
		if (Input.GetKeyDown(KeyCode.Equals) && !gameUIManager.gameManager.isMultiplayerGame)
			gameUIManager.IncreaseGameSpeed();
		if (Input.GetKeyDown(KeyCode.Minus) && !gameUIManager.gameManager.isMultiplayerGame)
			gameUIManager.DecreaseGameSpeed();
		if (Input.GetKeyDown(KeyCode.Space) && !gameUIManager.gameManager.isMultiplayerGame)
			gameUIManager.PauseGame();
	}
	//hotkeys for game menu functions
	public void MenuHotkeys()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (gameUIManager.settingsObj.activeInHierarchy)
				gameUIManager.CloseSettings();
			else
				gameUIManager.OpenSettings();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopBaseBuildingsName]))
		{
			gameUIManager.ShowBuildingsBaseShop();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopVehProdBuildingsName]))
		{
			gameUIManager.ShowBuildingsVehicleProdShop();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopLightUnitsName]))
		{
			gameUIManager.ShowUnitsLightShop();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopHeavyUnitsName]))
		{
			gameUIManager.ShowUnitsHeavyShop();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindTechTreeName]))
		{
			gameUIManager.ShowTechTree();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindUnitProdQueue]))
		{
			gameUIManager.ShowUnitProdQueues();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindUnitGroupsList]))
		{
			gameUIManager.ShowGroupedUnits();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindMiniMapName]))
		{
			gameUIManager.miniMap.ChangeAndUpdateMiniMapSize();
		}
	}
	//logic path for quick buying units/buildings
	public void BuyShopItemHotkeys()
	{
		if (!Input.GetKey(KeyCode.LeftShift) && gameUIManager.unitsLightUiShopOneObj.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddScoutVehToBuildQueue(0);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddRadarVehToBuildQueue(1);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddLightMechToBuildQueue(2);
			}
		}
		else if (!Input.GetKey(KeyCode.LeftShift) && gameUIManager.unitsHeavyUiShopTwoObj.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddHeavyMechKnightToBuildQueue(3);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddHeavyMechTankToBuildQueue(4);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddVTOLToBuildQueue(5);
			}
			if (Input.GetKeyDown(KeyCode.Alpha4) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				Debug.LogError("Future Defense Turret not added");
			}
		}
		else if (!Input.GetKey(KeyCode.LeftShift) && gameUIManager.buildingsBaseUiShopObj.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceEnergyGenBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceRefineryBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceDefenseTurret();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha1) && buildingPlacementManager.currentBuildingPlacement != null 
				|| Input.GetKeyDown(KeyCode.Alpha2) && buildingPlacementManager.currentBuildingPlacement != null
				|| Input.GetKeyDown(KeyCode.Alpha3) && buildingPlacementManager.currentBuildingPlacement != null )
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Already Placing A Building", 2f);
			}
		}
		else if (!Input.GetKey(KeyCode.LeftShift) && gameUIManager.buildingsVehicleProdUiShopObj.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceLightVehProdBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceHeavyVehProdBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceVTOLProdBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha1) && buildingPlacementManager.currentBuildingPlacement != null
				|| Input.GetKeyDown(KeyCode.Alpha2) && buildingPlacementManager.currentBuildingPlacement != null
				|| Input.GetKeyDown(KeyCode.Alpha3) && buildingPlacementManager.currentBuildingPlacement != null)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Already Placing A Building", 2f);
			}
		}
	}
	public bool IsMouseOverUI()
	{
		return EventSystem.current.IsPointerOverGameObject();
	}
	public bool CheckIfCanBuyEntity(int MoneyCost, int AlloyCost, int CrystalCost)
	{
		if (isPlayerOne)
		{
			if (MoneyCost > GameManager.Instance.playerOneCurrentMoney.Value || AlloyCost > GameManager.Instance.playerOneCurrentAlloys.Value
				|| CrystalCost > GameManager.Instance.playerOneCurrentCrystals.Value)
			{
				return false;
			}
			return true;
		}
		else if (!isPlayerOne)
		{
			if (MoneyCost > GameManager.Instance.playerTwoCurrentMoney.Value || AlloyCost > GameManager.Instance.playerTwoCurrentAlloys.Value
				|| CrystalCost > GameManager.Instance.playerTwoCurrentCrystals.Value)
			{
				return false;
			}
			return true;
		}
		else
		{
			Debug.LogError("This error shouldnt happen");
			return false;
		}
	}
}
