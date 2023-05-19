using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyGenController : MonoBehaviour
{
	public BuildingManager buildingRef;
	public bool isActive;

	//every 3 secs if buildings in cap point are unpowered power them
	public void StartPower()
	{
		StartCoroutine(UpdatePoweredBuildings());
	}
	IEnumerator UpdatePoweredBuildings()
	{
		//Debug.Log("powering buildings");
		PowerBuildings();
		yield return new WaitForSeconds(3f);
		//call itself again after 3 seconds
		StartCoroutine(UpdatePoweredBuildings());
	}
	public void PowerBuildings()
	{
		if (!buildingRef.capturePointController.HQRef.isPowered)
			buildingRef.capturePointController.HQRef.isPowered = true;

		foreach (BuildingManager building in buildingRef.capturePointController.RefinaryBuildings)
		{
			if (!building.isPowered && buildingRef.capturePointController.RefinaryBuildings.Count != 0)
				building.isPowered = true;
		}
		foreach (BuildingManager building in buildingRef.capturePointController.lightVehProdBuildings)
		{
			if (!building.isPowered && buildingRef.capturePointController.lightVehProdBuildings.Count != 0)
				building.isPowered = true;
		}
		foreach (BuildingManager building in buildingRef.capturePointController.heavyVehProdBuildings)
		{
			if (!building.isPowered && buildingRef.capturePointController.heavyVehProdBuildings.Count != 0)
				building.isPowered = true;
		}
		foreach (BuildingManager building in buildingRef.capturePointController.vtolProdBuildings)
		{
			if(!building.isPowered && buildingRef.capturePointController.vtolProdBuildings.Count != 0)
				building.isPowered = true;
		}
	}
	public void UnpowerBuildings()
	{
		foreach (BuildingManager building in buildingRef.capturePointController.RefinaryBuildings)
			building.isPowered = false;
		foreach (BuildingManager building in buildingRef.capturePointController.lightVehProdBuildings)
			building.isPowered = false;
		foreach (BuildingManager building in buildingRef.capturePointController.heavyVehProdBuildings)
			building.isPowered = false;
		foreach (BuildingManager building in buildingRef.capturePointController.vtolProdBuildings)
			building.isPowered = false;
	}
}
