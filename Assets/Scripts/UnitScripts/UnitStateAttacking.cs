using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

public class UnitStateAttacking : UnitBaseState
{	
	public override void Enter(UnitStateController unit)
	{
		Debug.LogWarning("Entered Attacking State");
		if (unit.isTurret)
			unit.turretController.ActivateTurret();
		if (unit.isUnitArmed && unit.currentUnitTarget == null && unit.currentBuildingTarget == null)
		{
			unit.weaponSystem.TryFindTargets();
			//unit.weaponSystem.TryFindTargetsServerRPC(unit.GetComponent<NetworkObject>().NetworkObjectId);
		}
	}
	public override void Exit(UnitStateController unit)
	{

	}
	public override void UpdateLogic(UnitStateController unit)
	{
		if (unit.isUnitArmed)
		{
			unit.weaponSystem.GunTimersServerRPC();
		}
	}
	public override void UpdatePhysics(UnitStateController unit)
	{
		//only look at target when its within attack range
		if (unit.playerSetTarget != null && unit.CheckIfInAttackRange(unit.playerSetTarget.transform.position))
			StopAndLookAtTarget(unit, unit.playerSetTarget);
		else if (unit.currentUnitTarget != null && unit.CheckIfInAttackRange(unit.currentUnitTarget.transform.position))
			StopAndLookAtTarget(unit, unit.currentUnitTarget);
		else if (unit.currentBuildingTarget != null && unit.CheckIfInAttackRange(unit.currentBuildingTarget.transform.position))
			StopAndLookAtTarget(unit, unit.currentBuildingTarget);

		//continue to last movement destination
		if (!unit.isTurret && unit.agentNav.remainingDistance < unit.agentNav.stoppingDistance)
		{
			if (unit.hasMoveAnimation)
				unit.animatorController.SetBool("isIdle", true);

			if (unit.movingSFX.isPlaying)
				unit.movingSFX.Stop();
			unit.agentNav.isStopped = true;
		}
		else if (!unit.isTurret && unit.playerSetTarget != null && !unit.hasReachedPlayerSetTarget && 
			unit.CheckIfEntityInLineOfSight(unit.playerSetTarget))
		{
			if (unit.agentNav.remainingDistance < unit.attackRange - 5)
			{
				if (unit.hasMoveAnimation)
					unit.animatorController.SetBool("isIdle", true);

				if (unit.movingSFX.isPlaying)
					unit.movingSFX.Stop();
				unit.agentNav.isStopped = true;
				unit.hasReachedPlayerSetTarget = true;
			}
		}
	}
	public void StopAndLookAtTarget(UnitStateController unit, Entities entityToLookAt)
	{
		if (unit.isTurret)
		{
			unit.turretController.FaceTarget(entityToLookAt);
			unit.turretController.ChangeGunElevation(entityToLookAt);
		}
		else if (!unit.isTurret)
		{
			var lookRotation = Quaternion.LookRotation(entityToLookAt.transform.position - unit.transform.position);
			unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, unit.agentNav.angularSpeed / 1000);
		}
	}
}
