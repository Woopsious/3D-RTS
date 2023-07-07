using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementManager : MonoBehaviour
{
	[Header("Game Ui + Refs")]
	public PlayerController playerController;

	public int buildingLayer;

	[Header("Player Building Prefabs")]
	public GameObject buildingHQ;
	public GameObject buildingEnergyGen;
	public GameObject buildingRefinery;
	public GameObject buildingLightVehProd;
	public GameObject buildingHeavyVehProd;
	public GameObject buildingVTOLProd;
	public GameObject buildingTurret;

	[Header("Dynamic Refs")]
	public BuildingManager currentBuildingPlacement;
	public CanPlaceBuilding canPlaceBuilding;

	public void Start()
	{
		if (playerController.isPlayerOne)
		{
			AssignBuildingRefs(GameManager.Instance.PlayerOneBuildingsList);
		}
		if (!playerController.isPlayerOne)
		{
			AssignBuildingRefs(GameManager.Instance.PlayerTwoBuildingsList);
		}
	}
	public void AssignBuildingRefs(List<GameObject> buildingList)
	{
		foreach (GameObject obj in buildingList)
		{
			BuildingManager building = obj.GetComponent<BuildingManager>();

			if (building.isGeneratorBuilding)
				buildingEnergyGen = building.gameObject;
			if (building.isRefineryBuilding)
				buildingRefinery = building.gameObject;
			if (building.isGeneratorBuilding)
				buildingEnergyGen = building.gameObject;
			if (building.isLightVehProdBuilding)
				buildingLightVehProd = building.gameObject;
			if (building.isHeavyVehProdBuilding)
				buildingHeavyVehProd = building.gameObject;
			if (building.isVTOLProdBuilding)
				buildingVTOLProd = building.gameObject;
		}
	}
	public void Update()
	{
		BuildingFollowsMouseCursor();

		if (currentBuildingPlacement != null && !playerController.IsMouseOverUI())
			PlaceBuildingManager();
	}
	public void BuildingFollowsMouseCursor()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (currentBuildingPlacement != null)
		{
			if (Physics.Raycast(ray, out RaycastHit hitInfo, 250f, playerController.ignoreMe))
			{
				currentBuildingPlacement.transform.position = hitInfo.point;
				float mouseWheelRotation = Input.mouseScrollDelta.y;
				currentBuildingPlacement.transform.Rotate(10 * mouseWheelRotation * Vector3.up);
			}
		}
	}
	//place building and toggle it on
	public void PlaceBuildingManager()
	{
		if (Input.GetMouseButtonDown(0) && currentBuildingPlacement.GetComponent<CanPlaceBuilding>().CheckIfCanPlace())
		{
			//enable building triggers, navMeshObstacle, set layer and unhighlight, -building cost and update resUI
			currentBuildingPlacement.GetComponent<BuildingManager>().enabled = true;
			if(currentBuildingPlacement.isVTOLProdBuilding)
				currentBuildingPlacement.GetComponent<SphereCollider>().isTrigger = true;
			else
				currentBuildingPlacement.GetComponent<BoxCollider>().isTrigger = true;

			currentBuildingPlacement.gameObject.layer = buildingLayer;
			currentBuildingPlacement.GetComponent<CanPlaceBuilding>().highlighterObj.SetActive(false);
			currentBuildingPlacement.GetComponent<CanPlaceBuilding>().navMeshObstacle.enabled = true;
			currentBuildingPlacement.GetComponent<CanPlaceBuilding>().isPlaced = true;

			BuildingCost(currentBuildingPlacement.moneyCost, currentBuildingPlacement.alloyCost, currentBuildingPlacement.crystalCost);
			currentBuildingPlacement.AddBuildingRefs();
			currentBuildingPlacement = null;
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Building placed", 1f);
			GameManager.Instance.gameUIManager.UpdateCurrentResourcesUI();
		}
		else if (Input.GetMouseButtonDown(0) && !currentBuildingPlacement.GetComponent<CanPlaceBuilding>().CheckIfCanPlace())
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Couldnt place building", 1f);

		if (Input.GetMouseButtonDown(1))
			Destroy(currentBuildingPlacement.gameObject);
	}

	//buy Invidual buildings
	public void PlaceEnergyGenBuilding()
	{
		BuyBuilding(buildingEnergyGen);
	}
	public void PlaceRefineryBuilding()
	{
		BuyBuilding(buildingRefinery);
	}
	public void PlaceLightVehProdBuilding()
	{
		BuyBuilding(buildingLightVehProd);
	}
	public void PlaceHeavyVehProdBuilding()
	{
		BuyBuilding(buildingHeavyVehProd);
	}
	public void PlaceVTOLProdBuilding()
	{
		BuyBuilding(buildingVTOLProd);
	}

	//buy functions
	public void BuyBuilding(GameObject buildingType)
	{
		BuildingManager building = buildingType.GetComponent<BuildingManager>();

		if (currentBuildingPlacement == null)
		{
			if (CheckIfCanBuy(building.moneyCost, building.alloyCost, building.crystalCost))
			{
				GameObject obj = Instantiate(buildingType, new Vector3(0, 5, 0), Quaternion.identity);
				currentBuildingPlacement = obj.GetComponent<BuildingManager>();
				canPlaceBuilding = obj.GetComponent<CanPlaceBuilding>();
				obj.GetComponent<BuildingManager>().playerController = playerController;
			}
			else
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Cant Afford buildings", 2);
		}
	}
	public void BuildingCost(int moneyCost, int alloyCost, int crystalCost)
	{
		GameManager.Instance.playerOneCurrentMoney -= moneyCost;
		GameManager.Instance.playerOneCurrentAlloys -= alloyCost;
		GameManager.Instance.playerOneCurrentCrystals -= crystalCost;

		playerController.gameUIManager.UpdateCurrentResourcesUI();
	}
	public bool CheckIfCanBuy(int MoneyCost, int AlloyCost, int CrystalCost)
	{
		if (MoneyCost > GameManager.Instance.playerOneCurrentMoney || AlloyCost > GameManager.Instance.playerOneCurrentAlloys
			|| CrystalCost > GameManager.Instance.playerOneCurrentCrystals)
		{
			return false;
		}
		return true;
	}
}
