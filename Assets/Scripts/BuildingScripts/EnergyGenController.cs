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
		//StartCoroutine(UpdatePoweredBuildings());
	}
	IEnumerator UpdatePoweredBuildings()
	{
		yield return new WaitForSeconds(0.5f);
		PowerBuildings();
		//Debug.Log("powering buildings");
	}
	public void PowerBuildings()
	{
		if (!buildingRef.capturePointController.HQRef.isPowered)
			buildingRef.capturePointController.HQRef.PowerBuilding();

		foreach (BuildingManager building in buildingRef.capturePointController.RefinaryBuildings)
		{
			if (!building.isPowered && buildingRef.capturePointController.RefinaryBuildings.Count != 0)
				building.PowerBuilding();
		}
		foreach (BuildingManager building in buildingRef.capturePointController.lightVehProdBuildings)
		{
			if (!building.isPowered && buildingRef.capturePointController.lightVehProdBuildings.Count != 0)
				building.PowerBuilding();
		}
		foreach (BuildingManager building in buildingRef.capturePointController.heavyVehProdBuildings)
		{
			if (!building.isPowered && buildingRef.capturePointController.heavyVehProdBuildings.Count != 0)
				building.PowerBuilding();
		}
		foreach (BuildingManager building in buildingRef.capturePointController.vtolProdBuildings)
		{
			if(!building.isPowered && buildingRef.capturePointController.vtolProdBuildings.Count != 0)
				building.PowerBuilding();
		}
	}
	public void UnpowerBuildings()
	{
		foreach (BuildingManager building in buildingRef.capturePointController.RefinaryBuildings)
			building.UnpowerBuilding();
		foreach (BuildingManager building in buildingRef.capturePointController.lightVehProdBuildings)
			building.UnpowerBuilding();
		foreach (BuildingManager building in buildingRef.capturePointController.heavyVehProdBuildings)
			building.UnpowerBuilding();
		foreach (BuildingManager building in buildingRef.capturePointController.vtolProdBuildings)
			building.UnpowerBuilding();
	}
}
