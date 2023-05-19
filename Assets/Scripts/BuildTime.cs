using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BuildTime : MonoBehaviour
{
	public Button cancelProductionButton;
	public UnitProductionManager unitProductionManager;
	public Vector3 buildPosDestination;
	public bool isPlayerOne;

	public Text buildCostText;
	public Text buildTimeText;

	public GameObject UnitPrefab;
	public int listNumRef;
	public float buildTime;
	public float buildTimer;
	public bool isInProduction;

	public VehProdSpawnLocation unitSpawnLocation;

	public void Start()
	{
		UnitStateController unit = UnitPrefab.GetComponent<UnitStateController>();
		buildCostText.text = unit.unitName + "\n Cost: " + unit.moneyCost + " Money\n" + unit.alloyCost + " Alloys, " + unit.crystalCost + " Crystals";
		buildTimeText.text = "Time To Build: " + buildTime;
	}
	public void Update()
	{
		if (isInProduction)
			DisplayBuildTime();
	}

	public void DisplayBuildTime()
	{
		buildTimer -= Time.deltaTime;
		if (buildTimer <= buildTime)
		{
			buildTimer %= buildTime;
			buildTimeText.text = "Time To Build: " + buildTimer;
		}
	}
	public void FindClosestProdBuilding()
	{
		VehProdSpawnLocation[] allVehProdbuildings = FindObjectsOfType<VehProdSpawnLocation>();
		List<VehProdSpawnLocation> correctProdBuildings = new List<VehProdSpawnLocation>();

		if(allVehProdbuildings.Length == 0)
			Debug.LogError("No production buildings found");

		foreach (VehProdSpawnLocation possibleBuilding in allVehProdbuildings)
		{
			if (isPlayerOne == possibleBuilding.building.isPlayerOneBuilding && possibleBuilding.building.isPowered)
			{
				if (possibleBuilding.building.isLightVehProdBuilding && listNumRef == 1)
					correctProdBuildings.Add(possibleBuilding);

				else if (possibleBuilding.building.isHeavyVehProdBuilding && listNumRef == 2)
					correctProdBuildings.Add(possibleBuilding);

				else if (possibleBuilding.building.isVTOLProdBuilding && listNumRef == 3)
					correctProdBuildings.Add(possibleBuilding);
			}
		}
		if (correctProdBuildings.Count == 0)
		{
			Debug.LogError("No correct type production buildings powered");
			unitProductionManager.failedUnitPlacements.Add(this);
			//notify player
		}
		else
		{
			List<VehProdSpawnLocation> closestProdBuildings = correctProdBuildings.OrderBy(newBuilding => Vector3.Distance(buildPosDestination,
				newBuilding.gameObject.transform.position)).ToList();

			VehProdSpawnLocation closestBuilding = closestProdBuildings[0];
			unitSpawnLocation = closestBuilding;
		}
	}
	public void StartProduction()
	{
		StartCoroutine(ProductionTimer());
	}
	public IEnumerator ProductionTimer()
	{
		GetComponent<Image>().color = new Color(0, 255, 0, 0.1f);
		buildTimer = buildTime;
		isInProduction = true;
		yield return new WaitForSeconds(buildTime);

		//once removed from build queue start next build
		if (listNumRef == 1)
			unitProductionManager.RemoveFromQueueAndStartNextBuild(unitProductionManager.lightVehProdList, this);

		if (listNumRef == 2)
			unitProductionManager.RemoveFromQueueAndStartNextBuild(unitProductionManager.heavyVehProdList, this);

		if (listNumRef == 3)
			unitProductionManager.RemoveFromQueueAndStartNextBuild(unitProductionManager.vtolVehProdList, this);

		unitProductionManager.SpawnUnitsAtProdBuilding(this, unitSpawnLocation, buildPosDestination);
		RemoveUi();
	}
	public void CancelProduction()
	{
		RemoveUi();

		UnitStateController unit = UnitPrefab.GetComponent<UnitStateController>();
		UnitCost(unit.moneyCost, unit.alloyCost, unit.crystalCost);
	}
	public void RemoveUi()
	{
		if (listNumRef == 1)
			unitProductionManager.RemoveFromQueueAndStartNextBuild(unitProductionManager.lightVehProdList, this);

		if (listNumRef == 2)
			unitProductionManager.RemoveFromQueueAndStartNextBuild(unitProductionManager.heavyVehProdList, this);

		if (listNumRef == 3)
			unitProductionManager.RemoveFromQueueAndStartNextBuild(unitProductionManager.vtolVehProdList, this);

		unitProductionManager.currentUnitPlacements.Remove(this);
		Destroy(gameObject);
	}
	public void UnitCost(int moneyCost, int alloyCost, int crystalCost)
	{
		GameManager.Instance.playerOneCurrentMoney += moneyCost;
		GameManager.Instance.playerOneCurrentAlloys += alloyCost;
		GameManager.Instance.playerOneCurrentCrystals += crystalCost;
	}
}
