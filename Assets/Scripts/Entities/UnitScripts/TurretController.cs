using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurretController : UnitStateController
{

	[Header("Turret Refs")]
	public CapturePointController capturePointController;
	public GameObject refundBuildingBackgroundObj;
	public GameObject turretTower;
	public GameObject turretGuns;

	public void ActivateTurret()
	{
		animatorController.SetBool("isIdle", false);
		audioSFXs[0].Play();

		weaponSystem.TryFindTargets();
	}
	public void DeactivateTurret()
	{
		animatorController.SetBool("isIdle", true);
		audioSFXs[1].Play();
	}

	//functions to make turret tower/guns aim at enemy targets
	public void FaceTarget(Entities entityTarget)
	{
		Vector3 lookDirection = new Vector3(entityTarget.transform.position.x, turretTower.transform.position.y, entityTarget.transform.position.z);
		var lookRotation = Quaternion.LookRotation(lookDirection - turretTower.transform.position);
		turretTower.transform.rotation = Quaternion.Slerp(turretTower.transform.rotation, lookRotation, 0.1f);
	}
	public void ChangeGunElevation(Entities entityTarget)
	{
		Vector3 lookDirection = new Vector3(entityTarget.transform.position.x, entityTarget.transform.position.y, entityTarget.transform.position.z);
		var lookRotation = Quaternion.LookRotation(lookDirection - turretGuns.transform.position);
		turretGuns.transform.rotation = Quaternion.Slerp(turretGuns.transform.rotation, lookRotation, 0.1f);
	}

	[ServerRpc(RequireOwnership = false)]
	public void LookAtTargetServerRPC()
	{
		LookAtTargetClientRPC();
	}
	[ClientRpc]
	public void LookAtTargetClientRPC()
	{
		FaceTargetTest();
		ChangeGunElevationTest();
	}
	public void FaceTargetTest()
	{
		if (playerSetTarget != null)
		{
			Vector3 lookDirection = new Vector3(playerSetTarget.transform.position.x, turretTower.transform.position.y, playerSetTarget.transform.position.z);
			var lookRotation = Quaternion.LookRotation(lookDirection - turretTower.transform.position);
			turretTower.transform.rotation = Quaternion.Slerp(turretTower.transform.rotation, lookRotation, 0.1f);
		}
		else
		{
			Vector3 lookDirection = new Vector3(currentUnitTarget.transform.position.x, turretTower.transform.position.y, currentUnitTarget.transform.position.z);
			var lookRotation = Quaternion.LookRotation(lookDirection - turretTower.transform.position);
			turretTower.transform.rotation = Quaternion.Slerp(turretTower.transform.rotation, lookRotation, 0.1f);
		}
	}
	public void ChangeGunElevationTest()
	{
		if (playerSetTarget != null)
		{
			Vector3 lookDirection = new Vector3(playerSetTarget.transform.position.x, playerSetTarget.transform.position.y, playerSetTarget.transform.position.z);
			var lookRotation = Quaternion.LookRotation(lookDirection - turretGuns.transform.position);
			turretGuns.transform.rotation = Quaternion.Slerp(turretGuns.transform.rotation, lookRotation, 0.1f);
		}
		else
		{
			Vector3 lookDirection = new Vector3(currentUnitTarget.transform.position.x, currentUnitTarget.transform.position.y, currentUnitTarget.transform.position.z);
			var lookRotation = Quaternion.LookRotation(lookDirection - turretGuns.transform.position);
			turretGuns.transform.rotation = Quaternion.Slerp(turretGuns.transform.rotation, lookRotation, 0.1f);
		}
	}
	public void AddTurretRefs()
	{
		capturePointController.TurretDefenses.Add(this);
	}
}
