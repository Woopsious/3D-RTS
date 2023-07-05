using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
		//Debug.Log("Entered Attacking State");

		if (unit.isUnitArmed && unit.currentUnitTarget == null && unit.currentBuildingTarget == null)
			unit.weaponSystem.TryFindTarget();
	}
	public override void Exit(UnitStateController unit)
	{

	}
	public override void UpdateLogic(UnitStateController unit)
	{
		if (unit.isUnitArmed)
		{
			MainGunTimer(unit);

			if (unit.weaponSystem.hasSecondaryWeapon)
				SecondaryGunTimer(unit);
		}
	}
	public override void UpdatePhysics(UnitStateController unit)
	{
		//only look at target when its within attack range
		if (unit.playerSetTarget != null)
		{
			if (unit.CheckIfInAttackRange(unit.playerSetTarget.transform.position))
				StopAndLookAtTarget(unit);
		}
		else if (unit.currentUnitTarget != null)
		{
			if (unit.CheckIfInAttackRange(unit.currentUnitTarget.transform.position))
				StopAndLookAtTarget(unit);
		}
		else if (unit.currentBuildingTarget != null)
		{
			if (unit.CheckIfInAttackRange(unit.currentBuildingTarget.transform.position))
				StopAndLookAtTarget(unit);
		}

		//continue to last movement destination
		if (unit.agentNav.remainingDistance < unit.agentNav.stoppingDistance)
		{
			if (unit.hasMoveAnimation)
				unit.animatorController.SetBool("isIdle", true);

			if (unit.movingSFX.isPlaying)
				unit.movingSFX.Stop();
			unit.agentNav.isStopped = true;
		}
		else if (unit.playerSetTarget != null && unit.CheckIfEntityInLineOfSight(unit.playerSetTarget))
		{
			if (unit.agentNav.remainingDistance < unit.attackRange - 5)
			{
				if (unit.hasMoveAnimation)
					unit.animatorController.SetBool("isIdle", true);

				if (unit.movingSFX.isPlaying)
					unit.movingSFX.Stop();
				unit.agentNav.isStopped = true;
			}
		}
	}
	public void StopAndLookAtTarget(UnitStateController unit)
	{
		if (unit.playerSetTarget != null)
		{
			var lookRotation = Quaternion.LookRotation(unit.playerSetTarget.transform.position - unit.transform.position);
			unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, unit.agentNav.angularSpeed / 1000);
		}
		else if (unit.currentUnitTarget != null)
		{
			var lookRotation = Quaternion.LookRotation(unit.currentUnitTarget.transform.position - unit.transform.position);
			unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, unit.agentNav.angularSpeed / 1000);
		}
		else if (unit.currentBuildingTarget != null)
		{
			var lookRotation = Quaternion.LookRotation(unit.currentBuildingTarget.transform.position - unit.transform.position);
			unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, unit.agentNav.angularSpeed / 1000);
		}
	}
	public void MainGunTimer(UnitStateController unit)
	{
		if (unit.weaponSystem.mainWeaponAttackSpeedTimer > 0)
			unit.weaponSystem.mainWeaponAttackSpeedTimer -= Time.deltaTime;
		else
		{
			unit.weaponSystem.ShootMainWeapon();
			unit.weaponSystem.mainWeaponAttackSpeedTimer = unit.weaponSystem.mainWeaponAttackSpeed;
		}
	}
	public void SecondaryGunTimer(UnitStateController unit)
	{
		if (unit.weaponSystem.secondaryWeaponAttackSpeedTimer > 0)
				unit.weaponSystem.secondaryWeaponAttackSpeedTimer -= Time.deltaTime;
		else
		{
			if (unit.hasShootAnimation)
				unit.StartCoroutine(unit.DelaySecondaryAttack(unit, 1));
			else
				unit.weaponSystem.ShootSecondaryWeapon();

			unit.weaponSystem.secondaryWeaponAttackSpeedTimer = unit.weaponSystem.secondaryWeaponAttackSpeed;
		}
	}
}
