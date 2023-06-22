using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

public class BuildingManager : Entities
{
	public CapturePointController capturePointController;

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

	public override void Start()
	{
		base.Start();
	}
	public override void Update()
	{
		base.Update();
	}
	public IEnumerator HideUi()
	{
		yield return new WaitForSeconds(0.1f);
		UiObj.SetActive(false);
	}

	//UTILITY FUNCTIONS
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
			capturePointController.energyGeneratorBuilding = null;
			GetComponent<EnergyGenController>().UnpowerBuildings();
		}
		else if (isRefineryBuilding)
		{
			capturePointController.RefinaryBuildings.Remove(this);
		}
		else if (isLightVehProdBuilding)
		{
			capturePointController.lightVehProdBuildings.Remove(this);
			playerController.lightVehProdBuildingsList.Remove(this);
		}
		else if (isHeavyVehProdBuilding)
		{
			capturePointController.heavyVehProdBuildings.Remove(this);
			playerController.heavyVehProdBuildingsList.Remove(this);
		}
		else if (isVTOLProdBuilding)
		{
			capturePointController.heavyVehProdBuildings.Remove(this);
			playerController.heavyVehProdBuildingsList.Remove(this);
		}
	}
}
