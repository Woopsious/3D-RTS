using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		unit.ShowUnit();
		if (unit.currentUnitTarget == null && unit.currentBuildingTarget == null)
		{
			unit.weaponSystem.GetTargetList();
		}
	}
	public override void Exit(UnitStateController unit)
	{

	}
	public override void UpdateLogic(UnitStateController unit)
	{
		unit.weaponSystem.mainWeaponAttackSpeedTimer += Time.deltaTime;
		if (unit.weaponSystem.mainWeaponAttackSpeedTimer >= unit.weaponSystem.mainWeaponAttackSpeed)
		{
			unit.weaponSystem.mainWeaponAttackSpeedTimer++;
			unit.weaponSystem.mainWeaponAttackSpeedTimer %= unit.weaponSystem.mainWeaponAttackSpeed - 1;
			unit.weaponSystem.ShootMainWeapon();
		}
		if (unit.weaponSystem.hasSecondaryWeapon)
		{
			unit.weaponSystem.secondaryWeaponAttackSpeedTimer += Time.deltaTime;
			if (unit.weaponSystem.secondaryWeaponAttackSpeedTimer >= unit.weaponSystem.secondaryWeaponAttackSpeed)
			{
				unit.weaponSystem.secondaryWeaponAttackSpeedTimer++;
				unit.weaponSystem.secondaryWeaponAttackSpeedTimer %= unit.weaponSystem.secondaryWeaponAttackSpeed - 1;
				unit.weaponSystem.ShootSecondaryWeapon();
			}
		}
	}
	public override void UpdatePhysics(UnitStateController unit)
	{
		//continue to last movement destination, only look at target thats within attack range
		if(unit.currentUnitTarget != null)
		{
			if (unit.weaponSystem.CheckIfInAttackRange(unit.currentUnitTarget.transform.position))
				StopAndLookAtTarget(unit);
		}
		else if (unit.currentBuildingTarget != null) 
		{
			if (unit.weaponSystem.CheckIfInAttackRange(unit.currentUnitTarget.transform.position))
				StopAndLookAtTarget(unit);
		}
		if (Vector3.Distance(unit.transform.position, unit.movePos) > unit.agentNav.stoppingDistance && unit.movePos != new Vector3(0, 0, 0))
		{
			unit.animatorController.SetBool("isIdle", false);
		}
		else
		{
			unit.movePos = new Vector3(0, 0, 0);
			unit.animatorController.SetBool("isIdle", true);
			if (unit.movingSFX.isPlaying)
				unit.movingSFX.Stop();
		}
	}

	public void StopAndLookAtTarget(UnitStateController unit)
	{
		if (unit.currentUnitTarget != null)
		{
			var lookRotation = Quaternion.LookRotation(unit.currentUnitTarget.transform.position - unit.transform.position);
			unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, unit.agentNav.angularSpeed / 1000);
		}
		if (unit.currentUnitTarget == null && unit.currentBuildingTarget != null)
		{
			var lookRotation = Quaternion.LookRotation(unit.currentBuildingTarget.transform.position - unit.transform.position);
			unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, unit.agentNav.angularSpeed / 1000);
		}
	}
}
