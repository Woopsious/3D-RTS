using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

public class UnitStateAttacking : UnitBaseState
{
	public override void Enter(UnitStateController unit)
	{
		//Debug.Log("Entered Attacking State");
		unit.ShowUnit();
	}
	public override void Exit(UnitStateController unit)
	{

	}
	public override void UpdateLogic(UnitStateController unit)
	{
		unit.attackSpeedTimer += Time.deltaTime;
		if (unit.attackSpeedTimer >= unit.attackSpeed)
		{
			unit.attackSpeedTimer++;
			unit.attackSpeedTimer %= unit.attackSpeed;
			GetTargetList(unit);
		}
	}
	public override void UpdatePhysics(UnitStateController unit)
	{
		//continue to last movement destination, only look at target thats within attack range
		if(unit.currentUnitTarget != null)
		{
			if (CheckIfInAttackRange(unit, unit.currentUnitTarget.transform.position))
				StopAndLookAtTarget(unit);
		}
		else if (unit.currentBuildingTarget != null) 
		{
			if (CheckIfInAttackRange(unit, unit.currentUnitTarget.transform.position))
				StopAndLookAtTarget(unit);
		}
		if (Vector3.Distance(unit.transform.position, unit.movePos) > unit.agentNav.stoppingDistance && unit.movePos != new Vector3(0, 0, 0))
		{
			unit.animatorController.SetBool("isIdle", false);
		}
		else
		{
			unit.movePos = new Vector3(0, 0, 0);
			unit.animatorController.SetBool("isIdle", true);
			if (unit.audioSFXs[0].isPlaying)
				unit.audioSFXs[0].Stop();
		}
	}
	public void StopAndLookAtTarget(UnitStateController unit)
	{
		if (unit.currentUnitTarget != null)
		{
			var lookRotation = Quaternion.LookRotation(unit.currentUnitTarget.transform.position - unit.transform.position);
			unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, unit.agentNav.angularSpeed / 1000);
		}
		if (unit.currentUnitTarget == null && unit.currentBuildingTarget != null)
		{
			var lookRotation = Quaternion.LookRotation(unit.currentBuildingTarget.transform.position - unit.transform.position);
			unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, unit.agentNav.angularSpeed / 1000);
		}
	}

	//grab everything in attack range, if x component exists check if its not already in list, if not add it to list, else ignore it
	public void GetTargetList(UnitStateController unit)
	{
		Collider[] newTargetArray = Physics.OverlapSphere(unit.transform.position, unit.ViewRange); //find targets in attack range
		//check what side unit is on, check if unit is already in target list
		foreach (Collider collider in newTargetArray)
		{
			if (collider.GetComponent<UnitStateController>() != null && unit.isPlayerOneUnit != collider.GetComponent<UnitStateController>().isPlayerOneUnit)
			{
				if(!unit.unitTargetList.Contains(collider.GetComponent<UnitStateController>()))
				{
					unit.unitTargetList.Add(collider.GetComponent<UnitStateController>());
					collider.GetComponent<UnitStateController>().ShowUnit();
				}
			}
			if (collider.GetComponent<BuildingManager>() != null && unit.isPlayerOneUnit != collider.GetComponent<BuildingManager>().isPlayerOneBuilding)
			{
				if (!unit.buildingTargetList.Contains(collider.GetComponent<BuildingManager>()))
				{
					unit.buildingTargetList.Add(collider.GetComponent<BuildingManager>());
				}
			}
		}
		unit.currentUnitTarget = GrabClosestUnit(unit);
		unit.currentBuildingTarget = GrabClosestBuilding(unit);

		ShootTarget(unit);
	}
	//sort targets from closest to furthest, then check if target is in view, once a target is found and in view, return that unit and end loop
	public UnitStateController GrabClosestUnit(UnitStateController unit)
	{
		foreach (UnitStateController listedUnit in unit.unitTargetList)
		{
			if (listedUnit == null)
				unit.unitTargetList.Remove(listedUnit);
		}
		unit.unitTargetList = unit.unitTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.unitTargetList.Count; i++)
		{
			Physics.Linecast(unit.CenterPoint.transform.position, unit.unitTargetList[i].GetComponent<UnitStateController>().CenterPoint.transform.position,
			out RaycastHit hit, unit.ignoreMe);

			if (hit.collider.GetComponent<UnitStateController>() != null)
			{
				return unit.unitTargetList[i];
			}
		}
		return null;
	}
	public BuildingManager GrabClosestBuilding(UnitStateController unit)
	{
		foreach (BuildingManager listedBuilding in unit.buildingTargetList)
		{
			if (listedBuilding == null)
				unit.buildingTargetList.Remove(listedBuilding);
		}
		unit.buildingTargetList = unit.buildingTargetList.OrderBy(newtarget => Vector3.Distance(unit.transform.position, newtarget.transform.position)).ToList();

		for (int i = 0; i < unit.buildingTargetList.Count; i++)
		{
			Physics.Linecast(unit.CenterPoint.transform.position, unit.buildingTargetList[i].GetComponent<BuildingManager>().CenterPoint.transform.position,
			out RaycastHit hit, unit.ignoreMe);

			if (hit.collider.GetComponent<BuildingManager>() != null)
			{
				return unit.buildingTargetList[i];
			}
		}
		return null;
	}
	//shoot at target if it exists, prioritising units first
	public void ShootTarget(UnitStateController unit)
	{
		//start attacking current target
		if (unit.hasAnimation)
		{
			unit.animatorController.SetBool("isAttacking", true);
		}

		if (unit.currentUnitTarget != null)
		{
			//float unitDistance = Vector3.Distance(unit.transform.position, unit.currentUnitTarget.transform.position);
			if (CheckIfInAttackRange(unit, unit.currentUnitTarget.transform.position))
			{
				unit.audioSFXs[1].Play();
				unit.currentUnitTarget.RecieveDamage(unit.damage);
				unit.unitTargetList.Clear();
			}
		}
		else if (unit.currentUnitTarget == null && unit.currentBuildingTarget != null)
		{
			//float buildingDistance = Vector3.Distance(unit.transform.position, unit.currentBuildingTarget.transform.position);
			if(CheckIfInAttackRange(unit, unit.currentBuildingTarget.transform.position))
			{
				unit.audioSFXs[1].Play();
				unit.currentBuildingTarget.RecieveDamage(unit.damage);
				unit.buildingTargetList.Clear();
			}
		}
		//enter corrisponding state if no targets where found and set
		else if(unit.currentUnitTarget == null && unit.currentBuildingTarget == null && unit.navMeshPath != null)
		{
			unit.ChangeStateMoving();
		}
		else if (unit.currentUnitTarget == null && unit.currentBuildingTarget == null && unit.navMeshPath == null)
		{
			unit.ChangeStateIdle();
		}
	}
	public bool CheckIfInAttackRange(UnitStateController unit, Vector3 targetPos)
	{
		float Distance = Vector3.Distance(unit.transform.position, targetPos);

		if(Distance <= unit.attackRange)
			return true;
		else
			return false;
	}
}
