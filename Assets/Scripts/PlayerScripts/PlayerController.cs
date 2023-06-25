using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	public LayerMask ignoreMe;

	[Header("Game Ui Refs")]
	public Camera miniMapCameraRenderer;
	public CameraController mainCameraParent;
	public GameUIManager gameUIManager;
	public UnitSelectionManager unitSelectionManager;
	public BuildingPlacementManager buildingPlacementManager;
	public UnitProductionManager unitProductionManager;

	public bool isPlayerOne;

	[Header("Dynamic Refs")]
	public List<UnitStateController> unitListForPlayer;
	public List<UnitStateController> SpottedUnitsList;
	public List<BuildingManager> lightVehProdBuildingsList;
	public List<BuildingManager> heavyVehProdBuildingsList;
	public List<BuildingManager> vtolVehProdBuildingsList;

	public void Start()
	{
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
		if (SceneManager.GetActiveScene().buildIndex == 1)
			PlayerInputs();

		IsMouseOverUI();
	}
	public void PlayerInputs()
	{
		MenuHotkeys();
		BuyShopItemHotkeys();
		GameSpeedHotkeys();
	}
	public void GameSpeedHotkeys()
	{
		if (Input.GetKeyDown(KeyCode.Equals))
		{
			gameUIManager.IncreaseGameSpeed();
		}
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			gameUIManager.DecreaseGameSpeed();
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			gameUIManager.PauseGame();
		}
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
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopBuildingsName]))
		{
			gameUIManager.ShowBuildingShop();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopLightUnitsName]))
		{
			gameUIManager.ShowUnitShopUnarmed();
		}
		if (Input.GetKeyDown(InputManager.Instance.keyBindDictionary[InputManager.Instance.keyBindShopHeavyUnitsName]))
		{
			gameUIManager.ShowUnitShopArmed();
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
		if (gameUIManager.unitUiShopOneObj.activeInHierarchy)
		{
			if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddScoutVehToBuildQueue();
			}
			else if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddRadarVehToBuildQueue();
			}
			if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha3) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddLightMechToBuildQueue();
			}
		}
		else if (gameUIManager.unitUiShopTwoObj.activeInHierarchy)
		{
			if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddHeavyMechKnightToBuildQueue();
			}
			if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddHeavyMechTankToBuildQueue();
			}
			if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha3) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				unitProductionManager.AddVTOLToBuildQueue();
			}
			if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha4) && unitProductionManager.currentUnitPlacements.Count < 5)
			{
				Debug.LogError("Future Defense Turret not added");
			}
		}
		else if (!Input.GetKey(KeyCode.LeftShift) && gameUIManager.buildingsUiShopObj.activeInHierarchy)
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
				buildingPlacementManager.PlaceLightVehProdBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha4) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceHeavyVehProdBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha5) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceVTOLProdBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha6) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				buildingPlacementManager.PlaceEnergyGenBuilding();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha1) && buildingPlacementManager.currentBuildingPlacement != null 
				|| Input.GetKeyDown(KeyCode.Alpha2) && buildingPlacementManager.currentBuildingPlacement != null
				|| Input.GetKeyDown(KeyCode.Alpha3) && buildingPlacementManager.currentBuildingPlacement != null 
				|| Input.GetKeyDown(KeyCode.Alpha4) && buildingPlacementManager.currentBuildingPlacement != null
				|| Input.GetKeyDown(KeyCode.Alpha5) && buildingPlacementManager.currentBuildingPlacement != null 
				|| Input.GetKeyDown(KeyCode.Alpha6) && buildingPlacementManager.currentBuildingPlacement != null)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Already Placing A Building", 2);
				//NOTIFY PLAYER CODE HERE
			}
		}
	}

	public bool IsMouseOverUI()
	{
		return EventSystem.current.IsPointerOverGameObject();
	}
}
