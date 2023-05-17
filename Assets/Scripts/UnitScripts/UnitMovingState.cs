using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using static UnityEngine.GraphicsBuffer;

public class UnitMovingState : UnitBaseState
{
	public override void Enter(UnitStateController unit)
	{
		Debug.Log("Entered Moving State");
		if (unit.hasRadar)
		{
			unit.audioSFXs[2].Stop();
			unit.audioSFXs[1].Play();
		}
		unit.movingSFX.Play();

		if (unit.unitName != "Scout Vehicle")
			unit.animatorController.SetBool("isIdle", true);

		if (unit.hasAnimation)
			unit.animatorController.SetBool("isAttacking", false);

		unit.agentNav.isStopped = false;
		if (CheckPath(unit))
			unit.agentNav.SetPath(unit.navMeshPath);

		else if (!CheckPath(unit))
			unit.agentNav.destination = unit.transform.position;

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
		CheckDistance(unit);
	}
	public void CheckDistance(UnitStateController unit)
	{
		if (Vector3.Distance(unit.transform.position, unit.movePos) < unit.agentNav.stoppingDistance)
		{
			unit.ChangeStateIdle();
			unit.agentNav.isStopped = true;
		}
	}
	public bool CheckPath(UnitStateController unit)
	{
		NavMeshPath path = new NavMeshPath();
		if (unit.agentNav.CalculatePath(unit.movePos, path))
		{
			unit.navMeshPath = path;
			Debug.Log("can find path");
			return true;
		}
		else
		{
			Debug.Log("cant find path");
			return false;
		}
	}
}
