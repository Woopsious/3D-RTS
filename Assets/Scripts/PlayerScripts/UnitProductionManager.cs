using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UnitProductionManager : MonoBehaviour
{
	[Header("Game Ui + Refs")]
	public PlayerController playerController;

	public GameObject vehicleProdInfoTemplateObj;

	[Header("Light Veh Queue Refs")]
	public GameObject lightVehProdQueueObj;
	public List<BuildTime> lightVehProdList;

	[Header("Heavy Veh Queue Refs")]
	public GameObject heavyVehProdQueueObj;
	public List<BuildTime> heavyVehProdList;

	[Header("VTOL Veh Queue Refs")]
	public GameObject vtolVehProdQueueObj;
	public List<BuildTime> vtolVehProdList;

	[Header("Player Unit Prefabs")]
	public GameObject unitScoutVehicle;
	public GameObject unitRadarVehicle;
	public GameObject unitLightMech;
	public GameObject unitHeavyMechKnight;
	public GameObject unitHeavyMechTank;
	public GameObject unitVTOL;

	[Header("Base Vehicle Build Times")]
	public float vtolShipBuildTime = 120f;
	public float heavyMechKnightBuildTime = 100f;
	public float heavyMechTankBuildTime = 90f;
	public float lightMechBuildTime = 60f;
	public float radarVehicleBuildTime = 75f;
	public float scoutVehicleBuildTime = 30f;

	public List<BuildTime> currentUnitPlacements;
	public List<BuildTime> failedUnitPlacements;

	public GameObject unitBuildHighlighterParent;
	public List<GameObject> unitPlacementPoints = new List<GameObject>();

	public void Start()
	{
		if(playerController.isPlayerOne)
		{
			AssignUnitRefs(GameManager.Instance.PlayerOneUnitsList);
		}
		if(!playerController.isPlayerOne)
		{
			AssignUnitRefs(GameManager.Instance.PlayerTwoUnitsList);
		}
	}
	public void AssignUnitRefs(List<GameObject> unitlist)
	{
		foreach (GameObject obj in unitlist)
		{
			UnitStateController unit = obj.GetComponent<UnitStateController>();
			if (unit.entityName == "Scout Vehicle")
				unitScoutVehicle = unit.gameObject;
			if (unit.entityName == "Radar Vehicle")
				unitRadarVehicle = unit.gameObject;
			if (unit.entityName == "Light Mech")
				unitLightMech = unit.gameObject;
			if (unit.entityName == "Heavy Mech Knight")
				unitHeavyMechKnight = unit.gameObject;
			if (unit.entityName == "Heavy Mech Support")
				unitHeavyMechTank = unit.gameObject;
			if (unit.entityName == "VTOL Gunship")
				unitVTOL = unit.gameObject;
		}
	}
	public void Update()
	{
		ShowUnitBuildGhostProjections();

		PlaceUnitManager();
	}
	public void ShowUnitBuildGhostProjections()
	{
		if (playerController.unitProductionManager.currentUnitPlacements.Count != 0)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit hitInfo, 250f, playerController.ignoreMe))
			{
				unitBuildHighlighterParent.transform.position = hitInfo.point;
				//not working properly
				float mouseWheelRotation = Input.mouseScrollDelta.y;
				unitBuildHighlighterParent.transform.Rotate(10 * mouseWheelRotation * Vector3.up);
				if (currentUnitPlacements.Count < 6)
				{
					for (int i = 0; i < currentUnitPlacements.Count; i++)
					{
						if (!unitPlacementPoints[i].activeSelf)
						{
							unitPlacementPoints[i].SetActive(true);
						}
					}
				}
				else if (currentUnitPlacements.Count > 6)
				{
					for (int i = 0; i < 5; i++)
					{
						if (!unitPlacementPoints[i].activeSelf)
						{
							unitPlacementPoints[i].SetActive(true);
						}
					}
				}
			}
		}
	}
	//possibly split function up in future as its messy
	public void PlaceUnitManager()
	{
		//on left click add all builds to correct lists + check if player can afford them
		if (Input.GetMouseButtonDown(0) && currentUnitPlacements.Count != 0)
		{
			foreach (BuildTime build in currentUnitPlacements)
			{
				//run check to see if a building of the correct type for a unit is built in future
				build.buildPosDestination = unitBuildHighlighterParent.transform.position;

				if(build.FindClosestProdBuilding())
				{
					UnitStateController broughtUnit = build.UnitPrefab.GetComponent<UnitStateController>();

					if (CheckIfCanBuy(broughtUnit.moneyCost, broughtUnit.alloyCost, broughtUnit.crystalCost)) //if player can afford them 
					{
						//then -unit prices and add to correct queue list and start production on first one if not already started, then update resUI
						UnitCost(broughtUnit.moneyCost, broughtUnit.alloyCost, broughtUnit.crystalCost);

						if (build.listNumRef == 1)
						{
							lightVehProdList.Add(build);
							if (!lightVehProdList[0].isInProduction)
								lightVehProdList[0].StartProduction();
						}

						else if (build.listNumRef == 2)
						{
							heavyVehProdList.Add(build);
							if (!heavyVehProdList[0].isInProduction)
								heavyVehProdList[0].StartProduction();
						}

						else if (build.listNumRef == 3)
						{
							vtolVehProdList.Add(build);
							if (!vtolVehProdList[0].isInProduction)
								vtolVehProdList[0].StartProduction();
						}
						GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Units Brought", 2f);
					}
					else //if player cant afford them add build to failed list then at the end remove ui + refs
					{
						failedUnitPlacements.Add(build);
						GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Couldnt Afford some/all unit(s)", 3f);
					}
				}
				else //if no valid spawn point for them is found
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("no powered vehicle production buildings found", 3f);
				}
			}
			if (failedUnitPlacements.Count != 0)
			{
				foreach (BuildTime failedBuild in failedUnitPlacements)
				{
					failedBuild.RemoveUi();
				}
				failedUnitPlacements.Clear();
			}

			foreach (GameObject obj in unitPlacementPoints)
			{
				if(obj.activeInHierarchy)
					obj.SetActive(false);
			}

			currentUnitPlacements.Clear();
		}
		//on right click remove last added build and build highlighter then remove ui + refs
		if (Input.GetMouseButtonDown(1) && currentUnitPlacements.Count != 0)
		{
			int i = currentUnitPlacements.Count - 1;
			currentUnitPlacements[i].RemoveUi();
			unitPlacementPoints[i].SetActive(false);
		}
	}

	//BUY UNIT FUNCTIONS (Called from Ui button or Hotkey in PlayerController), spawns ui, Ghost Projections and sets refs/stats
	//possible to add an if statement to switch prefab to playerTwo unit prefab based on is playerOne bool
	public void AddScoutVehToBuildQueue()
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, lightVehProdQueueObj.transform);
		BuildTime script = uiObj.GetComponent<BuildTime>();

		script.UnitPrefab = unitScoutVehicle;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 1;
		script.buildTime = UpdateBuildTime(scoutVehicleBuildTime, playerController.lightVehProdBuildingsList); ;

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddRadarVehToBuildQueue()
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, lightVehProdQueueObj.transform);
		BuildTime script = uiObj.GetComponent<BuildTime>();

		script.UnitPrefab = unitRadarVehicle;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 1;
		script.buildTime = UpdateBuildTime(radarVehicleBuildTime, playerController.lightVehProdBuildingsList); ;

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddLightMechToBuildQueue()
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, lightVehProdQueueObj.transform);
		BuildTime script = uiObj.GetComponent<BuildTime>();

		script.UnitPrefab = unitLightMech;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 1;
		script.buildTime = UpdateBuildTime(lightMechBuildTime, playerController.lightVehProdBuildingsList);

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddHeavyMechKnightToBuildQueue()
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, heavyVehProdQueueObj.transform);
		BuildTime script = uiObj.GetComponent<BuildTime>();

		script.UnitPrefab = unitHeavyMechKnight;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 2;
		script.buildTime = UpdateBuildTime(heavyMechKnightBuildTime, playerController.heavyVehProdBuildingsList);

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddHeavyMechTankToBuildQueue()
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, heavyVehProdQueueObj.transform);
		BuildTime script = uiObj.GetComponent<BuildTime>();

		script.UnitPrefab = unitHeavyMechTank;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 2;
		script.buildTime = UpdateBuildTime(heavyMechTankBuildTime, playerController.heavyVehProdBuildingsList);

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddVTOLToBuildQueue()
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, vtolVehProdQueueObj.transform);
		BuildTime script = uiObj.GetComponent<BuildTime>();

		script.UnitPrefab = unitVTOL;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 3;
		script.buildTime = UpdateBuildTime(vtolShipBuildTime, playerController.vtolVehProdBuildingsList);

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}

	//SPAWN BUILT UNITS FUNCTIONS
	public void SpawnUnitsAtProdBuilding(BuildTime buildOrder, VehProdSpawnLocation VehSpawnLocation, Vector3 destination)
	{
		StartCoroutine(OpenCloseDoors(VehSpawnLocation));
		GameObject go = Instantiate(buildOrder.UnitPrefab, VehSpawnLocation.vehProdSpawnPoint.transform.position, Quaternion.identity);

		playerController.gameUIManager.techTreeManager.ApplyTechUpgradesToNewUnits(go);
		StartCoroutine(ChangeBuiltUnitState(go.GetComponent<UnitStateController>(), destination));
	}
	public IEnumerator ChangeBuiltUnitState(UnitStateController newUnit, Vector3 destination)
	{
		yield return new WaitForSeconds(0.1f);
		newUnit.movePos = destination;
		newUnit.ChangeStateMoving();
	}
	public IEnumerator OpenCloseDoors(VehProdSpawnLocation VehSpawnLocation)
	{
		VehSpawnLocation.spawnAnim[0].SetBool("Open", true);
		yield return new WaitForSeconds(3);
		VehSpawnLocation.spawnAnim[0].SetBool("Open", false);
	}

	//QUEUE UPDATE FUNCTIONS
	//once first production is done remove it, (hopefully index 1 will now be index 0) and then if condition met start new index 0 production
	public void RemoveFromQueueAndStartNextBuild(List<BuildTime> list, BuildTime buildNum)
	{
		list.Remove(buildNum);

		if (list.Count != 0 && !list[0].isInProduction)
			list[0].StartProduction();
	}

	//CHECK PRICE
	public float UpdateBuildTime(float baseBuildTime, List<BuildingManager> list)
	{
		float buildTimeReduction = 0;

		foreach (BuildingManager building in list)
		{
			buildTimeReduction += building.unitBuildTimeBoost;
		}

		baseBuildTime -= buildTimeReduction;

		if (baseBuildTime < 10)
			baseBuildTime = 10;

		return baseBuildTime;
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
	public void UnitCost(int moneyCost, int alloyCost, int crystalCost)
	{
		GameManager.Instance.playerOneCurrentMoney -= moneyCost;
		GameManager.Instance.playerOneCurrentAlloys -= alloyCost;
		GameManager.Instance.playerOneCurrentCrystals -= crystalCost;

		playerController.gameUIManager.UpdateCurrentResourcesUI();
	}
}
