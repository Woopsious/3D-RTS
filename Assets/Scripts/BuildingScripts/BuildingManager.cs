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
	public GameObject refundBuildingBackgroundObj;
	public GameObject unpoweredBuildingIndicatorObj;


	[Header("Building Production Stats")]
	public int moneyProduction;
	public int alloyProduction;
	public int crystalProduction;
	public int unitBuildTimeBoost;

	[Header("Building Bools")]
	public bool isPowered;
	public bool isHQ;
	public bool isLightVehProdBuilding;
	public bool isHeavyVehProdBuilding;
	public bool isVTOLProdBuilding;
	public bool isRefineryBuilding;
	public bool isGeneratorBuilding;

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
	public void ShowRefundButton()
	{
		refundBuildingBackgroundObj.SetActive(true);
	}
	public void HideRefundButton()
	{
		refundBuildingBackgroundObj.SetActive(false);
	}

	//HEALTH/HIT FUNCTIONS OVERRIDES
	public override void TryDisplayEntityHitNotif()
	{
		if (!wasRecentlyHit && ShouldDisplaySpottedNotifToPlayer())
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("BUILDING UNDER ATTACK", transform.position);
	}
	public override void OnEntityDeath()
	{
		base.OnEntityDeath();
		if (ShouldDisplaySpottedNotifToPlayer())
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("BUILDING DESTROYED", transform.position);
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
		playerController.buildingListForPlayer.Add(this);

		if (isGeneratorBuilding)
		{
			capturePointController.energyGeneratorBuilding = this;
		}
		else if (isRefineryBuilding)
		{
				capturePointController.RefinaryBuildings.Add(this);
		}
		else if (isLightVehProdBuilding)
		{
			playerController.lightVehProdBuildingsList.Add(this);
				capturePointController.lightVehProdBuildings.Add(this);
		}
		else if (isHeavyVehProdBuilding)
		{
			playerController.heavyVehProdBuildingsList.Add(this);
				capturePointController.heavyVehProdBuildings.Add(this);
		}
		else if (isVTOLProdBuilding)
		{
			playerController.vtolVehProdBuildingsList.Add(this);
				capturePointController.vtolProdBuildings.Add(this);
		}
	}
	public override void RemoveEntityRefs()
	{
		playerController.buildingListForPlayer.Remove(this);

		if (isGeneratorBuilding)
		{
			GetComponent<EnergyGenController>().UnpowerBuildings();
			capturePointController.energyGeneratorBuilding = null;
		}
		else if (isRefineryBuilding)
		{
			if (GetComponent<RefineryController>().CargoShipList.Count == 2)
				GetComponent<RefineryController>().CargoShipList[1].DeleteSelf();
			if (GetComponent<RefineryController>().CargoShipList.Count == 1)
				GetComponent<RefineryController>().CargoShipList[0].DeleteSelf();

			capturePointController.RefinaryBuildings.Remove(this);			
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
