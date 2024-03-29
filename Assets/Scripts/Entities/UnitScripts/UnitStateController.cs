using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

[System.Serializable]
public class UnitStateController : Entities
{
	public LayerMask ignoreMe;

	public UnitBaseState currentState;
	public UnitIdleState idleState = new UnitIdleState();
	public UnitMovingState movingState = new UnitMovingState();
	public UnitStateAttacking attackState = new UnitStateAttacking();

	public TurretController turretController;
	public WeaponSystem weaponSystem;

	[Header("Unit Refs")]
	public Animator animatorController;
	public AudioSource movingSFX;
	public NavMeshAgent agentNav;
	public Rigidbody rb;
	public GameObject attackRangeMeshObj;
	public GameObject FoVMeshObj;

	[Header("Unit Stats")]
	public NetworkVariable<int> attackRange = new NetworkVariable<int>();
	public int ViewRange;

	[Header("Unit Bools")]
	public bool isUnitArmed;
	public bool isFlying;
	public bool hasRadar;
	public bool isCargoShip;
	public bool isTurret;
	public bool hasShootAnimation;
	public bool hasMoveAnimation;
	public bool hasReachedPlayerSetTarget;

	[Header("Unit Dynamic Refs")]
	public CapturePointController capturePoint;
	public List<GameObject> targetList;
	public List<UnitStateController> unitTargetList;
	public List<BuildingManager> buildingTargetList;

	public Entities playerSetTarget;
	public UnitStateController currentUnitTarget;
	public BuildingManager currentBuildingTarget;

	public Entities syncedPlayerSetTarget;
	public UnitStateController syncedCurrentUnitTarget;
	public BuildingManager syncedCurrentBuildingTarget;

	public int GroupNum;
	public Vector3 targetPos;
	public Vector3 movePos;
	public NavMeshPath navMeshPath;

	public Vector3 TestLineCastPos;

	public override void Start()
	{
		base.Start();
		OnUnitStartUp();
	}
	public override void Update()
	{
		base.Update();
		if (!isCargoShip)
			currentState.UpdateLogic(this);
	}
	public virtual void FixedUpdate()
	{
		base.Update();
		if (!isCargoShip)
			currentState.UpdatePhysics(this);
	}
	public void OnUnitStartUp()
	{
		ChangeStateIdleClientRPC();
		ChangeStateIdleServerRPC(EntityNetworkObjId);

		PlayerController playerCon = FindObjectOfType<PlayerController>(); //set refs here
		if (playerCon.isPlayerOne != !isPlayerOneEntity)
		{
			playerController = playerCon;
			playerController.unitListForPlayer.Add(this);
		}
		//set entity Minimap layer and colour
		if (isPlayerOneEntity)
			miniMapRenderObj.layer = 11;
		else
			miniMapRenderObj.layer = 12;

		if (playerController != null)
			miniMapRenderObj.GetComponent<SpriteRenderer>().color = Color.green;
		else
			miniMapRenderObj.GetComponent<SpriteRenderer>().color = Color.red;

		if (IsServer && playerCon.isPlayerOne)
			GameManager.Instance.playerUnitsList.Add(GetComponent<UnitStateController>());
		if (isTurret)
			GetComponent<TurretController>().AddTurretRefs();
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

		if (playerSetTarget == triggerObj.GetComponent<UnitStateController>())
			playerSetTarget = null;
		else if (playerSetTarget == triggerObj.GetComponent<BuildingManager>())
			playerSetTarget = null;

		if (currentUnitTarget == triggerObj.GetComponent<UnitStateController>())
			currentUnitTarget = null;
		if (currentBuildingTarget == triggerObj.GetComponent<BuildingManager>())
			currentBuildingTarget = null;
	}
	public IEnumerator TrySpotTargetsNotSpotted()
	{
		targetList = targetList.Where(item => item != null).ToList();
		for (int i = 0; i < targetList.Count; i++)
		{
			Entities entity = targetList[i].GetComponent<Entities>();
			if (CheckIfEntityInLineOfSight(entity))
			{
				if (!entity.wasRecentlySpotted && !IsPlayerControllerNull())
				{
					if (entity.GetComponent<CargoShipController>() != null)
						GameManager.Instance.playerNotifsManager.DisplayEventMessage("Enemy CargoShip Spotted", entity.transform.position);
					else if (entity.GetComponent<UnitStateController>() != null)
						GameManager.Instance.playerNotifsManager.DisplayEventMessage("Enemy Unit Spotted", entity.transform.position);
					else if (entity.GetComponent<BuildingManager>() != null)
						GameManager.Instance.playerNotifsManager.DisplayEventMessage("Enemy Building Spotted", entity.transform.position);
				}

				if (isUnitArmed)
				{
					AddSpottedTargetsToListsWhenInAttackRange(entity);
					if (targetList.Count != 0 && currentState != attackState) //switch to attack state if targets found
					{
						ChangeStateAttackingClientRPC();
						ChangeStateAttackingServerRPC(EntityNetworkObjId);
					}
				}

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
		if (!isCargoShip && !wasRecentlyHit && !IsPlayerControllerNull())
		{
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("UNIT UNDER ATTACK", transform.position);
			AnnouncerSystem.Instance.PlayAlertUnitUnderAttackSFX();
		}
	}
	public override void OnEntityDeath()
	{
		if (!isCargoShip && !IsPlayerControllerNull())
		{
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("UNIT DESTROYED", transform.position);
			AnnouncerSystem.Instance.PlayAlertUnitLostSFX();
		}

		base.OnEntityDeath();
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

		ChangeStateMovingServerRPC(EntityNetworkObjId);
	}

	//UTILITY FUNCTIONS
	public override void RemoveEntityRefs()
	{
		if (capturePoint != null)
			capturePoint.RemoveUnitRefsOnUnitDeath(this);

		if (playerController != null)
		{
			playerController.unitListForPlayer.Remove(this);
			playerController.unitSelectionManager.RemoveDeadUnitFromSelectedUnits(this);

			if (!isTurret)
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
			}
			else if (isTurret)
			{
				playerController.unitSelectionManager.RemoveDeadTurretFromSelectedTurrets(GetComponent<TurretController>());
				turretController.capturePointController.TurretDefenses.Remove(turretController);
			}
		}
	}
	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange.Value);
		Gizmos.DrawWireSphere(transform.position, ViewRange);
	}

	//STATE CHANGE FUNCTIONS
	[ServerRpc(RequireOwnership = false)]
	public void ChangeStateIdleServerRPC(ulong networkObjId)
	{
		NetworkManager.SpawnManager.SpawnedObjects[networkObjId].GetComponent<UnitStateController>().ChangeStateIdleClientRPC();
	}
	[ServerRpc(RequireOwnership = false)]
	public void ChangeStateMovingServerRPC(ulong networkObjId)
	{
		NetworkManager.SpawnManager.SpawnedObjects[networkObjId].GetComponent<UnitStateController>().ChangeStateMovingClientRPC();
	}
	[ServerRpc(RequireOwnership = false)]
	public void ChangeStateAttackingServerRPC(ulong networkObjId)
	{
		NetworkManager.SpawnManager.SpawnedObjects[networkObjId].GetComponent<UnitStateController>().ChangeStateAttackingClientRPC();
	}
	[ClientRpc]
	public void ChangeStateIdleClientRPC()
	{
		if (currentState != idleState)
		{
			currentState = idleState;
			currentState.Enter(this);
		}
	}
	[ClientRpc]
	public void ChangeStateMovingClientRPC()
	{
		currentState = movingState;
		currentState.Enter(this);
	}
	[ClientRpc]
	public void ChangeStateAttackingClientRPC()
	{
		if (currentState != attackState)
		{
			currentState = attackState;
			currentState.Enter(this);
		}
	}

	//BOOL FUNCTIONS
	public bool CheckIfEntityInLineOfSight(Entities entity)
	{
		Physics.Linecast(CenterPoint.transform.position, entity.CenterPoint.transform.position, out RaycastHit hit, ignoreMe);

		if (hit.point != null && hit.collider.gameObject == entity.gameObject)
			return true;
		else
			return false;
	}
	public bool CheckIfInAttackRange(Vector3 targetVector3)
	{
		float Distance = Vector3.Distance(transform.position, targetVector3);

		if (Distance <= attackRange.Value)
			return true;
		else
			return false;
	}
}
