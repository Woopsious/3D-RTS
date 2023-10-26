using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UnitBuildManager : MonoBehaviour
{
	public Button cancelProductionButton;
	public UnitProductionManager unitProductionManager;
	public Vector3 buildPosDestination;
	public bool isPlayerOne;

	public Text buildCostText;
	public Text buildTimeText;

	public GameObject UnitPrefab;
	public int BuildOrderIndex;
	public int listNumRef;
	public float buildTime;
	public float buildTimer;
	public bool isInProduction;
	public bool isSpawnPointStillValid;

	public VehProdSpawnLocation unitSpawnLocation;

	public void Start()
	{
		UnitStateController unit = UnitPrefab.GetComponent<UnitStateController>();
		buildCostText.text = unit.entityName + "\n Cost: " + unit.moneyCost + " Money\n" + unit.alloyCost + " Alloys, " + unit.crystalCost + " Crystals";
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
	public bool FindClosestProdBuilding()
	{
		VehProdSpawnLocation[] allVehProdbuildings = FindObjectsOfType<VehProdSpawnLocation>();
		List<VehProdSpawnLocation> correctProdBuildings = new List<VehProdSpawnLocation>();


		if (allVehProdbuildings.Length != 0)
		{
			foreach (VehProdSpawnLocation possibleBuilding in allVehProdbuildings)
			{
				if (isPlayerOne == possibleBuilding.building.isPlayerOneEntity && possibleBuilding.building.isPowered)
				{
					if (possibleBuilding.building.isLightVehProdBuilding && listNumRef == 1)
						correctProdBuildings.Add(possibleBuilding);

					else if (possibleBuilding.building.isHeavyVehProdBuilding && listNumRef == 2)
						correctProdBuildings.Add(possibleBuilding);

					else if (possibleBuilding.building.isVTOLProdBuilding && listNumRef == 3)
						correctProdBuildings.Add(possibleBuilding);
				}
			}
		}
		if (correctProdBuildings.Count == 0 || allVehProdbuildings.Length == 0)
		{
			unitProductionManager.failedUnitPlacements.Add(this);
			return false;
		}
		else
		{
			List<VehProdSpawnLocation> closestProdBuildings = correctProdBuildings.OrderBy(newBuilding => Vector3.Distance(buildPosDestination,
				newBuilding.gameObject.transform.position)).ToList();

			VehProdSpawnLocation closestBuilding = closestProdBuildings[0];
			unitSpawnLocation = closestBuilding;
			return true;
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

		//check if spawnpoint still valid if not, try find new one, if failed cancel
		if (unitSpawnLocation == null)
		{
			isSpawnPointStillValid = false;
			FindClosestProdBuilding();
		}

		if (unitSpawnLocation != null)
		{
			unitProductionManager.SpawnUnitsAtProdBuilding(BuildOrderIndex, 
				unitSpawnLocation.gameObject.GetComponent<NetworkObject>().NetworkObjectId, buildPosDestination);
			RemoveUi();
		}
		else
			CancelProduction();
	}
	public void CancelProduction()
	{
		UnitStateController unit = UnitPrefab.GetComponent<UnitStateController>();
		GameManager.Instance.UpdateResourcesServerRPC(isPlayerOne, false, true, false, 0, unit.moneyCost, unit.alloyCost, unit.crystalCost);

		if (isSpawnPointStillValid)
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("unit production canceled", 1f);
		else if (!isSpawnPointStillValid)
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("unit production canceled, no valid spawn point", 3f);
		RemoveUi();
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
}
