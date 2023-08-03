using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class RefineryController : NetworkBehaviour
{
	public BuildingManager building;
	public GameObject vehSpawnLocation;

	public GameObject cargoShipPrefab;

	public List<CargoShipController> CargoShipList;

	public List<ResourceNodes> resourceNodesList;

	public void Start()
	{
		ResourceNodes[] resourcesNodes = FindObjectsOfType<ResourceNodes>();

		foreach (ResourceNodes resourceNode in resourcesNodes)
		{
			resourceNodesList.Add(resourceNode);
		}
	}
	public void RefineResources(CargoShipController cargoShip)
	{
		float bonus = building.playerController.gameUIManager.techTreeManager.buildingBonusToResourceIncome;
		int moneyToAdd = 0;
		int alloysToAdd = 0;
		int crystalsToAdd = 0;

		if (cargoShip.crystalsCount != 0)
		{
			moneyToAdd = (int)(cargoShip.crystalsCount * 3 * bonus);
			alloysToAdd = 0;
			crystalsToAdd = (int)(cargoShip.crystalsCount * bonus);
		}
		else if (cargoShip.alloysCount != 0)
		{
			moneyToAdd = (int)(cargoShip.alloysCount * 1.2f * bonus);
			alloysToAdd = (int)(cargoShip.alloysCount * bonus);
			crystalsToAdd = 0;
		}

		GameManager.Instance.playerOneCurrentMoney += moneyToAdd;
		GameManager.Instance.playerOneCurrentAlloys += alloysToAdd;
		GameManager.Instance.playerOneCurrentCrystals += crystalsToAdd;

		GameManager.Instance.playerOneIncomeMoney += moneyToAdd;
		GameManager.Instance.playerOneIncomeAlloys += alloysToAdd;
		GameManager.Instance.playerOneIncomeCrystals += crystalsToAdd;

		GameManager.Instance.gameUIManager.UpdateCurrentResourcesUI();
	}
	public void CheckCargoShipsCount()
	{
		if(CargoShipList.Count < 2 && IsOwner)
			StartCoroutine(ConstructNewCargoShip());
	}
	public IEnumerator ConstructNewCargoShip()
	{
		yield return new WaitForSeconds(5);
		if (CargoShipList.Count < 2)
			SpawnCargoShipsServerRPC(GetComponent<NetworkObject>().NetworkObjectId, vehSpawnLocation.transform.position);

		//GameObject go = Instantiate(cargoShipPrefab, vehSpawnLocation.transform.position, Quaternion.identity);
		//CargoShipController script = go.GetComponent<CargoShipController>();
		//script.refineryControllerParent = this;

		//CargoShipList.Add(script);

		//CheckCargoShipsCount();
	}
	[ServerRpc(RequireOwnership = false)]
	public void SpawnCargoShipsServerRPC(ulong buildingNetworkObjId, Vector3 spawnLocation)
	{
		GameObject obj = Instantiate(cargoShipPrefab, spawnLocation, Quaternion.identity);
		obj.GetComponent<NetworkObject>().Spawn(true);

		SpawnCargoShipsClientRPC(buildingNetworkObjId, obj.GetComponent<NetworkObject>().NetworkObjectId);
	}
	[ClientRpc]
	public void SpawnCargoShipsClientRPC(ulong buildingNetworkObjId, ulong cargoShipNetworkObjId)
	{
		RefineryController refinery = NetworkManager.SpawnManager.SpawnedObjects[buildingNetworkObjId].GetComponent<RefineryController>();
		CargoShipController cargoShip = NetworkManager.SpawnManager.SpawnedObjects[cargoShipNetworkObjId].GetComponent<CargoShipController>();

		refinery.CargoShipList.Add(cargoShip);
		cargoShip.refineryControllerParent = refinery;
		refinery.CheckCargoShipsCount();
	}
}
