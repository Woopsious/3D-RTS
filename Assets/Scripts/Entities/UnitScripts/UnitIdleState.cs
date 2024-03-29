using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class UnitIdleState : UnitBaseState
{
	public override void Enter(UnitStateController unit)
	{
		if (!unit.isTurret)
		{
			if (unit.hasRadar)
			{
				unit.audioSFXs[1].Stop();
				unit.audioSFXs[2].Play();
			}
			unit.movingSFX.Stop();

			if (unit.hasMoveAnimation)
				unit.animatorController.SetBool("isIdle", true);
			if (unit.hasShootAnimation)
				unit.animatorController.SetBool("isAttacking", false);
		}
		else
			unit.turretController.DeactivateTurret();
	}
	public override void Exit(UnitStateController unit)
	{

	}
	public override void UpdateLogic(UnitStateController unit)
	{

	}
	public override void UpdatePhysics(UnitStateController unit)
	{

	}
}
