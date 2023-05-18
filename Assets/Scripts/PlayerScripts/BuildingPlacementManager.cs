using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementManager : MonoBehaviour
{
	[Header("Game Ui + Refs")]
	public PlayerController playerController;

	public bool isPlayerOne;
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
		if (isPlayerOne)
		{
			buildingHQ = GameManager.Instance.buildingHQPlayerOne;
			buildingEnergyGen = GameManager.Instance.buildingEnergyGenPlayerOne;
			buildingRefinery = GameManager.Instance.buildingRefineryPlayerOne;
			buildingLightVehProd = GameManager.Instance.buildingLightVehProdPlayerOne;
			buildingHeavyVehProd = GameManager.Instance.buildingHeavyVehProdPlayerOne;
			buildingVTOLProd = GameManager.Instance.buildingVTOLProdPlayerOne;
			//prefabs not made yet
			buildingHQ = GameManager.Instance.buildingHQPlayerOne;
		}
		if (!isPlayerOne)
		{
			buildingHQ = GameManager.Instance.buildingHQPlayerTwo;
			buildingEnergyGen = GameManager.Instance.buildingEnergyGenPlayerTwo;
			buildingRefinery = GameManager.Instance.buildingRefineryPlayerTwo;
			buildingLightVehProd = GameManager.Instance.buildingLightVehProdPlayerTwo;
			buildingHeavyVehProd = GameManager.Instance.buildingHeavyVehProdPlayerTwo;
			buildingVTOLProd = GameManager.Instance.buildingVTOLProdPlayerTwo;
			//prefabs not made yet
			buildingHQ = GameManager.Instance.buildingHQPlayerTwo;
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
			if (Physics.Raycast(ray, out RaycastHit hitInfo, playerController.ignoreMe))
			{
				currentBuildingPlacement.transform.position = hitInfo.point;
				float mouseWheelRotation = Input.mouseScrollDelta.y;
				currentBuildingPlacement.transform.Rotate(10 * mouseWheelRotation * Vector3.up);
			}
		}
	}
	public void PlaceBuildingManager()
	{
		//place building and toggle it on
		if (Input.GetMouseButtonDown(0) && currentBuildingPlacement.GetComponent<CanPlaceBuilding>().CheckIfCanPlace())
		{
			//enable building triggers, navMeshObstacle, set layer and unhighlight, -building cost and update resUI
			currentBuildingPlacement.GetComponent<BuildingManager>().enabled = true;

			if(currentBuildingPlacement.isRefineryBuilding)
				currentBuildingPlacement.GetComponent<RefineryController>().CheckCargoShipsCount();

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
			if (currentBuildingPlacement.isGeneratorBuilding)
				currentBuildingPlacement.GetComponent<EnergyGenController>().StartPower();
			currentBuildingPlacement = null;
			//NOTIFY PLAYER CODE HERE
		}
		else if (Input.GetMouseButtonDown(0) && !currentBuildingPlacement.GetComponent<CanPlaceBuilding>().CheckIfCanPlace())
			Debug.Log("Couldnt Place building");
		//NOTIFY PLAYER CODE HERE

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
			{
				Debug.LogError("Cannot Afford Building");
			}
		}
		else
		{
			Debug.LogError("Building Already Being Placed");
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
