using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using static UnityEngine.GraphicsBuffer;

public class UnitMovingState : UnitBaseState
{
	public override void Enter(UnitStateController unit)
	{
		Debug.LogWarning("Entered Moving State");

		if (!CheckAndSetNewPath(unit)) //if path valid move
		{
			if (!NavMesh.SamplePosition(unit.movePos, out NavMeshHit navMeshHit, 5, unit.agentNav.areaMask))//if path false try find new one
			{
				Debug.LogError("Unit cant find path");
				unit.ChangeStateIdleClientRPC();
				unit.ChangeStateIdleServerRPC(unit.EntityNetworkObjId);

				if (unit.playerController != null)
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Unit Cant find path to location", 2);
					AnnouncerSystem.Instance.PlayNegReplyInvalidUnitPositionSFX();
				}
				return; //if failed return
			}

			else //else set new path
			{
				unit.movePos = navMeshHit.position;
				CheckAndSetNewPath(unit);
			}
		}

		if (unit.hasRadar)
		{
			unit.audioSFXs[2].Stop();
			unit.audioSFXs[1].Play();
		}
		unit.movingSFX.Play();

		if (unit.hasMoveAnimation)
			unit.animatorController.SetBool("isIdle", false);

		if (unit.hasShootAnimation)
			unit.animatorController.SetBool("isAttacking", false);

		unit.agentNav.isStopped = false;
	}
	public override void Exit(UnitStateController unit)
	{

	}
	public override void UpdateLogic(UnitStateController unit)
	{

	}
	public override void UpdatePhysics(UnitStateController unit)
	{
		CheckDistance(unit);
	}
	public void CheckDistance(UnitStateController unit)
	{
		if (unit.agentNav.remainingDistance < unit.agentNav.stoppingDistance)
		{
			unit.ChangeStateIdleClientRPC();
			unit.ChangeStateIdleServerRPC(unit.EntityNetworkObjId);
			unit.agentNav.isStopped = true;
		}
		else if (unit.playerSetTarget != null && !unit.hasReachedPlayerSetTarget && unit.CheckIfEntityInLineOfSight(unit.playerSetTarget))
		{
			if (unit.agentNav.remainingDistance < unit.attackRange.Value - 5)
			{
				unit.ChangeStateIdleClientRPC();
				unit.ChangeStateIdleServerRPC(unit.EntityNetworkObjId);
				unit.agentNav.isStopped = true;
				unit.hasReachedPlayerSetTarget = true;
			}
		}
	}
	public bool CheckAndSetNewPath(UnitStateController unit)
	{
		NavMeshPath path = new NavMeshPath();
		if (unit.agentNav.CalculatePath(unit.movePos, path))
		{
			unit.agentNav.SetPath(path);
			return true;
		}
		else
		{
			return false;
		}
	}
}
