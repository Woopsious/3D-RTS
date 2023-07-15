using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class WeaponSystem : MonoBehaviour
{
	public UnitStateController unit;

	public AudioSource mainWeaponAudio;
	public ParticleSystem mainWeaponParticles;
	public ParticleSystem mainWeaponProjectileParticle;
	public float mainWeaponDamage;
	public float mainWeaponAttackSpeed;
	[System.NonSerialized]
	public float mainWeaponAttackSpeedTimer;

	public AudioSource secondaryWeaponAudio;
	public ParticleSystem secondaryWeaponParticles;
	public ParticleSystem secondaryWeaponProjectileParticle;
	public float secondaryWeaponDamage;
	public float secondaryWeaponAttackSpeed;
	[System.NonSerialized]
	public float secondaryWeaponAttackSpeedTimer;
	public bool hasSecondaryWeapon;

	//if found then grab closest one
	public void TryFindTarget()
	{
		RemoveNullRefsFromTargetLists();

		unit.currentUnitTarget = GrabClosestUnit();
		unit.currentBuildingTarget = GrabClosestBuilding();
	}
	//sort targets from closest to furthest, then check if target is in view + attack range, once a valid target is found, return that target and end loop
	public UnitStateController GrabClosestUnit()
	{
		unit.unitTargetList = unit.unitTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.unitTargetList.Count; i++)
		{
			if (unit.CheckIfInAttackRange(unit.unitTargetList[i].transform.position) && 
				unit.CheckIfEntityInLineOfSight(unit.unitTargetList[i]) && unit.unitTargetList[i] != null)
					return unit.unitTargetList[i];
		}
		return null;
	}
	public BuildingManager GrabClosestBuilding()
	{
		unit.buildingTargetList = unit.buildingTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.buildingTargetList.Count; i++)
		{
			if (unit.CheckIfInAttackRange(unit.buildingTargetList[i].transform.position) && 
				unit.CheckIfEntityInLineOfSight(unit.buildingTargetList[i]) && unit.buildingTargetList[i] != null)
					return unit.buildingTargetList[i];
		}
		return null;
	}

	//check if entity exists + is in attack range, if true shoot it in order of attack priority, else try get new target and remove null refs from lists
	public void ShootMainWeapon()
	{
		if (HasPlayerSetTarget() && unit.CheckIfInAttackRange(unit.playerSetTarget.transform.position) && 
			unit.CheckIfEntityInLineOfSight(unit.playerSetTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.playerSetTarget.CenterPoint.transform.position);
			unit.playerSetTarget.RecieveDamage(mainWeaponDamage);
			unit.playerSetTarget.ResetIsEntityHitTimer();

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else if (HasUnitTarget() && unit.CheckIfInAttackRange(unit.currentUnitTarget.transform.position) && 
			unit.CheckIfEntityInLineOfSight(unit.currentUnitTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			unit.currentUnitTarget.RecieveDamage(mainWeaponDamage);
			unit.currentUnitTarget.ResetIsEntityHitTimer();

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else if (HasBuildingTarget() && unit.CheckIfInAttackRange(unit.currentBuildingTarget.transform.position) && 
			unit.CheckIfEntityInLineOfSight(unit.currentBuildingTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			unit.currentBuildingTarget.RecieveDamage(mainWeaponDamage);
			unit.currentBuildingTarget.ResetIsEntityHitTimer();

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else
			TryFindTarget();
	}
	public void ShootSecondaryWeapon()
	{
		if (HasPlayerSetTarget())
		{
			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.playerSetTarget.CenterPoint.transform.position);
			unit.playerSetTarget.RecieveDamage(secondaryWeaponDamage);

			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();
		}
		else if (HasUnitTarget())
		{
			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			unit.currentUnitTarget.RecieveDamage(secondaryWeaponDamage);

			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();
		}
		else if (HasBuildingTarget())
		{
			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			unit.currentBuildingTarget.RecieveDamage(secondaryWeaponDamage);

			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();
		}
	}

	//UTILITY FUNCTIONS
	public void AimProjectileAtTarget(GameObject particleObject, Vector3 targetPos) //function to shoot projectile at target center
	{
		var lookRotation = Quaternion.LookRotation(targetPos - particleObject.transform.position);
		particleObject.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, 1);
	}
	public void RemoveNullRefsFromTargetLists()
	{
		unit.targetList = unit.targetList.Where(item => item != null).ToList();
		unit.unitTargetList = unit.unitTargetList.Where(item => item != null).ToList();
		unit.buildingTargetList = unit.buildingTargetList.Where(item => item != null).ToList();
	}

	//BOOL CHECKS
	public bool HasPlayerSetTarget()
	{
		if (unit.playerSetTarget != null)
			return true;
		return false;
	}
	public bool HasUnitTarget()
	{
		if (unit.currentUnitTarget != null)
			return true;
		return false;
	}
	public bool HasBuildingTarget()
	{
		if (unit.currentBuildingTarget != null)
			return true;
		return false;
	}
}
