using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
	public UnitStateController unit;

	public AudioSource mainWeaponAudio;
	public AudioSource mainWeaponHitAudio;
	public ParticleSystem mainWeaponParticles;
	public ParticleSystem mainWeaponProjectileParticle;
	public ParticleSystem mainWeaponHitParticles;
	public int mainWeaponDamage;
	public float mainWeaponAttackSpeed;
	//[System.NonSerialized]
	public float mainWeaponAttackSpeedTimer;

	public AudioSource secondaryWeaponAudio;
	public AudioSource secondaryWeaponHitAudio;
	public ParticleSystem secondaryWeaponParticles;
	public ParticleSystem secondaryWeaponHitParticles;
	public int secondaryWeaponDamage;
	public float secondaryWeaponAttackSpeed;
	//[System.NonSerialized]
	public float secondaryWeaponAttackSpeedTimer;

	public bool hasSecondaryWeapon;

	//1. get list of possible units and buildings in visual range, (remove any null refs), loop through sorted list from closest unit, checking Line of sight
	//2. then return that unit and set it as unit/buildingTarget, and start shooting at it with main weapon and secondary weapon (if it has one)
	//3. check if ref is null and target is in attack range after every shot. continue till one is false then loop back to 1.
	//4. if lists of possible targets are 0 go back to x state

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
			if (collider.GetComponent<BuildingManager>() != null && unit.isPlayerOneUnit != collider.GetComponent<BuildingManager>().isPlayerOneBuilding)
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
		//else restart shooting loop of weapons (see comment above function for more info)
		else
		{
			//if(!unit.isPlayerOneUnit)
			{
				ShootMainWeapon();
				if (hasSecondaryWeapon)
					ShootSecondaryWeapon();
			}
		}
	}
	//check if entity exists + is in attack range, if true shoot it, else stop coroutine and try get new target (ShootMainWeapon function calls GetTargetList again)
	public void ShootMainWeapon()
	{
		if (unit.hasAnimation)
		{
			unit.animatorController.SetBool("isAttacking", true);
		}
		if (HasUnitTarget() && CheckIfInAttackRange(unit.currentUnitTarget.transform.position))
		{
			//StartCoroutine(MainWeaponCooldown());
			unit.currentUnitTarget.RecieveDamage(mainWeaponDamage);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else if (!HasUnitTarget() && HasBuildingTarget() && CheckIfInAttackRange(unit.currentBuildingTarget.transform.position))
		{
			//StartCoroutine(MainWeaponCooldown());
			unit.currentBuildingTarget.RecieveDamage(secondaryWeaponDamage);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();
		}
		else
		{
			unit.currentUnitTarget = null;
			unit.currentBuildingTarget = null;
			//StopCoroutine(MainWeaponCooldown());
			GetTargetList();
		}
	}
	public void ShootSecondaryWeapon()
	{
		if (HasUnitTarget() && CheckIfInAttackRange(unit.currentUnitTarget.transform.position))
		{
			//StartCoroutine(SecondaryWeaponCooldown());
			unit.currentUnitTarget.RecieveDamage(unit.damage);

			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();
		}
		else if (!HasUnitTarget() && HasBuildingTarget() && CheckIfInAttackRange(unit.currentBuildingTarget.transform.position))
		{
			//StartCoroutine(SecondaryWeaponCooldown());
			unit.currentBuildingTarget.RecieveDamage(unit.damage);

			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();
		}
		else
		{
			//StopCoroutine(MainWeaponCooldown());
		}
	}

	//Weapon Cooldown Enumerators
	public IEnumerator MainWeaponCooldown()
	{
		yield return new WaitForSeconds(mainWeaponAttackSpeed);

		ShootMainWeapon();
	}
	public IEnumerator SecondaryWeaponCooldown()
	{
		yield return new WaitForSeconds(secondaryWeaponAttackSpeed);

		ShootSecondaryWeapon();
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
		/*
		foreach (UnitStateController listedUnit in unit.unitTargetList)
		{
			if (listedUnit == null)
				unit.unitTargetList.Remove(listedUnit);
		}
		*/
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
		/*
		foreach (BuildingManager listedBuilding in unit.buildingTargetList)
		{
			if (listedBuilding == null)
				unit.buildingTargetList.Remove(listedBuilding);
		}
		*/
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
