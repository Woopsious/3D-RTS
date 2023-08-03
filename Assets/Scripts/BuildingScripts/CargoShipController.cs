using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.UI.CanvasScaler;

public class CargoShipController : UnitStateController
{
	[Header("Cargo Ship stats")]
	public float viewRange;
	public int moveSpeed;
	public int maxMoveSpeed;
	public int turnSpeed;

	public int maxCrystalCarryCapacity;
	public int maxAlloyCarryCapacity;
	public int alloysCount;
	public int crystalsCount;

	public bool canChangeOrders;
	public bool hasNewOrders;
	public bool hasPauseOperation;

	[Header("Dynamic Refs")]
	public RefineryController refineryControllerParent;
	public ResourceNodes targetResourceNode;
	public ResourceNodes playerSetResourceNode;

	public override void Start()
	{
		base.Start();
		Debug.LogWarning("CargoShip StartUp 1");
		FindClosestTargetResourcesNodeServerRPC(GetComponent<NetworkObject>().NetworkObjectId);
		Debug.LogWarning("CargoShip StartUp 2");
	}
	public override void FixedUpdate()
	{
		if (targetResourceNode != null)
			transform.position = Vector3.MoveTowards(transform.position, movePos, moveSpeed * Time.deltaTime);
	}

	//FUNCTIONS FOR LOOPING RESOURCE GATHERING
	//can chnage mine orders here
	public IEnumerator IncreaseHeightFromRefinery()
	{
		canChangeOrders = true;
		SetDestination(new Vector3(refineryControllerParent.transform.position.x, 22, refineryControllerParent.transform.position.z));

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);

		StartCoroutine(MoveToResourceNode());
	}
	public IEnumerator MoveToResourceNode()
	{
		canChangeOrders = true;
		SetDestination(new Vector3(targetResourceNode.transform.position.x, 22, targetResourceNode.transform.position.z));

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);

		StartCoroutine(DecreaseHeightToResourceNode());
	}
	public IEnumerator DecreaseHeightToResourceNode()
	{
		canChangeOrders = true;
		SetDestination(new Vector3(targetResourceNode.transform.position.x, targetResourceNode.transform.position.y + 4f,
			targetResourceNode.transform.position.z));

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);

		StartCoroutine(MineResourceNodeAndReturnToRefinery());
	}
	//cant change mine orders here, and will have to wait
	public IEnumerator MineResourceNodeAndReturnToRefinery()
	{
		canChangeOrders = false;
		yield return new WaitForSeconds(15);                                                                                //mine resources for 15s
		MineResourcesFromNodeServerRPC();

		if (CheckIfNodeIsEmpty()) //check to make sure resources are still avalable
			FindClosestTargetResourcesNodeServerRPC(GetComponent<NetworkObject>().NetworkObjectId); //if not find new closest resource node from parent refinery

		SetDestination(new Vector3(gameObject.transform.position.x, 22, gameObject.transform.position.z));

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);				//increase height from current pos + check pos till true

		SetDestination(new Vector3(refineryControllerParent.transform.position.x - 2.9f, 22, refineryControllerParent.transform.position.z - 0.9f));

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);				//move above refinery keeping height + check pos till true

		SetDestination(new Vector3(refineryControllerParent.transform.position.x - 2.9f, refineryControllerParent.transform.position.y + 6,
			refineryControllerParent.transform.position.z - 0.9f));

		RefineResourcesFromInventroy();                                                                 //drop off resources at refinery and wait 5s
		yield return new WaitForSeconds(5);

		if (hasNewOrders)
			ChangeResourceNodeServerRPC();																	//if neworders are avalable switch to them here

		if (!hasPauseOperation) //continue loop if not paused (parent refinery is unpowered)
			StartCoroutine(IncreaseHeightFromRefinery());
	}
	//only used when orders changed
	public IEnumerator IncreaseHeight()
	{
		canChangeOrders = true;
		SetDestination(new Vector3(gameObject.transform.position.x, 22, gameObject.transform.position.z));

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);

		StartCoroutine(MoveToResourceNode());
	}

	//function to find res nodes
	[ServerRpc(RequireOwnership = false)]
	public void SetResourceNodeFromPlayerInputServerRPC(ulong resourceNodeObjId)
	{
		SetResourceNodeFromPlayerInputClientRPC(GetComponent<NetworkObject>().NetworkObjectId, resourceNodeObjId);
	}
	[ClientRpc]
	public void SetResourceNodeFromPlayerInputClientRPC(ulong cargoShipObjId, ulong resourceNodeObjId)
	{
		CargoShipController cargoShip = NetworkManager.Singleton.SpawnManager.SpawnedObjects[resourceNodeObjId].GetComponent<CargoShipController>();
		playerSetResourceNode = NetworkManager.Singleton.SpawnManager.SpawnedObjects[resourceNodeObjId].GetComponent<ResourceNodes>();

		if (cargoShip.canChangeOrders)
		{
			cargoShip.ChangeResourceNodeServerRPC();
			cargoShip.StopAllCoroutines();
			cargoShip.StartCoroutine(IncreaseHeight());
		}
		else
			cargoShip.hasNewOrders = true;
	}
	[ServerRpc(RequireOwnership = false)]
	public void ChangeResourceNodeServerRPC()
	{
		targetResourceNode.isBeingMined.Value = false;
		targetResourceNode = playerSetResourceNode;
		targetResourceNode.isBeingMined.Value = true;
		hasNewOrders = false;
	}
	[ServerRpc(RequireOwnership = false)]
	public void FindClosestTargetResourcesNodeServerRPC(ulong cargoShipNetworkObjId)
	{
		Debug.LogWarning("CargoShip Find Nodes Server");
		FindClosestTargetResourcesNodeClientRPC(cargoShipNetworkObjId);
	}
	[ClientRpc]
	public void FindClosestTargetResourcesNodeClientRPC(ulong cargoShipNetworkObjId)
	{
		CargoShipController cargoShip = NetworkManager.SpawnManager.SpawnedObjects[cargoShipNetworkObjId].GetComponent<CargoShipController>();
		List<ResourceNodes> PossibleNodes = new List<ResourceNodes>();

		Debug.LogWarning("Refinery List of Resource Nodes: " + cargoShip.refineryControllerParent.resourceNodesList);
		Debug.LogWarning("List of Resource Nodes: " + PossibleNodes);

		foreach (ResourceNodes resourceNode in cargoShip.refineryControllerParent.resourceNodesList)
		{
			if (!resourceNode.isBeingMined.Value && !resourceNode.isEmpty.Value)
				PossibleNodes.Add(resourceNode);
		}

		if (PossibleNodes.Count == 0)
			Debug.LogWarning("No Free Resource Nodes");

		else
		{
			PossibleNodes = PossibleNodes.OrderBy(newtarget => Vector3.Distance(cargoShip.refineryControllerParent.transform.position,
				newtarget.transform.position)).ToList();

			cargoShip.targetResourceNode = PossibleNodes[0];
			cargoShip.targetResourceNode.isBeingMined.Value = true;

			Debug.LogWarning("Closest Resource Node: " + PossibleNodes[0]);
		}

		cargoShip.StartCoroutine(IncreaseHeightFromRefinery());
	}

	//HEALTH/HIT FUNCTIONS OVERRIDES
	public override void TryDisplayEntityHitNotif()
	{
		if (!wasRecentlyHit && ShouldDisplaySpottedNotifToPlayer())
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("CARGOSHIP UNDER ATTACK", transform.position);
	}
	public override void OnEntityDeath()
	{
		base.OnEntityDeath();
		if (ShouldDisplaySpottedNotifToPlayer())
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("CARGOSHIP DESTROYED", transform.position);
	}

	//UTILITY FUNCTIONS
	public override void RemoveEntityRefs()
	{
		targetResourceNode.isBeingMined.Value = false;
		playerController.unitListForPlayer.Remove(this);
		refineryControllerParent.CargoShipList.Remove(this);
		refineryControllerParent.CheckCargoShipsCount();
	}
	[ServerRpc(RequireOwnership = false)]
	public void MineResourcesFromNodeServerRPC()
	{
		if(targetResourceNode.isCrystalNode.Value)
		{
			if (targetResourceNode.resourcesAmount.Value > maxCrystalCarryCapacity)
			{
				crystalsCount = maxCrystalCarryCapacity;
				targetResourceNode.resourcesAmount.Value -= maxCrystalCarryCapacity;
			}
			else
			{
				crystalsCount = targetResourceNode.resourcesAmount.Value;
				targetResourceNode.resourcesAmount.Value -= targetResourceNode.resourcesAmount.Value;
			}
		}
		else
		{
			if (targetResourceNode.resourcesAmount.Value > alloysCount)
			{
				alloysCount = maxAlloyCarryCapacity;
				targetResourceNode.resourcesAmount.Value -= maxAlloyCarryCapacity;
			}
			else
			{
				alloysCount = targetResourceNode.resourcesAmount.Value;
				targetResourceNode.resourcesAmount.Value -= targetResourceNode.resourcesAmount.Value;
			}
		}
		targetResourceNode.CheckResourceCountServerRpc();
	}
	public void RefineResourcesFromInventroy()
	{
		refineryControllerParent.RefineResources(this);
		alloysCount = 0;
		crystalsCount = 0;
	}
	public void SetDestination(Vector3 moveDestination)
	{
		movePos = moveDestination;
	}
	public void PauseMining()
	{
		StopAllCoroutines();
		hasPauseOperation = true;
		SetDestination(new Vector3(refineryControllerParent.transform.position.x, 22, refineryControllerParent.transform.position.z));
	}
	public void ContinueMining()
	{
		hasPauseOperation = false;
		if (crystalsCount != 0 || alloysCount != 0)
			StartCoroutine(MineResourceNodeAndReturnToRefinery());

		else
			StartCoroutine(MoveToResourceNode());
	}
	public void DeleteSelf()
	{
		currentHealth.Value = -10;
		OnEntityDeath();
	}

	//BOOL CHECKS
	public bool CheckIfNodeIsEmpty()
	{
		if (targetResourceNode.isEmpty.Value)
			return true;
		else return false;
	}
	public bool CheckIfInPosition(Vector3 moveDestination)
	{
		float Distance = Vector3.Distance(gameObject.transform.position, moveDestination);

		if (Distance <= 0.1)
			return true;
		else return false;
	}
}
