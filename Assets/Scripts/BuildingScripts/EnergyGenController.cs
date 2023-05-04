using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyGenController : MonoBehaviour
{
	public BuildingManager buildingRef;
    CapturePointController pointController;
	public bool isActive;

	//on start every 3 secs if buildings in cap point are unpowered power them
	public void Start()
	{
		StartCoroutine(UpdatePoweredBuildings());
	}
	IEnumerator UpdatePoweredBuildings()
	{
		PowerBuildings();
		yield return new WaitForSeconds(3f);
		//call itself again after 3 seconds
		StartCoroutine(UpdatePoweredBuildings());
	}
	public void PowerBuildings()
	{
		try
		{
			if (!pointController.HQRef.isPowered)
			{
				pointController.HQRef.isPowered = true;
				//pointController.HQRef.UpdateProductionIncome(pointController.HQRef.moneyProduction,
					//pointController.HQRef.alloyProduction, pointController.HQRef.crystalProduction);
				buildingRef.playerController.gameUIManager.UpdateCurrentResourcesUI();
			}

			foreach (BuildingManager building in pointController.RefinaryBuildings)
			{
				if (!building.isPowered)
				{
					building.isPowered = true;
					//building.UpdateProductionIncome(building.moneyProduction, building.alloyProduction, building.crystalProduction);
					building.playerController.gameUIManager.UpdateCurrentResourcesUI();
				}
			}
			foreach (BuildingManager building in pointController.lightVehProdBuildings)
			{
				if (!building.isPowered)
					building.isPowered = true;
			}
			foreach (BuildingManager building in pointController.heavyVehProdBuildings)
			{
				if (!building.isPowered)
					building.isPowered = true;
			}
			foreach (BuildingManager building in pointController.vtolProdBuildings)
			{
				if(!building.isPowered)
					building.isPowered = true;
			}
		}
		catch
		{
			//do nothing
		}
	}
	public void UnpowerBuildings()
	{
		pointController.HQRef.isPowered = false;
		//pointController.HQRef.UpdateProductionIncome(-pointController.HQRef.moneyProduction, 
			//-pointController.HQRef.alloyProduction, -pointController.HQRef.crystalProduction);
		//buildingRef.playerController.gameUIManager.UpdateCurrentResourcesUI();

		foreach (BuildingManager building in pointController.RefinaryBuildings)
		{
			building.isPowered = false;
			//building.UpdateProductionIncome(-building.moneyProduction, -building.alloyProduction, -building.crystalProduction);
			//building.playerController.gameUIManager.UpdateCurrentResourcesUI();
		}
		foreach (BuildingManager building in pointController.lightVehProdBuildings)
		{
			building.isPowered = false;
		}
		foreach (BuildingManager building in pointController.heavyVehProdBuildings)
		{
			building.isPowered = false;
		}
		foreach (BuildingManager building in pointController.vtolProdBuildings)
		{
			building.isPowered = false;
		}
	}

	//track if colliding with capture point
	public void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<CapturePointController>())
			pointController = other.GetComponent<CapturePointController>();
	}
	public void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<CapturePointController>())
			pointController = null;
	}

}
