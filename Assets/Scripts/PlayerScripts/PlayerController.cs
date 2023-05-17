using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	public LayerMask ignoreMe;

	[Header("Game Ui Refs")]
	public Camera miniMapCameraRenderer;
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
			PlayerInputManager();
	}
	public void PlayerInputManager()
	{
		//shop tab hotkeys
		if (Input.GetKeyDown(KeyCode.B))
		{
			gameUIManager.ShowBuildingShop();
		}
		if (Input.GetKeyDown(KeyCode.G))
		{
			gameUIManager.ShowUnitShopUnarmed();
		}
		if (Input.GetKeyDown(KeyCode.U))
		{
			gameUIManager.ShowUnitShopArmed();
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			gameUIManager.ShowGroupedUnits();
		}
		//hotkeys for items in shop tabs
		BuyShopItemHotkeys();

		//if (buildingPlacementManager.currentBuildingPlacement != null)
			//buildingPlacementManager.PlaceBuildingManager();

		//hotkeys for game speed Change
		if (Input.GetKeyDown(KeyCode.Equals))
		{
			gameUIManager.IncreaseGameSpeed();
		}
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			gameUIManager.DecreaseGameSpeed();
		}
		//enlarge/shrink Minimap
		if (Input.GetKeyDown(KeyCode.M))
		{
			gameUIManager.ChangeMiniMapSize();
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
				Debug.Log("Building already being placed");
				//NOTIFY PLAYER CODE HERE
			}
		}
	}
	public void BuildingAndUnitCost(int moneyCost, int alloyCost, int crystalCost)
	{
		GameManager.Instance.playerOneCurrentMoney -= moneyCost;
		GameManager.Instance.playerOneCurrentAlloys -= alloyCost;
		GameManager.Instance.playerOneCurrentCrystals -= crystalCost;
	}
}
