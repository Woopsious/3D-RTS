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
		int moneyToAdd = 0;
		int alloysToAdd = 0;
		int crystalsToAdd = 0;

		if (cargoShip.crystalsCount != 0)
		{
			moneyToAdd = cargoShip.crystalsCount * 5;
			alloysToAdd = 0;
			crystalsToAdd = cargoShip.crystalsCount;
		}
		else if (cargoShip.alloysCount != 0)
		{
			moneyToAdd = cargoShip.alloysCount * (int)1.2f;
			alloysToAdd = cargoShip.alloysCount;
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
		{
			StartCoroutine(ConstructNewCargoShip());
		}
	}
	public IEnumerator ConstructNewCargoShip()
	{
		yield return new WaitForSeconds(3);
		GameObject go = Instantiate(cargoShipPrefab, vehSpawnLocation.transform.position, Quaternion.identity);
		CargoShipController script = go.GetComponent<CargoShipController>();
		script.refineryControllerParent = this;

		CargoShipList.Add(script);

		CheckCargoShipsCount();
	}
}
