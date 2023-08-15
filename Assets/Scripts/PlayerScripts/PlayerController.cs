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
	public UnitSelectionManager unitSelectionManager;
	public BuildingPlacementManager buildingPlacementManager;
	public UnitProductionManager unitProductionManager;

	public bool isPlayerOne;

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
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Already Placing A Building", 2);
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
				if (isPlayerOne && gameUIManager.gameManager.playerOneBuildingHasUnlockedHeavyMechs.Value ||
					!isPlayerOne && gameUIManager.gameManager.playerTwoBuildingHasUnlockedHeavyMechs.Value)
					buildingPlacementManager.PlaceHeavyVehProdBuilding();
				else
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Heavy Mechs Tech Not Researched", 2f);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3) && buildingPlacementManager.currentBuildingPlacement == null)
			{
				if (isPlayerOne && gameUIManager.gameManager.playerOneBuildingHasUnlockedVtols.Value ||
					!isPlayerOne && gameUIManager.gameManager.playerTwoBuildingHasUnlockedVtols.Value)
					buildingPlacementManager.PlaceVTOLProdBuilding();
				else
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("VTOLS Tech Not Researched", 2f);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha1) && buildingPlacementManager.currentBuildingPlacement != null
				|| Input.GetKeyDown(KeyCode.Alpha2) && buildingPlacementManager.currentBuildingPlacement != null
				|| Input.GetKeyDown(KeyCode.Alpha3) && buildingPlacementManager.currentBuildingPlacement != null)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Already Placing A Building", 2);
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
	[ServerRpc(RequireOwnership = false)]
	public void EntityCostServerRPC(bool isPlayerOneCall, int moneyCost, int alloyCost, int crystalCost)
	{
		if (isPlayerOneCall)
		{
			GameManager.Instance.playerOneCurrentMoney.Value -= moneyCost;
			GameManager.Instance.playerOneCurrentAlloys.Value -= alloyCost;
			GameManager.Instance.playerOneCurrentCrystals.Value -= crystalCost;
		}
		else if (!isPlayerOneCall)
		{
			GameManager.Instance.playerTwoCurrentMoney.Value -= moneyCost;
			GameManager.Instance.playerTwoCurrentAlloys.Value -= alloyCost;
			GameManager.Instance.playerTwoCurrentCrystals.Value -= crystalCost;
		}
		UpdateClientUiClientRPC();
	}
	[ClientRpc]
	public void UpdateClientUiClientRPC()
	{
		StartCoroutine(GameManager.Instance.gameUIManager.UpdateCurrentResourcesUI(1f));
	}
}
