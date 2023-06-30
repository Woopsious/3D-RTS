using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

public class BuildingManager : Entities
{
	[Header("Building Refs")]
	public bool isPowered;
	public bool isHQ;
	public bool isLightVehProdBuilding;
	public bool isHeavyVehProdBuilding;
	public bool isVTOLProdBuilding;
	public bool isRefineryBuilding;
	public bool isGeneratorBuilding;
	public GameObject refundBuildingButton;


	[Header("Building Production Stats")]
	public int moneyProduction;
	public int alloyProduction;
	public int crystalProduction;
	public int unitBuildTimeBoost;

	[Header("Building Dynamic Refs")]
	public CapturePointController capturePointController;

	public override void Start()
	{
		base.Start();

		if (isGeneratorBuilding)
			gameObject.GetComponent<EnergyGenController>().PowerBuildings();
		else if (!isGeneratorBuilding && capturePointController.energyGeneratorBuilding != null)
			capturePointController.energyGeneratorBuilding.GetComponent<EnergyGenController>().PowerBuildings();
	}
	public override void Update()
	{
		base.Update();
	}
	public IEnumerator HideUi()
	{
		yield return new WaitForSeconds(0.1f);
		HideUIHealthBar();
	}

	//UTILITY FUNCTIONS
	public void PowerBuilding()
	{
		isPowered = true;

		if (isRefineryBuilding)
		{
			RefineryController refineryController = gameObject.GetComponent<RefineryController>();
			refineryController.CheckCargoShipsCount();

			if (refineryController.CargoShipList.Count != 0)
			{
				foreach (CargoShipController cargoShip in refineryController.CargoShipList)
					cargoShip.ContinueMining();
			}
		}
	}
	public void UnpowerBuilding()
	{
		isPowered = false;

		if (isRefineryBuilding)
		{
			RefineryController refineryController = gameObject.GetComponent<RefineryController>();

			if (refineryController.CargoShipList.Count != 0)
			{
				foreach (CargoShipController cargoShip in refineryController.CargoShipList)
					cargoShip.PauseMining();
			}
		}
	}
	public void AddBuildingRefs()
	{
		if (isGeneratorBuilding)
		{
			capturePointController.energyGeneratorBuilding = this;
		}
		else if (isRefineryBuilding)
		{
			if (!capturePointController.RefinaryBuildings.Contains(this))
				capturePointController.RefinaryBuildings.Add(this);
		}
		else if (isLightVehProdBuilding)
		{
			if (!capturePointController.lightVehProdBuildings.Contains(this))
				capturePointController.lightVehProdBuildings.Add(this);
		}
		else if (isHeavyVehProdBuilding)
		{
			if (!capturePointController.heavyVehProdBuildings.Contains(this))
				capturePointController.heavyVehProdBuildings.Add(this);
		}
	}
	public override void RemoveEntityRefs()
	{
		if (isGeneratorBuilding)
		{
			GetComponent<EnergyGenController>().UnpowerBuildings();
			capturePointController.energyGeneratorBuilding = null;
		}
		else if (isRefineryBuilding)
		{
			try  //on the off chance one or both cargoships are already dead
			{
				GetComponent<RefineryController>().CargoShipList[1].DeleteSelf();
				GetComponent<RefineryController>().CargoShipList[0].DeleteSelf();
				capturePointController.RefinaryBuildings.Remove(this);
			}
			catch (Exception e)
			{
				throw e;
			}
			
		}
		else if (isLightVehProdBuilding)
		{
			playerController.lightVehProdBuildingsList.Remove(this);
			capturePointController.lightVehProdBuildings.Remove(this);
		}
		else if (isHeavyVehProdBuilding)
		{
			playerController.heavyVehProdBuildingsList.Remove(this);
			capturePointController.heavyVehProdBuildings.Remove(this);
		}
		else if (isVTOLProdBuilding)
		{
			playerController.heavyVehProdBuildingsList.Remove(this);
			capturePointController.heavyVehProdBuildings.Remove(this);
		}
	}
}
