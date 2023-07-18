using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefineryController : MonoBehaviour
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
		if(CargoShipList.Count < 2)
			StartCoroutine(ConstructNewCargoShip());
	}
	public IEnumerator ConstructNewCargoShip()
	{
		yield return new WaitForSeconds(5);
		GameObject go = Instantiate(cargoShipPrefab, vehSpawnLocation.transform.position, Quaternion.identity);
		CargoShipController script = go.GetComponent<CargoShipController>();
		script.refineryControllerParent = this;

		CargoShipList.Add(script);

		CheckCargoShipsCount();
	}
}
