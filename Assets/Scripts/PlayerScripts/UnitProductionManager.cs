using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UnitProductionManager : NetworkBehaviour
{
	[Header("Game Ui + Refs")]
	public PlayerController playerController;

	public GameObject vehicleProdInfoTemplateObj;

	[Header("Light Veh Queue Refs")]
	public GameObject lightVehProdQueueObj;
	public List<UnitBuildManager> lightVehProdList;

	[Header("Heavy Veh Queue Refs")]
	public GameObject heavyVehProdQueueObj;
	public List<UnitBuildManager> heavyVehProdList;

	[Header("VTOL Veh Queue Refs")]
	public GameObject vtolVehProdQueueObj;
	public List<UnitBuildManager> vtolVehProdList;

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

	public List<UnitBuildManager> currentUnitPlacements;
	public List<UnitBuildManager> failedUnitPlacements;

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
			foreach (UnitBuildManager build in currentUnitPlacements)
			{
				//run check to see if a building of the correct type for a unit is built in future
				build.buildPosDestination = unitBuildHighlighterParent.transform.position;
				build.isPlayerOne = playerController.isPlayerOne;

				if (build.FindClosestProdBuilding())
				{
					UnitStateController broughtUnit = build.UnitPrefab.GetComponent<UnitStateController>();
					//if player can afford them
					if (playerController.CheckIfCanBuyEntity(broughtUnit.moneyCost, broughtUnit.alloyCost, broughtUnit.crystalCost))
					{
						//then -unit prices and add to correct queue list and start production on first one if not already started, then update resUI
						playerController.EntityCostServerRPC(playerController.isPlayerOne, 
							broughtUnit.moneyCost, broughtUnit.alloyCost, broughtUnit.crystalCost);

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
				foreach (UnitBuildManager failedBuild in failedUnitPlacements)
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
	public void AddScoutVehToBuildQueue(int buildIndex)
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, lightVehProdQueueObj.transform);
		UnitBuildManager script = uiObj.GetComponent<UnitBuildManager>();

		script.UnitPrefab = unitScoutVehicle;
		script.BuildOrderIndex = buildIndex;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 1;
		script.buildTime = UpdateBuildTime(scoutVehicleBuildTime, playerController.lightVehProdBuildingsList); ;

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddRadarVehToBuildQueue(int buildIndex)
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, lightVehProdQueueObj.transform);
		UnitBuildManager script = uiObj.GetComponent<UnitBuildManager>();

		script.UnitPrefab = unitRadarVehicle;
		script.BuildOrderIndex = buildIndex;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 1;
		script.buildTime = UpdateBuildTime(radarVehicleBuildTime, playerController.lightVehProdBuildingsList); ;

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddLightMechToBuildQueue(int buildIndex)
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, lightVehProdQueueObj.transform);
		UnitBuildManager script = uiObj.GetComponent<UnitBuildManager>();

		script.UnitPrefab = unitLightMech;
		script.BuildOrderIndex = buildIndex;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 1;
		script.buildTime = UpdateBuildTime(lightMechBuildTime, playerController.lightVehProdBuildingsList);

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddHeavyMechKnightToBuildQueue(int buildIndex)
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, heavyVehProdQueueObj.transform);
		UnitBuildManager script = uiObj.GetComponent<UnitBuildManager>();

		script.UnitPrefab = unitHeavyMechKnight;
		script.BuildOrderIndex = buildIndex;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 2;
		script.buildTime = UpdateBuildTime(heavyMechKnightBuildTime, playerController.heavyVehProdBuildingsList);

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddHeavyMechTankToBuildQueue(int buildIndex)
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, heavyVehProdQueueObj.transform);
		UnitBuildManager script = uiObj.GetComponent<UnitBuildManager>();

		script.UnitPrefab = unitHeavyMechTank;
		script.BuildOrderIndex = buildIndex;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 2;
		script.buildTime = UpdateBuildTime(heavyMechTankBuildTime, playerController.heavyVehProdBuildingsList);

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}
	public void AddVTOLToBuildQueue(int buildIndex)
	{
		GameObject uiObj = Instantiate(vehicleProdInfoTemplateObj, vtolVehProdQueueObj.transform);
		UnitBuildManager script = uiObj.GetComponent<UnitBuildManager>();

		script.UnitPrefab = unitVTOL;
		script.BuildOrderIndex = buildIndex;
		script.unitProductionManager = this;
		script.isPlayerOne = playerController.isPlayerOne;
		script.listNumRef = 3;
		script.buildTime = UpdateBuildTime(vtolShipBuildTime, playerController.vtolVehProdBuildingsList);

		currentUnitPlacements.Add(script);
		playerController.gameUIManager.ShowUnitProdQueuesWhenBuyingUnit();
	}

	//SPAWN BUILT UNITS FUNCTIONS
	public void SpawnUnitsAtProdBuilding(int buildOrderIndex, ulong buildingNetworkObjId, Vector3 destination)
	{
		SpawnUnitsAtProdBuildingServerRPC(buildOrderIndex, buildingNetworkObjId, destination);
	}
	public IEnumerator ChangeBuiltUnitState(ulong NetworkUnitId, Vector3 destination)
	{
		yield return new WaitForSeconds(0.1f);
		playerController.unitSelectionManager.MoveUnitsServerRPC(NetworkUnitId, destination);
	}
	public IEnumerator OpenCloseDoors(VehProdSpawnLocation VehSpawnLocation)
	{
		VehSpawnLocation.spawnAnim[0].SetBool("Open", true);
		yield return new WaitForSeconds(3);
		VehSpawnLocation.spawnAnim[0].SetBool("Open", false);
	}

	//QUEUE UPDATE FUNCTIONS
	//once first production is done remove it, (index 1 should be index 0 now) and then if condition met start new index 0 production
	public void RemoveFromQueueAndStartNextBuild(List<UnitBuildManager> list, UnitBuildManager buildNum)
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

	//server spawns unit in
	[ServerRpc(RequireOwnership = false)]
	public void SpawnUnitsAtProdBuildingServerRPC(int buildOrderIndex, ulong buildingNetworkObjId, Vector3 destination,
		ServerRpcParams serverRpcParams = default)
	{
		ulong clientId = serverRpcParams.Receive.SenderClientId;
		if (!IsServer) return;

		VehProdSpawnLocation spawnLocationScript = NetworkManager.Singleton.SpawnManager.SpawnedObjects[buildingNetworkObjId].
			GetComponent<VehProdSpawnLocation>();
		Vector3 vehSpawnPos = spawnLocationScript.vehProdSpawnPoint.transform.position;

		if (clientId == 0)
		{
			StartCoroutine(OpenCloseDoors(spawnLocationScript));
			GameObject obj = Instantiate(GameManager.Instance.PlayerOneUnitsList[buildOrderIndex], vehSpawnPos, Quaternion.identity);
			obj.GetComponent<NetworkObject>().Spawn(true);
			ApplyTechUpgradesToNewUnitsServerRPC(obj.GetComponent<NetworkObject>().NetworkObjectId);
			StartCoroutine(ChangeBuiltUnitState(obj.GetComponent<NetworkObject>().NetworkObjectId, destination));
		}
		else if (clientId == 1)
		{
			StartCoroutine(OpenCloseDoors(spawnLocationScript));
			GameObject obj = Instantiate(GameManager.Instance.PlayerTwoUnitsList[buildOrderIndex], vehSpawnPos, Quaternion.identity);
			obj.GetComponent<NetworkObject>().Spawn(true);
			ApplyTechUpgradesToNewUnitsServerRPC(obj.GetComponent<NetworkObject>().NetworkObjectId);
			StartCoroutine(ChangeBuiltUnitState(obj.GetComponent<NetworkObject>().NetworkObjectId, destination));
		}
	}
	[ServerRpc(RequireOwnership = false)]
	public void ApplyTechUpgradesToNewUnitsServerRPC(ulong unitNetworkedId)
	{
		UnitStateController unit = NetworkManager.SpawnManager.SpawnedObjects[unitNetworkedId].GetComponent<UnitStateController>();

		if (unit.isPlayerOneEntity) //apply tech upgrades to player one units using player one bonus tech
		{
			unit.currentHealth.Value = (int)(unit.currentHealth.Value * 
				playerController.gameUIManager.gameManager.playerOneUnitHealthPercentageBonus.Value);
			unit.maxHealth.Value = (int)(unit.maxHealth.Value * 
				playerController.gameUIManager.gameManager.playerOneUnitHealthPercentageBonus.Value);
			unit.armour.Value = (int)(unit.armour.Value * 
				playerController.gameUIManager.gameManager.playerOneUnitArmourPercentageBonus.Value);

			if (unit.isUnitArmed)
			{
				unit.weaponSystem.mainWeaponDamage.Value *= 
					playerController.gameUIManager.gameManager.playerOneUnitDamagePercentageBonus.Value;
				unit.weaponSystem.secondaryWeaponDamage.Value *= 
					playerController.gameUIManager.gameManager.playerOneUnitDamagePercentageBonus.Value;
				unit.attackRange.Value += playerController.gameUIManager.gameManager.playerOneUnitAttackRangeBonus.Value;
				if (!unit.isTurret)
					unit.agentNav.speed += playerController.gameUIManager.gameManager.playerOneUnitSpeedBonus.Value;
			}
		}
		if (!unit.isPlayerOneEntity) //apply tech upgrades to player two units using player two bonus tech
		{
			unit.currentHealth.Value = (int)(unit.currentHealth.Value * 
				playerController.gameUIManager.gameManager.playerTwoUnitHealthPercentageBonus.Value);
			unit.maxHealth.Value = (int)(unit.maxHealth.Value * 
				playerController.gameUIManager.gameManager.playerTwoUnitHealthPercentageBonus.Value);
			unit.armour.Value = (int)(unit.armour.Value * 
				playerController.gameUIManager.gameManager.playerTwoUnitArmourPercentageBonus.Value);

			if (unit.isUnitArmed)
			{
				unit.weaponSystem.mainWeaponDamage.Value *= 
					playerController.gameUIManager.gameManager.playerTwoUnitDamagePercentageBonus.Value;
				unit.weaponSystem.secondaryWeaponDamage.Value *= 
					playerController.gameUIManager.gameManager.playerTwoUnitDamagePercentageBonus.Value;
				unit.attackRange.Value += playerController.gameUIManager.gameManager.playerTwoUnitAttackRangeBonus.Value;
				if (!unit.isTurret)
					unit.agentNav.speed += playerController.gameUIManager.gameManager.playerTwoUnitSpeedBonus.Value;
			}
		}
	}

	//Refund canceled unit production
	[ServerRpc(RequireOwnership = false)]
	public void UnitRefundCostServerRPC(bool wasPlayerOneEntity, int moneyCost, int alloyCost, int crystalCost)
	{
		if (wasPlayerOneEntity)
		{
			GameManager.Instance.playerOneCurrentMoney.Value += moneyCost;
			GameManager.Instance.playerOneCurrentAlloys.Value += alloyCost;
			GameManager.Instance.playerOneCurrentCrystals.Value += crystalCost;
		}
		else if (!wasPlayerOneEntity)
		{
			GameManager.Instance.playerTwoCurrentMoney.Value += moneyCost;
			GameManager.Instance.playerTwoCurrentAlloys.Value += alloyCost;
			GameManager.Instance.playerTwoCurrentCrystals.Value += crystalCost;
		}

		StartCoroutine(GameManager.Instance.gameUIManager.UpdateCurrentResourcesUI(1f));
	}
}
