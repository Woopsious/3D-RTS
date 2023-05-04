using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class UnitIdleState : UnitBaseState
{
	public override void Enter(UnitStateController unit)
	{
		//Debug.Log("Entered Idle State");
		if (unit.hasAnimation)
		{
			unit.animatorController.SetBool("isAttacking", false);
			unit.animatorController.SetBool("isIdle", true);
		}
		unit.audioSFXs[0].Stop();
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
