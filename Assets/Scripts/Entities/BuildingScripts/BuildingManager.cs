using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Netcode;
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
		if (unpoweredBuildingIndicatorObj != null)
			unpoweredBuildingIndicatorObj.transform.SetParent(FindObjectOfType<Canvas>().transform);
	}
	public override void Update()
	{
		base.Update();

		if (unpoweredBuildingIndicatorObj != null && unpoweredBuildingIndicatorObj.activeInHierarchy)
			unpoweredBuildingIndicatorObj.transform.position = Camera.main.WorldToScreenPoint(
				gameObject.transform.position + new Vector3(0, 7f, 0));
	}
	public void OnBuildingStartUp()
	{
		//enable building triggers, navMeshObstacle, set layer and unhighlight
		GetComponent<BuildingManager>().enabled = true;

		if (isVTOLProdBuilding)
			GetComponent<SphereCollider>().isTrigger = true;
		else
			GetComponent<BoxCollider>().isTrigger = true;

		if (isPlayerOneEntity)
			gameObject.layer = LayerMask.NameToLayer("PlayerOneUnits");
		else
			gameObject.layer = LayerMask.NameToLayer("PlayerTwoUnits");

		GetComponent<CanPlaceBuilding>().highlighterObj.SetActive(false);
		GetComponent<CanPlaceBuilding>().navMeshObstacle.enabled = true;
		GetComponent<CanPlaceBuilding>().isPlaced = true;

		AddBuildingRefs();
		if (isPlayerOneEntity == FindObjectOfType<PlayerController>().isPlayerOne && unpoweredBuildingIndicatorObj != null)
		{
			Debug.LogWarning("pass");
			unpoweredBuildingIndicatorObj.SetActive(true);
		}

		if (isGeneratorBuilding)
			gameObject.GetComponent<EnergyGenController>().PowerBuildings();
		else if (!isGeneratorBuilding && capturePointController.energyGeneratorBuilding != null)
			capturePointController.energyGeneratorBuilding.GetComponent<EnergyGenController>().PowerBuildings();
	}

	//HEALTH/HIT FUNCTIONS OVERRIDES
	public override void TryDisplayEntityHitNotif()
	{
		if (!wasRecentlyHit && !IsPlayerControllerNull())
		{
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("BASE UNDER ATTACK", transform.position);
			AnnouncerSystem.Instance.PlayAlertBaseUnderAttackSFX();
		}
	}
	public override void OnEntityDeath()
	{
		if (!IsPlayerControllerNull())
		{
			GameManager.Instance.playerNotifsManager.DisplayEventMessage("BUILDING DESTROYED", transform.position);
			AnnouncerSystem.Instance.PlayAlertBuildingLostSFX();

			if (isHQ)
				GameManager.Instance.GameOverPlayerHQDestroyedServerRPC(playerController.isPlayerOne);
		}
		base.OnEntityDeath();
	}

	//UTILITY FUNCTIONS
	public void PowerBuilding()
	{
		isPowered = true;
		if (isPlayerOneEntity == FindObjectOfType<PlayerController>().isPlayerOne && unpoweredBuildingIndicatorObj != null)
		{
			Debug.LogWarning("power");
			unpoweredBuildingIndicatorObj.SetActive(false);
		}

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
		if (isPlayerOneEntity == FindObjectOfType<PlayerController>().isPlayerOne && unpoweredBuildingIndicatorObj != null)
		{
			Debug.LogWarning("unpower");
			unpoweredBuildingIndicatorObj.SetActive(true);
		}

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
		EnsureCapturePointRefIsNotNull();

		if (playerController != null)
			playerController.buildingListForPlayer.Add(this);

		if (isGeneratorBuilding)
			capturePointController.energyGeneratorBuilding = this;

		else if (isRefineryBuilding)
			capturePointController.RefinaryBuildings.Add(this);

		else if (isLightVehProdBuilding)
		{
			capturePointController.lightVehProdBuildings.Add(this);
			if (playerController != null)
				playerController.lightVehProdBuildingsList.Add(this);
		}
		else if (isHeavyVehProdBuilding)
		{
			capturePointController.heavyVehProdBuildings.Add(this);
			if (playerController != null)
				playerController.heavyVehProdBuildingsList.Add(this);
		}
		else if (isVTOLProdBuilding)
		{
			capturePointController.vtolProdBuildings.Add(this);
			if (playerController != null)
				playerController.vtolVehProdBuildingsList.Add(this);
		}
		else if (isHQ)
		{
			foreach (CapturePointController capturePoint in GameManager.Instance.gameUIManager.playerController.capturePointsList)
			{
				if (capturePoint.isPlayerOneSpawn)
					capturePoint.SetOwnershipBasedOnHq(this);
				else if (capturePoint.isPlayerTwoSpawn)
					capturePoint.SetOwnershipBasedOnHq(this);
			}
		}
	}
	public override void RemoveEntityRefs()
	{
		if (playerController != null)
		{
			playerController.buildingListForPlayer.Remove(this);
			if (this == playerController.unitSelectionManager.selectedBuilding)
				playerController.unitSelectionManager.selectedBuilding = null;
		}

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
			capturePointController.lightVehProdBuildings.Remove(this);
			if (playerController != null)
				playerController.lightVehProdBuildingsList.Remove(this);
		}
		else if (isHeavyVehProdBuilding)
		{
			capturePointController.heavyVehProdBuildings.Remove(this);
			if (playerController != null)
				playerController.heavyVehProdBuildingsList.Remove(this);
		}
		else if (isVTOLProdBuilding)
		{
			capturePointController.heavyVehProdBuildings.Remove(this);
			if (playerController != null)
				playerController.heavyVehProdBuildingsList.Remove(this);
		}
	}
	public void ShowRefundButton()
	{
		refundBuildingBackgroundObj.SetActive(true);
	}
	public void HideRefundButton()
	{
		refundBuildingBackgroundObj.SetActive(false);
	}
	public void EnsureCapturePointRefIsNotNull()
	{
		if (capturePointController == null)
		{
			CapturePointController[] capturePointsArray = FindObjectsOfType<CapturePointController>();
			List<CapturePointController> capturePoints = new List<CapturePointController>();

			capturePoints.AddRange(capturePointsArray);
			capturePoints = capturePoints.OrderBy(capPoint => Vector3.Distance(gameObject.transform.position, capPoint.transform.position)).ToList();
			capturePointController = capturePoints[0];
		}
	}
}
