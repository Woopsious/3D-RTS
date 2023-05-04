using System.Collections;
using System.Collections.Generic;
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

	public void ShootAtEnemy(UnitStateController targetUnit, BuildingManager targetBuilding)
	{

	}
	public IEnumerator ShootMainWeapon()
	{

	}
	public IEnumerator ShootSecondaryWeapon()
	{
		if (!hasSecondaryWeapon)
		{
			StopCoroutine(ShootSecondaryWeapon());
		}
		yield return new WaitForSeconds(secondaryWeaponAttackSpeed);

		if (unit.currentUnitTarget != null || unit.currentBuildingTarget != null)
		{
			StartCoroutine(ShootSecondaryWeapon());
		}
	}
}
