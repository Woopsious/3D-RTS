using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		FindClosestTargetResourcesNode();

		if (refineryControllerParent.building.isPowered)
			StartCoroutine(IncreaseHeightFromRefinery());
		else
			PauseMining();
	}
	public override void FixedUpdate()
	{
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
		MineResourcesFromNode();

		if (CheckIfNodeIsEmpty()) //check to make sure resources are still avalable
			FindClosestTargetResourcesNode(); //if not find new closest resource node from parent refinery

		SetDestination(new Vector3(gameObject.transform.position.x, 22, gameObject.transform.position.z));

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);				//increase height from current pos + check pos till true

		SetDestination(new Vector3(refineryControllerParent.transform.position.x - 2.9f, 22, refineryControllerParent.transform.position.z - 0.9f));

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);				//move above refinery keeping height + check pos till true

		SetDestination(new Vector3(refineryControllerParent.transform.position.x - 2.9f, refineryControllerParent.transform.position.y + 6,
			refineryControllerParent.transform.position.z - 0.9f));

		RefineResourcesFromInventroy();                                                                 //drop off resources at refinery and wait 5s
		yield return new WaitForSeconds(5);

		if (hasNewOrders)
			ChangeResourceNode();																	//if neworders are avalable switch to them here

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
	public void SetResourceNodeFromPlayerInput(ResourceNodes resourceNode)
	{
		playerSetResourceNode = resourceNode;

		if (canChangeOrders)
		{
			ChangeResourceNode();
			StopAllCoroutines();
			StartCoroutine(IncreaseHeight());
		}
		else
			hasNewOrders = true;
	}
	public void ChangeResourceNode()
	{
		targetResourceNode.isBeingMined = false;
		targetResourceNode = playerSetResourceNode;
		targetResourceNode.isBeingMined = true;
		hasNewOrders = false;
	}
	public void FindClosestTargetResourcesNode()
	{
		List<ResourceNodes> PossibleNodes = new List<ResourceNodes>();

		foreach (ResourceNodes resourceNode in refineryControllerParent.resourceNodesList)
		{
			if (!resourceNode.isBeingMined && !resourceNode.isEmpty)
				PossibleNodes.Add(resourceNode);
		}

		if(PossibleNodes.Count == 0)
			Debug.LogError("No Free Resource Nodes");

		else
		{
			PossibleNodes = PossibleNodes.OrderBy(newtarget => Vector3.Distance(refineryControllerParent.transform.position,
				newtarget.transform.position)).ToList();

			targetResourceNode = PossibleNodes[0];
			targetResourceNode.isBeingMined = true;
		}
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
		targetResourceNode.isBeingMined = false;
		playerController.unitListForPlayer.Remove(this);
		refineryControllerParent.CargoShipList.Remove(this);
		refineryControllerParent.CheckCargoShipsCount();
	}
	public void MineResourcesFromNode()
	{
		if(targetResourceNode.isCrystalNode)
		{
			if (targetResourceNode.resourcesAmount > maxCrystalCarryCapacity)
			{
				crystalsCount = maxCrystalCarryCapacity;
				targetResourceNode.resourcesAmount -= maxCrystalCarryCapacity;
			}
			else
			{
				crystalsCount = targetResourceNode.resourcesAmount;
				targetResourceNode.resourcesAmount -= targetResourceNode.resourcesAmount;
			}
		}
		else
		{
			if (targetResourceNode.resourcesAmount > alloysCount)
			{
				alloysCount = maxAlloyCarryCapacity;
				targetResourceNode.resourcesAmount -= maxAlloyCarryCapacity;
			}
			else
			{
				alloysCount = targetResourceNode.resourcesAmount;
				targetResourceNode.resourcesAmount -= targetResourceNode.resourcesAmount;
			}
		}
		targetResourceNode.CheckResourceCount();
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
		currentHealth = -10;
		OnEntityDeath();
	}

	//BOOL CHECKS
	public bool CheckIfNodeIsEmpty()
	{
		if (targetResourceNode.isEmpty)
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
