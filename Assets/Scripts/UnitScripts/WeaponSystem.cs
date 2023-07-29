using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class WeaponSystem : NetworkBehaviour
{
	public UnitStateController unit;

	public AudioSource mainWeaponAudio;
	public ParticleSystem mainWeaponParticles;
	public ParticleSystem mainWeaponProjectileParticle;
	public float mainWeaponDamage;
	public float mainWeaponAttackSpeed;
	[System.NonSerialized]
	public NetworkVariable<float> mainWeaponAttackSpeedTimer = new NetworkVariable<float>();

	public AudioSource secondaryWeaponAudio;
	public ParticleSystem secondaryWeaponParticles;
	public ParticleSystem secondaryWeaponProjectileParticle;
	public float secondaryWeaponDamage;
	public float secondaryWeaponAttackSpeed;
	[System.NonSerialized]
	public NetworkVariable<float> secondaryWeaponAttackSpeedTimer = new NetworkVariable<float>();

	public bool hasSecondaryWeapon;

	//if found then grab closest one
	[ServerRpc(RequireOwnership = false)]
	public void TryFindTargetsServerRPC(ulong networkObjId)
	{
		TryFindTargetsClientRPC(networkObjId);
	}
	[ClientRpc]
	public void TryFindTargetsClientRPC(ulong networkObjId)
	{
		UnitStateController unitNetwork = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].GetComponent<UnitStateController>();

		unitNetwork.weaponSystem.RemoveNullRefsFromTargetLists();
		unitNetwork.currentUnitTarget = GrabClosestUnit();
		unitNetwork.currentBuildingTarget = GrabClosestBuilding();
	}
	//sort targets from closest to furthest, check if target in view + attack range, once a valid target found, return target then end loop
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

	//server/host checks if entity exists + in attack range, if true shoot it, else try get new target and remove null refs from lists
	[ClientRpc]
	public void ShootMainWeapClientRPC(ulong networkObjId)
	{
		UnitStateController unitNetwork = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].GetComponent<UnitStateController>();
		unitNetwork.weaponSystem.ShootMainWeapon();
	}
	public void ShootMainWeapon()
	{
		if (HasPlayerSetTarget() && unit.CheckIfInAttackRange(unit.playerSetTarget.transform.position) &&
			unit.CheckIfEntityInLineOfSight(unit.playerSetTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.playerSetTarget.CenterPoint.transform.position);
			unit.playerSetTarget.ResetIsEntityHitTimer();
			if (!IsServer) return;
			unit.playerSetTarget.RecieveDamageServerRPC(mainWeaponDamage);
		}
		else if (HasUnitTarget() && unit.CheckIfInAttackRange(unit.currentUnitTarget.transform.position) &&
			unit.CheckIfEntityInLineOfSight(unit.currentUnitTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			unit.currentUnitTarget.ResetIsEntityHitTimer();
			if (!IsServer) return;
			unit.currentUnitTarget.RecieveDamageServerRPC(mainWeaponDamage);
		}
		else if (HasBuildingTarget() && unit.CheckIfInAttackRange(unit.currentBuildingTarget.transform.position) &&
			unit.CheckIfEntityInLineOfSight(unit.currentBuildingTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			unit.currentBuildingTarget.ResetIsEntityHitTimer();
			if (!IsServer) return;
			unit.currentBuildingTarget.RecieveDamageServerRPC(mainWeaponDamage);
		}
		else //TryFindTarget();
			TryFindTargetsServerRPC(GetComponent<NetworkObject>().NetworkObjectId);
	}
	[ClientRpc]
	public void ShootSeconWeapClientRPC(ulong networkObjId)
	{
		UnitStateController unitNetwork = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjId].GetComponent<UnitStateController>();
		unitNetwork.weaponSystem.ShootSecondaryWeapon();
	}
	public void ShootSecondaryWeapon()
	{
		if (HasPlayerSetTarget())
		{
			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();

			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.playerSetTarget.CenterPoint.transform.position);
			if (!IsServer) return;
			unit.playerSetTarget.RecieveDamageServerRPC(secondaryWeaponDamage);
		}
		else if (HasUnitTarget())
		{
			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();

			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			if (!IsServer) return;
			unit.currentUnitTarget.RecieveDamageServerRPC(secondaryWeaponDamage);
		}
		else if (HasBuildingTarget())
		{
			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();

			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			if (!IsServer) return;
			unit.currentBuildingTarget.RecieveDamageServerRPC(secondaryWeaponDamage);
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

	//ATTACK FUNCTIONS
	[ServerRpc(RequireOwnership = false)]
	public void GunTimersServerRPC()
	{
		if (!IsServer) return;
		MainGunTimer();

		if (hasSecondaryWeapon)
			SecondaryGunTimer();
	}
	[ClientRpc]
	public void GunTimersClientRPC()
	{
		MainGunTimer();

		if (hasSecondaryWeapon)
			SecondaryGunTimer();
	}
	public void MainGunTimer()
	{
		if (mainWeaponAttackSpeedTimer.Value > 0)
			mainWeaponAttackSpeedTimer.Value -= Time.deltaTime;
		else
		{
			ShootMainWeapClientRPC(GetComponent<NetworkObject>().NetworkObjectId);
			mainWeaponAttackSpeedTimer.Value = mainWeaponAttackSpeed;
		}
	}
	public void SecondaryGunTimer()
	{
		if (secondaryWeaponAttackSpeedTimer.Value > 0)
			secondaryWeaponAttackSpeedTimer.Value -= Time.deltaTime;
		else
		{
			if (unit.hasShootAnimation)
				StartCoroutine(DelaySecondaryAttack(1));
			else
			{
				ShootSeconWeapClientRPC(GetComponent<NetworkObject>().NetworkObjectId);
			}

			secondaryWeaponAttackSpeedTimer.Value = secondaryWeaponAttackSpeed;
		}
	}
	public IEnumerator DelaySecondaryAttack(float seconds)
	{
		secondaryWeaponAttackSpeedTimer.Value++;
		secondaryWeaponAttackSpeedTimer.Value %= secondaryWeaponAttackSpeed - 1;
		yield return new WaitForSeconds(seconds);
		ShootSeconWeapClientRPC(GetComponent<NetworkObject>().NetworkObjectId);
	}
}
