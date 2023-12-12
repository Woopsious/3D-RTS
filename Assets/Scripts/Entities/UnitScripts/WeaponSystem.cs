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
	public NetworkVariable<float> mainWeaponDamage = new NetworkVariable<float>();
	public float mainWeaponAttackSpeed;
	private float mainWeaponAttackSpeedTimer;

	public AudioSource secondaryWeaponAudio;
	public ParticleSystem secondaryWeaponParticles;
	public ParticleSystem secondaryWeaponProjectileParticle;
	public NetworkVariable<float> secondaryWeaponDamage = new NetworkVariable<float>();
	public float secondaryWeaponAttackSpeed;
	private float secondaryWeaponAttackSpeedTimer;

	public bool hasSecondaryWeapon;

	//try find targets if found then grab closest one
	public void TryFindTargets()
	{
		if (IsServer)
		{
			RemoveNullRefsFromTargetLists();
			if (unit.currentUnitTarget == null)
				unit.currentUnitTarget = GrabClosestUnit();
			if (unit.currentUnitTarget == null && unit.currentBuildingTarget == null)
				unit.currentBuildingTarget = GrabClosestBuilding();

			if (unit.currentUnitTarget != null)
				SyncCurrentUnitTargetServerRPC(unit.EntityNetworkObjId, unit.currentUnitTarget.EntityNetworkObjId);
			if (unit.currentBuildingTarget != null)
				SyncCurrentBuildingTargetServerRPC(unit.EntityNetworkObjId, unit.currentBuildingTarget.EntityNetworkObjId);

			if (unit.targetList.Count == 0)
			{
				unit.ChangeStateIdleClientRPC();
				unit.ChangeStateIdleServerRPC(unit.EntityNetworkObjId);
			}
		}
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
	[ServerRpc(RequireOwnership = false)]
	public void ShootMainWeapServerRPC()
	{
		ShootMainWeaponClientRPC();
	}
	[ClientRpc]
	public void ShootMainWeaponClientRPC()
	{
		if (HasPlayerSetTarget() && unit.CheckIfInAttackRange(unit.playerSetTarget.transform.position) &&
			unit.CheckIfEntityInLineOfSight(unit.playerSetTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.playerSetTarget.CenterPoint.transform.position);
			if (IsServer)
				unit.playerSetTarget.RecieveDamageServerRPC(mainWeaponDamage.Value);
		}
		else if (HasUnitTarget() && unit.CheckIfInAttackRange(unit.currentUnitTarget.transform.position) &&
			unit.CheckIfEntityInLineOfSight(unit.currentUnitTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			if (IsServer)
				unit.currentUnitTarget.RecieveDamageServerRPC(mainWeaponDamage.Value);
		}
		else if (HasBuildingTarget() && unit.CheckIfInAttackRange(unit.currentBuildingTarget.transform.position) &&
			unit.CheckIfEntityInLineOfSight(unit.currentBuildingTarget))
		{
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", true);

			mainWeaponAudio.Play();
			mainWeaponParticles.Play();

			AimProjectileAtTarget(mainWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			if (IsServer)
				unit.currentBuildingTarget.RecieveDamageServerRPC(mainWeaponDamage.Value);
		}
		else
			TryFindTargets();
	}
	[ServerRpc(RequireOwnership = false)]
	public void ShootSeconWeapServerRPC()
	{
		ShootSecondaryWeaponClientRPC();
	}
	[ClientRpc]
	public void ShootSecondaryWeaponClientRPC()
	{
		if (HasPlayerSetTarget())
		{
			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();

			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.playerSetTarget.CenterPoint.transform.position);
			if (!IsServer) return;
			unit.playerSetTarget.RecieveDamageServerRPC(secondaryWeaponDamage.Value);
		}
		else if (HasUnitTarget())
		{
			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();

			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.currentUnitTarget.CenterPoint.transform.position);
			if (!IsServer) return;
			unit.currentUnitTarget.RecieveDamageServerRPC(secondaryWeaponDamage.Value);
		}
		else if (HasBuildingTarget())
		{
			secondaryWeaponAudio.Play();
			secondaryWeaponParticles.Play();

			AimProjectileAtTarget(secondaryWeaponParticles.gameObject, unit.currentBuildingTarget.CenterPoint.transform.position);
			if (!IsServer) return;
			unit.currentBuildingTarget.RecieveDamageServerRPC(secondaryWeaponDamage.Value);
		}
	}

	//UTILITY FUNCTIONS
	[ServerRpc(RequireOwnership = false)]
	public void SyncPlayerSetTargetServerRPC(ulong unitId, ulong PlayerSetTargetId)
	{
		SyncPlayerSetTargetClientRPC(unitId, PlayerSetTargetId);
	}
	[ClientRpc]
	public void SyncPlayerSetTargetClientRPC(ulong unitId, ulong PlayerSetTargetId)
	{
		UnitStateController unit = NetworkManager.SpawnManager.SpawnedObjects[unitId].GetComponent<UnitStateController>();
		Entities entity = NetworkManager.SpawnManager.SpawnedObjects[PlayerSetTargetId].GetComponent<UnitStateController>();
		unit.playerSetTarget = entity;
	}
	[ServerRpc(RequireOwnership = false)]
	public void SyncCurrentUnitTargetServerRPC(ulong unitId, ulong UnitTargetId)
	{
		SyncCurrentUnitTargetClientRPC(unitId, UnitTargetId);
	}
	[ClientRpc]
	public void SyncCurrentUnitTargetClientRPC(ulong unitId, ulong UnitTargetId)
	{
		UnitStateController unit = NetworkManager.SpawnManager.SpawnedObjects[unitId].GetComponent<UnitStateController>();
		UnitStateController unitTarget = NetworkManager.SpawnManager.SpawnedObjects[UnitTargetId].GetComponent<UnitStateController>();
		unit.currentUnitTarget = unitTarget;
	}
	[ServerRpc(RequireOwnership = false)]
	public void SyncCurrentBuildingTargetServerRPC(ulong unitId, ulong BuildingTargetId)
	{
		SyncCurrentBuildingTargetClientRPC(unitId, BuildingTargetId);
	}
	[ClientRpc]
	public void SyncCurrentBuildingTargetClientRPC(ulong unitId, ulong BuildingTargetId)
	{
		UnitStateController unit = NetworkManager.SpawnManager.SpawnedObjects[unitId].GetComponent<UnitStateController>();
		BuildingManager BuildingTarget = NetworkManager.SpawnManager.SpawnedObjects[BuildingTargetId].GetComponent<BuildingManager>();
		unit.currentBuildingTarget = BuildingTarget;
	}
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
	public void MainGunTimer()
	{
		if (mainWeaponAttackSpeedTimer > 0)
			mainWeaponAttackSpeedTimer -= Time.deltaTime;
		else if (IsServer)
		{
			ShootMainWeapServerRPC();
			mainWeaponAttackSpeedTimer = mainWeaponAttackSpeed;
		}
	}
	public void SecondaryGunTimer()
	{
		if (secondaryWeaponAttackSpeedTimer > 0)
			secondaryWeaponAttackSpeedTimer -= Time.deltaTime;
		else if (IsServer)
		{
			if (unit.hasShootAnimation)
				StartCoroutine(DelaySecondaryAttack(1));
			else
				ShootSeconWeapServerRPC();

			secondaryWeaponAttackSpeedTimer = secondaryWeaponAttackSpeed;
		}
	}
	public IEnumerator DelaySecondaryAttack(float seconds)
	{
		secondaryWeaponAttackSpeedTimer++;
		secondaryWeaponAttackSpeedTimer %= secondaryWeaponAttackSpeed - 1;
		yield return new WaitForSeconds(seconds);
		ShootSeconWeapServerRPC();
	}
}
