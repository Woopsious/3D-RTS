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

	//if x component exists and not in list, then add it to list, else ignore it, then grab closest
	public void TryFindTarget()
	{
		foreach (GameObject target in unit.targetList)
		{
			if (target.GetComponent<UnitStateController>() && target != null && CheckIfInAttackRange(target.transform.position) &&
				!unit.unitTargetList.Contains(target.GetComponent<UnitStateController>()))
			{
				unit.unitTargetList.Add(target.GetComponent<UnitStateController>());
			}
			else if (target.GetComponent<BuildingManager>() && target != null && CheckIfInAttackRange(target.transform.position) && 
				!unit.buildingTargetList.Contains(target.GetComponent<BuildingManager>()))
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
			if (CheckIfInAttackRange(unit.unitTargetList[i].transform.position) && 
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
			if (CheckIfInAttackRange(unit.buildingTargetList[i].transform.position) && 
				unit.CheckIfEntityInLineOfSight(unit.buildingTargetList[i]) && unit.buildingTargetList[i] != null)
					return unit.buildingTargetList[i];
		}
		return null;
	}

	//check if entity exists + is in attack range, if true shoot it, else try get new target and remove null refs from lists
	public void ShootMainWeapon()
	{
		if (HasUnitTarget() && CheckIfInAttackRange(unit.currentUnitTarget.transform.position) && unit.CheckIfEntityInLineOfSight(unit.currentUnitTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			if (!unit.currentUnitTarget.wasRecentlyHit && !unit.ShouldDisplayEventNotifToPlayer())
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("UNIT UNDER ATTACK", unit.currentUnitTarget.transform.position);

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			unit.currentUnitTarget.RecieveDamage(mainWeaponDamage);
			unit.currentUnitTarget.ResetIsEntityHitTimer();

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else if (!HasUnitTarget() && HasBuildingTarget() && 
			CheckIfInAttackRange(unit.currentBuildingTarget.transform.position) && unit.CheckIfEntityInLineOfSight(unit.currentBuildingTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			if (!unit.currentBuildingTarget.wasRecentlyHit && !unit.ShouldDisplayEventNotifToPlayer())
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("BUILDING UNDER ATTACK", unit.currentBuildingTarget.transform.position);

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			unit.currentBuildingTarget.RecieveDamage(mainWeaponDamage);
			unit.currentBuildingTarget.ResetIsEntityHitTimer();

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else
		{
			unit.currentUnitTarget = null;
			unit.currentBuildingTarget = null;
			RemoveNullRefsFromTargetLists();
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
