using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePointController : MonoBehaviour
{
	[Header("Bool Refs")]
	public bool isNeutralPoint;
	public bool isPlayerOnePoint;
	public bool isPlayerTwoPoint;

	[Header("Optional Refs")]
	public BuildingManager HQRef;

	[Header("Dynamic Refs")] 
	public BuildingManager energyGeneratorBuilding;

	public List<BuildingManager> RefinaryBuildings;
	public List<BuildingManager> lightVehProdBuildings;
	public List<BuildingManager> heavyVehProdBuildings;
	public List<BuildingManager> vtolProdBuildings;

	public List<UnitStateController> playerOneUnitList;
	public List<UnitStateController> playerTwoUnitList;

	public void Update()
	{
		StartCoroutine(TrackPointOwnerShip());
	}
	//check if buildings exist, check list of player units to fip point ownership 
	public IEnumerator TrackPointOwnerShip()
	{// check for buildings
		if (HQRef == null && energyGeneratorBuilding == null && RefinaryBuildings.Count == 0 && lightVehProdBuildings.Count == 0 && 
			heavyVehProdBuildings.Count == 0 && vtolProdBuildings.Count == 0)
		{
			//check for units in building area
			if (playerOneUnitList.Count == 0 && playerTwoUnitList.Count == 0)
			{
				isNeutralPoint = true;
				isPlayerOnePoint = false;
				isPlayerTwoPoint = false;

				//code to notify players
			}
			if (playerOneUnitList.Count != 0 && playerTwoUnitList.Count == 0)
			{
				isNeutralPoint = false;
				isPlayerOnePoint = true;
				isPlayerTwoPoint = false;

				//code to notify players
			}
			else if (playerOneUnitList.Count == 0 && playerTwoUnitList.Count != 0)
			{
				isNeutralPoint = false;
				isPlayerOnePoint = false;
				isPlayerTwoPoint = true;

				//code to notify players
			}
			yield return new WaitForSeconds(1);
		}
		else
			yield return new WaitForSeconds(1);
	}

	public void OnTriggerEnter(Collider other)
	{
		GrabUnitRefs(other);
	}
	public void OnTriggerExit(Collider other)
	{
		RemoveUnitRefs(other);
	}

	//track units in area
	public void GrabUnitRefs(Collider other)
	{
		if (other.gameObject.GetComponent<UnitStateController>())
		{
			UnitStateController unit = other.GetComponent<UnitStateController>();

			if (unit.isPlayerOneEntity)
				playerOneUnitList.Add(unit);
			else if (!unit.isPlayerOneEntity)
				playerTwoUnitList.Add(unit);
		}
	}
	public void RemoveUnitRefs(Collider other)
	{
		if (other.gameObject.GetComponent<UnitStateController>())
		{
			UnitStateController unit = other.GetComponent<UnitStateController>();

			if (unit.isPlayerOneEntity)
				playerOneUnitList.Remove(unit);
			else if (!unit.isPlayerOneEntity)
				playerTwoUnitList.Remove(unit);
		}
	}
}
