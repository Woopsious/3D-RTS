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

	[Header("Dynamic Refs")]
	public RefineryController refineryControllerParent;
	public ResourceNodes targetResourceNode;

	public override void Start()
	{       
		//assign correct playercontroller to unit on start
		PlayerController[] controllers = FindObjectsOfType<PlayerController>();
		foreach (PlayerController controller in controllers)
		{
			if (controller.isPlayerOne == isPlayerOneEntity)
			{
				playerController = controller;
				playerController.unitListForPlayer.Add(this);
			}
			else if (controller.isPlayerOne == !isPlayerOneEntity)
			{
				playerController = controller;
				playerController.unitListForPlayer.Add(this);
			}
		}

		UiObj.transform.SetParent(FindObjectOfType<GameUIManager>().gameObject.transform);
		UiObj.SetActive(false);
		UpdateHealthBar();
		UiObj.transform.rotation = Quaternion.identity;

		FindTargetResourcesNode();
		StartCoroutine(MoveToResourceNode());
	}

	public override void Update()
	{
		transform.position = Vector3.MoveTowards(transform.position, movePos, moveSpeed * Time.deltaTime);
	}
	public override void FixedUpdate()
	{
		//do nothing
	}
	//logic to loop when mining resource node
	public IEnumerator MoveToResourceNode()
	{
		SetDestination(new Vector3(refineryControllerParent.transform.position.x, refineryControllerParent.transform.position.y + 10, 
			refineryControllerParent.transform.position.z));														//increase height from parent refinery

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);												//check pos till true

		SetDestination(new Vector3(targetResourceNode.transform.position.x, targetResourceNode.transform.position.y + 10,
			targetResourceNode.transform.position.z));															//move to resource node location

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);												//check pos till true

		StartCoroutine(MineResources());
	}
	public IEnumerator MineResources()
	{
		SetDestination(new Vector3(targetResourceNode.transform.position.x, targetResourceNode.transform.position.y + 5,
			targetResourceNode.transform.position.z));																//Decrease height to resource node

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);												//check pos till true

		MineResourcesFromNode();

		yield return new WaitForSeconds(15);																				//mine resources for 15s

		SetDestination(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 10,
			gameObject.transform.position.z));																				//increase height from pos

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);												//check pos till true

		if (CheckIfNodeIsEmpty()) //check to make sure resources are still avalable
		{
			FindTargetResourcesNode(); //if not find new closest resource node from parent refinery
		}

		StartCoroutine(ReturnToParentRefinery());
	}
	public IEnumerator ReturnToParentRefinery()
	{
		SetDestination(new Vector3(refineryControllerParent.transform.position.x, refineryControllerParent.transform.position.y + 10,
			refineryControllerParent.transform.position.z));										//once true move to its parent refinery building

		yield return new WaitUntil(() => CheckIfInPosition(movePos) == true);												//check pos till true

		SetDestination(new Vector3(refineryControllerParent.transform.position.x, refineryControllerParent.transform.position.y + 5,
			refineryControllerParent.transform.position.z));												//once true decrease height to parent refinery

		RefineResourcesFromInventroy();

		yield return new WaitForSeconds(5);																				//drop off resources at refinery

		StartCoroutine(MoveToResourceNode());
	}

	//Functions to find or set resource node to mine from
	public void SetResourceNodeFromPlayerInput(ResourceNodes resourceNode)
	{
		targetResourceNode.isBeingMined = false;
		targetResourceNode = resourceNode;
		targetResourceNode.isBeingMined = true;
	}
	public void FindTargetResourcesNode()
	{
		List<ResourceNodes> PossibleNodes = new List<ResourceNodes>();

		foreach (ResourceNodes resourceNode in refineryControllerParent.resourceNodesList)
		{
			if (!resourceNode.isBeingMined && !resourceNode.isEmpty)
			{
				PossibleNodes.Add(resourceNode);
			}
		}

		if(PossibleNodes.Count == 0)
		{
			Debug.Log("No Free Resource Nodes");
		}
		else
		{
			PossibleNodes = PossibleNodes.OrderBy(newtarget => Vector3.Distance(refineryControllerParent.transform.position,
				newtarget.transform.position)).ToList();

			targetResourceNode = PossibleNodes[0];
			targetResourceNode.isBeingMined = true;
		}
	}

	//Utility Functions
	public override void RemoveEntityRefs()
	{
		targetResourceNode.isBeingMined = false;
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

	//bool checks
	public bool CheckIfNodeIsEmpty()
	{
		if (targetResourceNode.isEmpty)
		{
			return true;
		}
		return false;
	}
	public bool CheckIfInPosition(Vector3 moveDestination)
	{
		float Distance = Vector3.Distance(gameObject.transform.position, moveDestination);

		if (Distance <= 0.1)
			return true;

		else return false;
	}
}
