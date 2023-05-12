using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	//grab entities in view range - 0.25, if x component exists + is enemy entity + not in list, then add it to list, else ignore it
	public void GetTargetList()
	{
		unit.unitTargetList.Clear();
		unit.buildingTargetList.Clear();
		Collider[] newTargetArray = Physics.OverlapSphere(unit.transform.position, unit.ViewRange - 0.25f); //find targets in attack range
																									//check what side unit is on, check if unit is already in target list
		foreach (Collider collider in newTargetArray)
		{
			if (collider.GetComponent<UnitStateController>() != null && unit.isPlayerOneUnit != collider.GetComponent<UnitStateController>().isPlayerOneUnit)
			{
				if (!unit.unitTargetList.Contains(collider.GetComponent<UnitStateController>()))
				{
					unit.unitTargetList.Add(collider.GetComponent<UnitStateController>());
				}
			}
			if (collider.GetComponent<BuildingManager>() != null && unit.isPlayerOneUnit != collider.GetComponent<BuildingManager>().isPlayerOneBuilding
				&& !collider.GetComponent<CanPlaceBuilding>().isPlaced)		//filter out non placed buildings
			{
				if (!unit.buildingTargetList.Contains(collider.GetComponent<BuildingManager>()))
				{
					unit.buildingTargetList.Add(collider.GetComponent<BuildingManager>());
				}
			}
		}
		//return closest target (see comment above function for more info)
		unit.currentUnitTarget = GrabClosestUnit();
		unit.currentBuildingTarget = GrabClosestBuilding();
		if (unit.isPlayerOneUnit)
		{
			Debug.Log(unit.currentUnitTarget);
			Debug.Log(unit.currentBuildingTarget);
		}
		//change to idle if no valid enemy entities are returned
		if (!HasBuildingTarget() && !HasUnitTarget())
		{
			unit.ChangeStateIdle();
		}
	}
	//check if entity exists + is in attack range, if true shoot it, else try get new target (ShootMainWeapon function calls GetTargetList again)
	public void ShootMainWeapon()
	{
		if (unit.hasAnimation)
		{
			unit.animatorController.SetBool("isAttacking", true);
		}
		if (HasUnitTarget() && CheckIfInAttackRange(unit.currentUnitTarget.transform.position))
		{
			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			unit.currentUnitTarget.RecieveDamage(mainWeaponDamage);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else if (!HasUnitTarget() && HasBuildingTarget() && CheckIfInAttackRange(unit.currentBuildingTarget.transform.position))
		{
			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			unit.currentBuildingTarget.RecieveDamage(secondaryWeaponDamage);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else
		{
			unit.currentUnitTarget = null;
			unit.currentBuildingTarget = null;
			GetTargetList();
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
	public bool CheckIfInAttackRange(Vector3 targetPos)
	{
		float Distance = Vector3.Distance(unit.transform.position, targetPos);

		if (Distance <= unit.attackRange)
			return true;
		else
			return false;
	}
	//sort targets from closest to furthest, then check if target is in view + attack range, once a valid target is found, return that target and end loop
	public UnitStateController GrabClosestUnit()
	{
		unit.unitTargetList = unit.unitTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.unitTargetList.Count; i++)
		{
			Physics.Linecast(unit.CenterPoint.transform.position, unit.unitTargetList[i].GetComponent<UnitStateController>().CenterPoint.transform.position,
			out RaycastHit hit, unit.ignoreMe);

			if (hit.collider.GetComponent<UnitStateController>() != null && CheckIfInAttackRange(hit.collider.transform.position))
			{
				hit.collider.GetComponent<UnitStateController>().ShowUnit();
				return unit.unitTargetList[i];
			}
		}
		return null;
	}
	public BuildingManager GrabClosestBuilding()
	{
		unit.buildingTargetList = unit.buildingTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.buildingTargetList.Count; i++)
		{
			Physics.Linecast(unit.CenterPoint.transform.position, unit.buildingTargetList[i].GetComponent<BuildingManager>().CenterPoint.transform.position,
			out RaycastHit hit, unit.ignoreMe);

			if (hit.collider.GetComponent<BuildingManager>() != null && CheckIfInAttackRange(hit.collider.transform.position))
			{
				return unit.buildingTargetList[i];
			}
		}
		return null;
	}
}
