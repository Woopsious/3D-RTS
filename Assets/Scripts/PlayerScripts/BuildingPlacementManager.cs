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
		if (currentBuildingPlacement != null)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit hitInfo, 250f, playerController.ignoreMe))
			{
				float mouseWheelRotation = Input.mouseScrollDelta.y;
				currentBuildingPlacement.transform.position = hitInfo.point;
				currentBuildingPlacement.transform.Rotate(10 * mouseWheelRotation * Vector3.up);
			}
		}
	}

	//place building and toggle it on
	public void PlaceBuildingManager()
	{
		if (Input.GetMouseButtonDown(0) && currentBuildingPlacement.GetComponent<CanPlaceBuilding>().CheckIfCanPlace())
			TryPlaceCurrentBuildingPlacementServerRPC(currentBuildingPlacement.GetComponent<NetworkObject>().NetworkObjectId);

		if (Input.GetMouseButtonDown(1))
			CancelBuildingPlacementServerRPC(currentBuildingPlacement.GetComponent<NetworkObject>().NetworkObjectId);
	}
	[ServerRpc(RequireOwnership = false)]
	public void ApplyTechUpgradesToNewBuildingsServerRPC(ulong buildingNetworkedId)
	{
		if (NetworkManager.SpawnManager.SpawnedObjects[buildingNetworkedId].GetComponent<BuildingManager>() != null)
		{
			BuildingManager building = NetworkManager.SpawnManager.SpawnedObjects[buildingNetworkedId].GetComponent<BuildingManager>();

			if (building.isPlayerOneEntity)
			{
				building.currentHealth.Value = (int)(building.currentHealth.Value *
					playerController.gameUIManager.gameManager.playerOneBuildingHealthPercentageBonus.Value);
				building.maxHealth.Value = (int)(building.maxHealth.Value * 
					playerController.gameUIManager.gameManager.playerOneBuildingHealthPercentageBonus.Value);
				building.armour.Value = (int)(building.armour.Value * 
					playerController.gameUIManager.gameManager.playerOneBuildingArmourPercentageBonus.Value);
			}
			if (!building.isPlayerOneEntity)
			{
				building.currentHealth.Value = (int)(building.currentHealth.Value *
					playerController.gameUIManager.gameManager.playerTwoBuildingHealthPercentageBonus.Value);
				building.maxHealth.Value = (int)(building.maxHealth.Value * 
					playerController.gameUIManager.gameManager.playerTwoBuildingHealthPercentageBonus.Value);
				building.armour.Value = (int)(building.armour.Value * 
					playerController.gameUIManager.gameManager.playerTwoBuildingArmourPercentageBonus.Value);
			}
		}
		else
			playerController.unitProductionManager.ApplyTechUpgradesToNewUnitsServerRPC(buildingNetworkedId);
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
			if (playerController.CheckIfCanBuyEntity(building.moneyCost, building.alloyCost, building.crystalCost))
				SpawnPlayerBuildingServerRPC(buildingIndex);
			else
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Cant Afford building", 2);
		}
		else
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Already Placing a Building", 2);
	}

	//NETWORKING FUNCTIONS
	[ServerRpc(RequireOwnership = false)]
	public void SpawnPlayerBuildingServerRPC(int buildingIndex, ServerRpcParams serverRpcParams = default)
	{
		if (!IsServer) return;
		ulong clientId = serverRpcParams.Receive.SenderClientId;

		if (clientId == 0)
		{
			GameObject obj = Instantiate(GameManager.Instance.PlayerOneBuildingsList[buildingIndex], new Vector3(0, 5, 0), Quaternion.identity);
			obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
		}
		else if (clientId == 1)
		{
			GameObject obj = Instantiate(GameManager.Instance.PlayerTwoBuildingsList[buildingIndex], new Vector3(0, 5, 0), Quaternion.identity);
			obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void CancelBuildingPlacementServerRPC(ulong networkObjId)
	{
		if (!IsServer) return;
		NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].Despawn(true);
	}

	[ServerRpc(RequireOwnership = false)]
	public void TryPlaceCurrentBuildingPlacementServerRPC(ulong networkObjId)
	{
		if (!IsServer) return;
		ApplyTechUpgradesToNewBuildingsServerRPC(networkObjId);
		BuildingPlacedClientRPC(networkObjId);
	}
	[ClientRpc]
	public void BuildingPlacedClientRPC(ulong networkObjId)
	{
		NetworkObject buildingObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId];
		//enable building triggers, navMeshObstacle, set layer and unhighlight, -building cost and update resUI
		if (buildingObj.GetComponent<BuildingManager>() != null)
		{
			buildingObj.GetComponent<BuildingManager>().enabled = true;

			if (buildingObj.GetComponent<BuildingManager>().isVTOLProdBuilding)
				buildingObj.GetComponent<SphereCollider>().isTrigger = true;
			else
				buildingObj.GetComponent<BoxCollider>().isTrigger = true;
		}
		else if (buildingObj.GetComponent<TurretController>() != null)
		{
			buildingObj.GetComponent<TurretController>().enabled = true;
			buildingObj.GetComponent<BoxCollider>().isTrigger = true;
			buildingObj.transform.GetChild(4).GetComponent<SphereCollider>().enabled = true;
		}

		if (buildingObj.GetComponent<Entities>().isPlayerOneEntity)
			buildingObj.GetComponent<Entities>().gameObject.layer = LayerMask.NameToLayer("PlayerOneUnits");
		else
			buildingObj.GetComponent<Entities>().gameObject.layer = LayerMask.NameToLayer("PlayerTwoUnits");

		buildingObj.GetComponent<CanPlaceBuilding>().highlighterObj.SetActive(false);
		buildingObj.GetComponent<CanPlaceBuilding>().navMeshObstacle.enabled = true;
		buildingObj.GetComponent<CanPlaceBuilding>().isPlaced = true;

		if (currentBuildingPlacement != null && currentBuildingPlacement.GetComponent<NetworkObject>().IsOwner)
		{
			playerController.EntityCostServerRPC(playerController.isPlayerOne,
				currentBuildingPlacement.moneyCost, currentBuildingPlacement.alloyCost, currentBuildingPlacement.crystalCost);
			currentBuildingPlacement = null;
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Building placed", 1f);
		}
	}
}
