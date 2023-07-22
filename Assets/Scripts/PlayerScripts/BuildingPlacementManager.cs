using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class BuildingPlacementManager : NetworkBehaviour
{
	[Header("Game Ui + Refs")]
	public PlayerController playerController;

	public int buildingLayer;

	[Header("Player Building Prefabs")]
	public GameObject buildingHQ;
	public GameObject buildingEnergyGen;
	public GameObject buildingRefinery;
	public GameObject buildingTurret;
	public GameObject buildingLightVehProd;
	public GameObject buildingHeavyVehProd;
	public GameObject buildingVTOLProd;

	[Header("Dynamic Refs")]
	public Entities currentBuildingPlacement;
	public CanPlaceBuilding canPlaceBuilding;
	public ulong currentBuildingPlacementNetworkId;

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
			if (obj.GetComponent<TurretController>() != null)
				buildingTurret = obj.GetComponent<TurretController>().gameObject;
			else
			{
				BuildingManager building = obj.GetComponent<BuildingManager>();

				if (building.isGeneratorBuilding)
					buildingEnergyGen = building.gameObject;
				if (building.isRefineryBuilding)
					buildingRefinery = building.gameObject;
				if (building.isLightVehProdBuilding)
					buildingLightVehProd = building.gameObject;
				if (building.isHeavyVehProdBuilding)
					buildingHeavyVehProd = building.gameObject;
				if (building.isVTOLProdBuilding)
					buildingVTOLProd = building.gameObject;
			}
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
				float mouseWheelRotation = Input.mouseScrollDelta.y;
				MovePlayerBuildingServerRPC(hitInfo.point, mouseWheelRotation, currentBuildingPlacementNetworkId);
			}
		}
	}
	//place building and toggle it on
	public void PlaceBuildingManager()
	{
		if (Input.GetMouseButtonDown(0) && currentBuildingPlacement.GetComponent<CanPlaceBuilding>().CheckIfCanPlace())
		{
			//enable building triggers, navMeshObstacle, set layer and unhighlight, -building cost and update resUI
			if (currentBuildingPlacement.GetComponent<BuildingManager>() != null)
			{
				currentBuildingPlacement.GetComponent<BuildingManager>().enabled = true;
				currentBuildingPlacement.GetComponent<BuildingManager>().AddBuildingRefs();

				if (currentBuildingPlacement.GetComponent<BuildingManager>().isVTOLProdBuilding)
					currentBuildingPlacement.GetComponent<SphereCollider>().isTrigger = true;
				else
					currentBuildingPlacement.GetComponent<BoxCollider>().isTrigger = true;
			}
			else if (currentBuildingPlacement.GetComponent<TurretController>() != null)
			{
				currentBuildingPlacement.GetComponent<TurretController>().enabled = true;
				currentBuildingPlacement.GetComponent<TurretController>().AddTurretRefs();
				if (currentBuildingPlacement.GetComponent<TurretController>().isPlayerOneEntity)
					currentBuildingPlacement.GetComponent<TurretController>().gameObject.layer = LayerMask.NameToLayer("PlayerOneUnits");
				else
					currentBuildingPlacement.GetComponent<TurretController>().gameObject.layer = LayerMask.NameToLayer("PlayerTwoUnits");
				currentBuildingPlacement.GetComponent<BoxCollider>().isTrigger = true;
				currentBuildingPlacement.transform.GetChild(4).GetComponent<SphereCollider>().enabled = true;
			}

			currentBuildingPlacement.gameObject.layer = buildingLayer;
			currentBuildingPlacement.GetComponent<CanPlaceBuilding>().highlighterObj.SetActive(false);
			currentBuildingPlacement.GetComponent<CanPlaceBuilding>().navMeshObstacle.enabled = true;
			currentBuildingPlacement.GetComponent<CanPlaceBuilding>().isPlaced = true;

			BuildingCost(currentBuildingPlacement.moneyCost, currentBuildingPlacement.alloyCost, currentBuildingPlacement.crystalCost);
			currentBuildingPlacement = null;
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Building placed", 1f);
			GameManager.Instance.gameUIManager.UpdateCurrentResourcesUI();
		}

		if (Input.GetMouseButtonDown(1))
			Destroy(currentBuildingPlacement.gameObject);
	}

	//buy Invidual buildings
	public void PlaceEnergyGenBuilding()
	{
		BuyBuilding(0);
	}
	public void PlaceRefineryBuilding()
	{
		BuyBuilding(1);
	}
	public void PlaceDefenseTurret()
	{
		BuyBuilding(2);
	}
	public void PlaceLightVehProdBuilding()
	{
		BuyBuilding(3);
	}
	public void PlaceHeavyVehProdBuilding()
	{
		BuyBuilding(4);
	}
	public void PlaceVTOLProdBuilding()
	{
		BuyBuilding(5);
	}

	//buy functions
	public void BuyBuilding(int buildingIndex)
	{
		Entities building = GameManager.Instance.PlayerOneBuildingsList[buildingIndex].GetComponent<Entities>();

		if (currentBuildingPlacement == null)
		{
			if (CheckIfCanBuy(building.moneyCost, building.alloyCost, building.crystalCost))
				SpawnPlayerBuildingServerRPC(buildingIndex);
			else
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Cant Afford buildings", 2);
		}
		else
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Already Placing a Building", 2);
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

	[ServerRpc(RequireOwnership = false)]
	public void SpawnPlayerBuildingServerRPC(int buildingIndex, ServerRpcParams serverRpcParams = default)
	{
		ulong clientId = serverRpcParams.Receive.SenderClientId;

		if (clientId == 0)
		{
			GameObject obj = Instantiate(GameManager.Instance.PlayerOneBuildingsList[buildingIndex], new Vector3(0, 5, 0), Quaternion.identity);
			obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);



			if (obj.GetComponent<BuildingManager>() != null)
				playerController.gameUIManager.techTreeManager.ApplyTechUpgradesToNewBuildings(obj);
			else
				playerController.gameUIManager.techTreeManager.ApplyTechUpgradesToNewUnits(obj);
		}
		else if (clientId == 1)
		{
			GameObject obj = Instantiate(GameManager.Instance.PlayerTwoBuildingsList[buildingIndex], new Vector3(0, 5, 0), Quaternion.identity);
			obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
			if (obj.GetComponent<BuildingManager>() != null)
				playerController.gameUIManager.techTreeManager.ApplyTechUpgradesToNewBuildings(obj);
			else
				playerController.gameUIManager.techTreeManager.ApplyTechUpgradesToNewUnits(obj);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void MovePlayerBuildingServerRPC(Vector3 vector3Position, float RotationSpeed, ulong networkObjId)
	{
		NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].transform.position = vector3Position;
		NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].transform.Rotate(10 * RotationSpeed * Vector3.up);
	}
}
