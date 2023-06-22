using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WeaponSystem : MonoBehaviour
{
	public UnitStateController unit;

	public AudioSource mainWeaponAudio;
	public ParticleSystem mainWeaponParticles;
	public ParticleSystem mainWeaponProjectileParticle;
	public int mainWeaponDamage;
	public float mainWeaponAttackSpeed;
	[System.NonSerialized]
	public float mainWeaponAttackSpeedTimer;

	public AudioSource secondaryWeaponAudio;
	public ParticleSystem secondaryWeaponParticles;
	public ParticleSystem secondaryWeaponProjectileParticle;
	public int secondaryWeaponDamage;
	public float secondaryWeaponAttackSpeed;
	[System.NonSerialized]
	public float secondaryWeaponAttackSpeedTimer;
	public bool hasSecondaryWeapon;

	//if x component exists + not in list, then add it to list, else ignore it, then grab closest
	public void TryFindTarget()
	{
		foreach (GameObject target in unit.targetList)
		{
			if (target.GetComponent<UnitStateController>() && target != null && !unit.unitTargetList.Contains(target.GetComponent<UnitStateController>()))
			{
				unit.unitTargetList.Add(target.GetComponent<UnitStateController>());
			}
			else if (target.GetComponent<BuildingManager>() && target != null && !unit.buildingTargetList.Contains(target.GetComponent<BuildingManager>()))
			{
				unit.buildingTargetList.Add(target.GetComponent<BuildingManager>());
			}
		}
		unit.currentUnitTarget = GrabClosestUnit();
		unit.currentBuildingTarget = GrabClosestBuilding();
	}
	//sort targets from closest to furthest, then check if target is in view + attack range, once a valid target is found, return that target and end loop
	public UnitStateController GrabClosestUnit()
	{
		unit.unitTargetList = unit.unitTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.unitTargetList.Count; i++)
		{
			if (CheckIfInAttackRange(unit.unitTargetList[i].transform.position) && unit.CheckIfEntityInLineOfSight(unit.unitTargetList[i])
				&& unit.unitTargetList[i] != null)
				return unit.unitTargetList[i];
		}
		return null;
	}
	public BuildingManager GrabClosestBuilding()
	{
		unit.buildingTargetList = unit.buildingTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.buildingTargetList.Count; i++)
		{
			if (CheckIfInAttackRange(unit.buildingTargetList[i].transform.position) && unit.CheckIfEntityInLineOfSight(unit.buildingTargetList[i])
				&& unit.buildingTargetList[i] != null)
				return unit.buildingTargetList[i];
		}
		return null;
	}

	//check if entity exists + is in attack range, if true shoot it, else try get new target and remove null refs from lists
	public void ShootMainWeapon()
	{
		if (HasUnitTarget() && CheckIfInAttackRange(unit.currentUnitTarget.transform.position) && unit.CheckIfEntityInLineOfSight(unit.currentUnitTarget))
		{
			if (unit.hasAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			unit.currentUnitTarget.RecieveDamage(mainWeaponDamage);
			unit.ResetIsEntityHitTimer();

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else if (!HasUnitTarget() && HasBuildingTarget() && 
			CheckIfInAttackRange(unit.currentBuildingTarget.transform.position) && unit.CheckIfEntityInLineOfSight(unit.currentBuildingTarget))
		{
			if (unit.hasAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			unit.currentBuildingTarget.RecieveDamage(mainWeaponDamage);
			unit.ResetIsEntityHitTimer();

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else
		{
			unit.currentUnitTarget = null;
			unit.currentBuildingTarget = null;
			unit.RemoveNullRefsFromLists(unit.targetList, unit.unitTargetList, unit.buildingTargetList);
			TryFindTarget();
		}
	}
	public void ShootSecondaryWeapon()
	{
		if (HasUnitTarget() && CheckIfInAttackRange(unit.currentUnitTarget.transform.position))
		{
			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			unit.currentUnitTarget.RecieveDamage(secondaryWeaponDamage);

			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();
		}
		else if (!HasUnitTarget() && HasBuildingTarget() && CheckIfInAttackRange(unit.currentBuildingTarget.transform.position))
		{
			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			unit.currentBuildingTarget.RecieveDamage(secondaryWeaponDamage);

			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();
		}
	}
	//function to shoot projectile at target center
	public void AimProjectileAtTarget(GameObject particleObject, Vector3 targetPos)
	{
		var lookRotation = Quaternion.LookRotation(targetPos - particleObject.transform.position);
		particleObject.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, 1);
	}

	//BOOL CHECKS
	public bool HasUnitTarget()
	{
		if (unit.currentUnitTarget != null)
		{
			return true;
		}
		return false;
	}
	public bool HasBuildingTarget()
	{
		if (unit.currentBuildingTarget != null)
		{
			return true;
		}
		return false;
	}
	public bool CheckIfInAttackRange(Vector3 targetVector3)
	{
		float Distance = Vector3.Distance(transform.position, targetVector3);

		if (Distance <= unit.attackRange)
			return true;

		else
			return false;
	}
}
