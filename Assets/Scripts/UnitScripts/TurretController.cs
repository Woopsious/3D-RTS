using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : UnitStateController
{
	public GameObject turretTower;
	public GameObject turretGuns;

	public void FaceTarget(Entities entityTarget)
	{
		Vector3 lookDirection = new Vector3(entityTarget.transform.position.x, transform.position.y, entityTarget.transform.position.z);

		var lookRotation = Quaternion.LookRotation(lookDirection - transform.position);
		transform.rotation = Quaternion.Slerp(turretTower.transform.rotation, lookRotation, 1);
	}
	public void ChangeGunElevation(Entities entityTarget)
	{
		Vector3 lookDirection = new Vector3(transform.position.x, entityTarget.transform.position.y, transform.position.z);

		var lookRotation = Quaternion.LookRotation(lookDirection - transform.position);
		transform.rotation = Quaternion.Slerp(turretGuns.transform.rotation, lookRotation, 1);
	}
}
