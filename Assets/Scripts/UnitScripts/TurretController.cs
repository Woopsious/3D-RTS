using System.Collections;
using System.Collections.Generic;
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
		weaponSystem.TryFindTarget();
	}
	public void DeactivateTurret()
	{
		animatorController.SetBool("isIdle", true);
		audioSFXs[1].Play();
	}
	public override void TryAttackPlayerSetTarget(Entities entity)
	{
		if (IsPlayerSetTargetSpotted(entity)) //check if already spotted in target lists
			playerSetTarget = entity;
		else //let player know its out of view/range of turret
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Target Out Of View", 1.5f);
	}
	public void FaceTarget(Entities entityTarget)
	{
		Vector3 lookDirection = new Vector3(entityTarget.transform.position.x, transform.position.y, entityTarget.transform.position.z);

		var lookRotation = Quaternion.LookRotation(lookDirection - transform.position);
		transform.rotation = Quaternion.Slerp(turretTower.transform.rotation, lookRotation, 1 / 1000);
	}
	public void ChangeGunElevation(Entities entityTarget)
	{
		Vector3 lookDirection = new Vector3(transform.position.x, entityTarget.transform.position.y, transform.position.z);

		var lookRotation = Quaternion.LookRotation(lookDirection - transform.position);
		transform.rotation = Quaternion.Slerp(turretGuns.transform.rotation, lookRotation, 1 / 1000);
	}
	public void AddTurretRefs()
	{
		capturePointController.TurretDefenses.Add(this);
	}
}
