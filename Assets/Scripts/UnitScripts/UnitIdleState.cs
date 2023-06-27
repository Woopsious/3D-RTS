using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class UnitIdleState : UnitBaseState
{
	public override void Enter(UnitStateController unit)
	{
		//Debug.Log("Entered Idle State");
		if (unit.hasRadar)
		{
			unit.audioSFXs[1].Stop();
			unit.audioSFXs[2].Play();
		}
		unit.movingSFX.Stop();

		if (unit.unitName != "Scout Vehicle")
			unit.animatorController.SetBool("isIdle", true);

		if (unit.hasShootAnimation)
			unit.animatorController.SetBool("isAttacking", false);

		//unit.HideEntity();
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
