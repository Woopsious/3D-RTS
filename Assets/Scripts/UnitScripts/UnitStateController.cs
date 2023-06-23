using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

public class UnitStateController : Entities
{
	public LayerMask ignoreMe;
	public UnitBaseState currentState;
	public UnitIdleState idleState = new UnitIdleState();
	public UnitMovingState movingState = new UnitMovingState();
	public UnitStateAttacking attackState = new UnitStateAttacking();

	public WeaponSystem weaponSystem;

	[Header("Unit Refs")]
	public Animator animatorController;
	public AudioSource movingSFX;
	public NavMeshAgent agentNav;
	public Rigidbody rb;
	public GameObject FoVMeshObj;

	public bool isUnitArmed;
	public bool isFlying;
	public bool hasAnimation;
	public bool hasRadar;
	public bool isCargoShip;

	[Header("Unit Stat Refs")]
	public string unitName;
	public float attackRange;
	public float ViewRange;

	[Header("Unit Dynamic Refs")]
	public int GroupNum;

	public List<GameObject> targetList;
	public List<BuildingManager> buildingTargetList;
	public BuildingManager currentBuildingTarget;
	public List<UnitStateController> unitTargetList;
	public UnitStateController currentUnitTarget;
	public Vector3 targetPos;
	public Vector3 movePos;
	public NavMeshPath navMeshPath;

	RaycastHit raycastHit;

	public override void Start()
	{
		base.Start();

		ChangeStateIdle();
		//assign correct playercontroller to unit on start
		PlayerController[] controllers = FindObjectsOfType<PlayerController>();
		foreach(PlayerController controller in controllers)
		{
			if(controller.isPlayerOne == isPlayerOneEntity)
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
	}
	public override void Update()
	{
		base.Update();

		currentState.UpdateLogic(this);
	}
	public virtual void FixedUpdate()
	{
		currentState.UpdatePhysics(this);

		if (targetList.Count != 0 && isUnitArmed && !isCargoShip && currentState != attackState) //switch to attack state if targets found
			ChangeStateAttacking();

		else if (targetList.Count == 0 && currentState == attackState)
			ChangeStateIdle();
	}

	//SPOTTING SYSTEM FUNCTIONS
	public void AddTargetsOnFOVEnter(GameObject triggerObj) //filter out everything but enemy Entities
	{
		if (triggerObj.GetComponent<UnitStateController>() != null && isPlayerOneEntity != triggerObj.GetComponent<UnitStateController>().isPlayerOneEntity)
		{
			if (!unitTargetList.Contains(triggerObj.GetComponent<UnitStateController>()))
				targetList.Add(triggerObj);
		}
		else if (triggerObj.GetComponent<BuildingManager>() != null && isPlayerOneEntity != triggerObj.GetComponent<BuildingManager>().isPlayerOneEntity
			&& triggerObj.GetComponent<CanPlaceBuilding>().isPlaced)    //filter out non placed buildings
		{
			if (!buildingTargetList.Contains(triggerObj.GetComponent<BuildingManager>()))
				targetList.Add(triggerObj);
		}
		if (targetList.Count == 1)
			StartCoroutine(TrySpotTargetsNotSpotted());
	}
	public void RemoveTargetsOnFOVExit(GameObject triggerObj)
	{
		if (targetList.Contains(triggerObj))
		{
			targetList.Remove(triggerObj);
			//triggerObj.GetComponent<Entities>().HideEntity();
		}

		if (unitTargetList.Contains(triggerObj.GetComponent<UnitStateController>()))
			unitTargetList.Remove(triggerObj.GetComponent<UnitStateController>());

		if (buildingTargetList.Contains(triggerObj.GetComponent<BuildingManager>()))
			buildingTargetList.Remove(triggerObj.GetComponent<BuildingManager>());
	}
	public bool CheckIfUnitInLineOf(UnitStateController unit)
	{
		Physics.Linecast(CenterPoint.transform.position, unit.CenterPoint.transform.position, out RaycastHit hit, ignoreMe);
		raycastHit = hit;

		if (hit.collider.gameObject == unit.gameObject)
			return true;

		else
			return false;
	}
	public bool CheckIfBuildingInLineOf(BuildingManager building)
	{
		Physics.Linecast(CenterPoint.transform.position, building.CenterPoint.transform.position, out RaycastHit hit, ignoreMe);
		raycastHit = hit;

		if (hit.collider.gameObject == building.gameObject)
			return true;

		else
			return false;
	}
	public bool CheckIfEntityInLineOfSight(Entities entity)
	{
		Physics.Linecast(CenterPoint.transform.position, entity.CenterPoint.transform.position, out RaycastHit hit, ignoreMe);

		if (hit.collider.gameObject == entity.gameObject)
			return true;

		else
			return false;
	}
	public IEnumerator TrySpotTargetsNotSpotted()
	{
		try
		{
			for (int i = 0; i < targetList.Count; i++)
			{
				Entities entity = targetList[i].GetComponent<Entities>();
				if (!entity.isSpotted && CheckIfEntityInLineOfSight(entity) && entity != null)
				{
					entity.ShowEntity();
					entity.ResetEntitySpottedTimer();
				}

				else if (entity.isSpotted && !CheckIfEntityInLineOfSight(entity) && entity != null)
					entity.HideEntity();
			}
		}
		catch (Exception e)
		{
			throw e; //error pops up when a target is removed from the list, dont know how to fix
			//should be fine to leave as null refs from lists get removed after target is destroyed
		}

		yield return new WaitForSeconds(0.5f);

		if (targetList.Count != 0)
			StartCoroutine(TrySpotTargetsNotSpotted());
	}

	//UTILITY FUNCTIONS
	public IEnumerator DelaySecondaryAttack(UnitStateController unit, float seconds)
	{
		unit.weaponSystem.secondaryWeaponAttackSpeedTimer++;
		unit.weaponSystem.secondaryWeaponAttackSpeedTimer %= unit.weaponSystem.secondaryWeaponAttackSpeed - 1;
		yield return new WaitForSeconds(seconds);
		unit.weaponSystem.ShootSecondaryWeapon();
	}
	public override void RemoveEntityRefs()
	{
		if (GroupNum == 1)
		{
			playerController.unitSelectionManager.unitGroupOne.Remove(this);
			playerController.gameUIManager.UpdateUnitGroupUi(playerController.unitSelectionManager.unitGroupOne, 1);
		}
		if (GroupNum == 2)
		{
			playerController.unitSelectionManager.unitGroupTwo.Remove(this);
			playerController.gameUIManager.UpdateUnitGroupUi(playerController.unitSelectionManager.unitGroupTwo, 2);
		}
		if (GroupNum == 3)
		{
			playerController.unitSelectionManager.unitGroupThree.Remove(this);
			playerController.gameUIManager.UpdateUnitGroupUi(playerController.unitSelectionManager.unitGroupThree, 3);
		}
		if (GroupNum == 4)
		{
			playerController.unitSelectionManager.unitGroupFour.Remove(this);
			playerController.gameUIManager.UpdateUnitGroupUi(playerController.unitSelectionManager.unitGroupFour, 4);
		}
		if (GroupNum == 5)
		{
			playerController.unitSelectionManager.unitGroupFive.Remove(this);
			playerController.gameUIManager.UpdateUnitGroupUi(playerController.unitSelectionManager.unitGroupFive, 5);
		}

		playerController.unitSelectionManager.RemoveDeadUnitFromSelectedUnits(this);
		playerController.unitListForPlayer.Remove(this);
	}
	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);
		Gizmos.DrawWireSphere(transform.position, ViewRange);
		Gizmos.DrawLine(CenterPoint.transform.position, raycastHit.point);
	}

	//STATE CHANGE FUNCTIONS
	public void ChangeStateIdle()
	{
		currentState = idleState;
		currentState.Enter(this);
	}
	public void ChangeStateMoving()
	{
		currentState = movingState;
		currentState.Enter(this);
	}
	public void ChangeStateAttacking()
	{
		currentState = attackState;
		currentState.Enter(this);
	}
}
