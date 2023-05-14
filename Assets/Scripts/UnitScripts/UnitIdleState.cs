using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class UnitIdleState : UnitBaseState
{
	public override void Enter(UnitStateController unit)
	{
		Debug.Log("Entered Idle State");
		unit.animatorController.SetBool("isIdle", true);
		if (unit.hasAnimation)
			unit.animatorController.SetBool("isAttacking", false);

		unit.movingSFX.Stop();
		if (unit.hasRadar)
		{
			unit.audioSFXs[1].Stop();
			unit.audioSFXs[2].Play();
		}
		unit.HideUnit();
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
