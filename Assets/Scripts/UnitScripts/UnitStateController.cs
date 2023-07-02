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

	[Header("Unit Stats")]
	public string unitName;
	public float attackRange;
	public float ViewRange;

	[Header("Unit Bools")]
	public bool isUnitArmed;
	public bool isFlying;
	public bool hasRadar;
	public bool isCargoShip;
	public bool hasShootAnimation;
	public bool hasMoveAnimation;

	[Header("Unit Dynamic Refs")]
	public List<GameObject> targetList;
	public List<UnitStateController> unitTargetList;
	public List<BuildingManager> buildingTargetList;

	public Entities playerSetTarget;
	public UnitStateController currentUnitTarget;
	public BuildingManager currentBuildingTarget;

	public int GroupNum;
	public Vector3 targetPos;
	public Vector3 movePos;
	public NavMeshPath navMeshPath;

	public override void Start()
	{
		base.Start();

		ChangeStateIdle();
		//assign correct playercontroller to unit on start
		PlayerController controller = FindObjectOfType<PlayerController>();
		if (true)
		{
			playerController = controller;
			playerController.unitListForPlayer.Add(this);
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
		if (triggerObj.GetComponent<UnitStateController>() !=null && isPlayerOneEntity != triggerObj.GetComponent<UnitStateController>().isPlayerOneEntity)
		{
			if (!unitTargetList.Contains(triggerObj.GetComponent<UnitStateController>()))
				targetList.Add(triggerObj);
		}
		else if (triggerObj.GetComponent<BuildingManager>() != null && isPlayerOneEntity != triggerObj.GetComponent<BuildingManager>().isPlayerOneEntity
			&& triggerObj.GetComponent<CanPlaceBuilding>().isPlaced)
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
			targetList.Remove(triggerObj);

		if (unitTargetList.Contains(triggerObj.GetComponent<UnitStateController>()))
			unitTargetList.Remove(triggerObj.GetComponent<UnitStateController>());

		if (buildingTargetList.Contains(triggerObj.GetComponent<BuildingManager>()))
			buildingTargetList.Remove(triggerObj.GetComponent<BuildingManager>());
	}
	public IEnumerator TrySpotTargetsNotSpotted()
	{
		targetList = targetList.Where(item => item != null).ToList();
		for (int i = 0; i < targetList.Count; i++)
		{
			Entities entity = targetList[i].GetComponent<Entities>();
			if (CheckIfEntityInLineOfSight(entity) && entity != null)
			{
				if (!entity.wasRecentlySpotted && ShouldDisplaySpottedNotifToPlayer() && entity.GetComponent<CargoShipController>() == null)
					GameManager.Instance.playerNotifsManager.DisplayEventMessage("New Enemy Spotted", entity.transform.position);

				if (isUnitArmed)
					AddSpottedTargetsToListsWhenInAttackRange(entity);

				entity.ShowEntity();
				entity.ResetEntitySpottedTimer();
			}
		}
		yield return new WaitForSeconds(0.5f);

		if (targetList.Count != 0)
			StartCoroutine(TrySpotTargetsNotSpotted());
	}
	public void AddSpottedTargetsToListsWhenInAttackRange(Entities entity)
	{
		if (entity.GetComponent<UnitStateController>() && entity != null && CheckIfInAttackRange(entity.transform.position) &&
			!unitTargetList.Contains(entity.GetComponent<UnitStateController>()))
		{
			unitTargetList.Add(entity.GetComponent<UnitStateController>());
		}
		else if (entity.GetComponent<BuildingManager>() && entity != null && CheckIfInAttackRange(entity.transform.position) &&
			!buildingTargetList.Contains(entity.GetComponent<BuildingManager>()))
		{
			buildingTargetList.Add(entity.GetComponent<BuildingManager>());
		}
	}

	//HEALTH/HIT FUNCTIONS OVERRIDES
	public override void TryDisplayEntityHitNotif()
	{
		if (!isCargoShip && !wasRecentlyHit && ShouldDisplaySpottedNotifToPlayer())
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("UNIT UNDER ATTACK", transform.position);
	}
	public override void OnDeath()
	{
		base.OnDeath();
		if (!isCargoShip && ShouldDisplaySpottedNotifToPlayer())
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("UNIT DESTROYED", transform.position);
	}

	//ATTACK PLAYER SET TARGET FUNCTIONS
	public void TryAttackPlayerSetTarget(Entities entity)
	{
		if (IsPlayerSetTargetSpotted(entity)) //check if already spotted in target lists
		{
			playerSetTarget = entity;
		}
		else //walk in line of sight of enemy then switch to that target
		{
			playerSetTarget = entity;
			MoveToDestination(entity.transform.position);
		}
	}
	public bool IsPlayerSetTargetSpotted(Entities entity)
	{
		if (entity.GetComponent<UnitStateController>() != null)
		{
			if (unitTargetList.Contains(entity))
				return true;
			else
				return false;
		}
		else if (entity.GetComponent<BuildingManager>() != null)
		{
			if (buildingTargetList.Contains(entity))
				return true;
			else
				return false;
		}
		else
			return false;
	}

	//UNIT MOVE FUNCTION
	public void MoveToDestination(Vector3 newMovePos)
	{
		if (isFlying)
			movePos = new Vector3(newMovePos.x, newMovePos.y + 7, newMovePos.z);
		else
			movePos = newMovePos;

		ChangeStateMoving();
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

	//BOOL FUNCTIONS
	public bool CheckIfEntityInLineOfSight(Entities entity)
	{
		Physics.Linecast(CenterPoint.transform.position, entity.CenterPoint.transform.position, out RaycastHit hit, ignoreMe);

		if (hit.collider.gameObject == entity.gameObject)
			return true;

		else
			return false;
	}
	public bool CheckIfInAttackRange(Vector3 targetVector3)
	{
		float Distance = Vector3.Distance(transform.position, targetVector3);

		if (Distance <= attackRange)
			return true;
		else
			return false;
	}
}
