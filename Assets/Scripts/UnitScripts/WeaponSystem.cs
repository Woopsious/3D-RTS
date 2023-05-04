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
	public float mainWeaponAttackSpeed;

	public AudioSource secondaryWeaponAudio;
	public AudioSource secondaryWeaponHitAudio;
	public float secondaryWeaponAttackSpeed;

	public bool hasSecondaryWeapon;

	public void ShootMainWeapon()
	{
		GetTargetList();
		if (unit.hasAnimation)
		{
			unit.animatorController.SetBool("isAttacking", true);
		}
		if (HasUnitTarget())
		{
			StartCoroutine(MainWeaponCooldown());
			mainWeaponAudio.Play();
			mainWeaponHitAudio.Play();
			unit.currentUnitTarget.RecieveDamage(unit.damage);
		}
		else if (!HasUnitTarget() && HasBuildingTarget())
		{
			StartCoroutine(MainWeaponCooldown());
			mainWeaponAudio.Play();
			mainWeaponHitAudio.Play();
			unit.currentBuildingTarget.RecieveDamage(unit.damage);
		}
		else if (!HasUnitTarget() && !HasBuildingTarget())
		{
			StopCoroutine(MainWeaponCooldown());
		}
	}
	public void ShootSecondaryWeapon()
	{
		if (hasSecondaryWeapon)
		{
			if (HasUnitTarget())
			{
				StartCoroutine(SecondaryWeaponCooldown());
				secondaryWeaponAudio.Play();
				secondaryWeaponHitAudio.Play();
				unit.currentUnitTarget.RecieveDamage(unit.damage);
			}
			else if (!HasUnitTarget() && HasBuildingTarget())
			{
				StartCoroutine(SecondaryWeaponCooldown());
				secondaryWeaponAudio.Play();
				secondaryWeaponHitAudio.Play();
				unit.currentBuildingTarget.RecieveDamage(unit.damage);
			}
			else if (!HasUnitTarget() && !HasBuildingTarget())
			{
				StopCoroutine(MainWeaponCooldown());
			}
		}
	}
	public IEnumerator MainWeaponCooldown()
	{
		yield return new WaitForSeconds(secondaryWeaponAttackSpeed);

		ShootMainWeapon();
	}
	public IEnumerator SecondaryWeaponCooldown()
	{
		yield return new WaitForSeconds(secondaryWeaponAttackSpeed);

		ShootSecondaryWeapon();
	}
	//grab everything in attack range, if x component exists check if its not already in list, if not add it to list, else ignore it
	public void GetTargetList()
	{
		Collider[] newTargetArray = Physics.OverlapSphere(unit.transform.position, unit.ViewRange); //find targets in attack range
																									//check what side unit is on, check if unit is already in target list
		foreach (Collider collider in newTargetArray)
		{
			if (collider.GetComponent<UnitStateController>() != null && unit.isPlayerOneUnit != collider.GetComponent<UnitStateController>().isPlayerOneUnit)
			{
				if (!unit.unitTargetList.Contains(collider.GetComponent<UnitStateController>()))
				{
					unit.unitTargetList.Add(collider.GetComponent<UnitStateController>());
					collider.GetComponent<UnitStateController>().ShowUnit();
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
		unit.currentUnitTarget = GrabClosestUnit();
		unit.currentBuildingTarget = GrabClosestBuilding();
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
	//sort targets from closest to furthest, then check if target is in view, once a target is found and in view, return that unit and end loop
	public UnitStateController GrabClosestUnit()
	{
		foreach (UnitStateController listedUnit in unit.unitTargetList)
		{
			if (listedUnit == null)
				unit.unitTargetList.Remove(listedUnit);
		}
		unit.unitTargetList = unit.unitTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.unitTargetList.Count; i++)
		{
			Physics.Linecast(unit.CenterPoint.transform.position, unit.unitTargetList[i].GetComponent<UnitStateController>().CenterPoint.transform.position,
			out RaycastHit hit, unit.ignoreMe);

			if (hit.collider.GetComponent<UnitStateController>() != null)
			{
				return unit.unitTargetList[i];
			}
		}
		return null;
	}
	public BuildingManager GrabClosestBuilding()
	{
		foreach (BuildingManager listedBuilding in unit.buildingTargetList)
		{
			if (listedBuilding == null)
				unit.buildingTargetList.Remove(listedBuilding);
		}
		unit.buildingTargetList = unit.buildingTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.buildingTargetList.Count; i++)
		{
			Physics.Linecast(unit.CenterPoint.transform.position, unit.buildingTargetList[i].GetComponent<BuildingManager>().CenterPoint.transform.position,
			out RaycastHit hit, unit.ignoreMe);

			if (hit.collider.GetComponent<BuildingManager>() != null)
			{
				return unit.buildingTargetList[i];
			}
		}
		return null;
	}
}
