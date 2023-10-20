using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CanPlaceBuilding : NetworkBehaviour
{
	public CapturePointController pointController;

	public readonly float placementHeightMax = 11f;
	public readonly float placementHeightMin = 8f;

	public TurretController turret;
	public BuildingManager building;
	public GameObject highlighterObj;
	public NavMeshObstacle navMeshObstacle;
	public bool isPlayerOneBuilding;
	public bool isCollidingWithAnotherBuilding;
	public bool canPlace;
	public bool isPlaced;

	public float timer;

	public void Start()
	{
		if (building != null && !building.isHQ)
			SetUpBuildingOnStartUp(building.GetComponent<Entities>());
		else if (building != null && building.isHQ)
			Invoke(nameof(SetUpHQ), 1);
		else
			SetUpTurretOnStartUp(turret.GetComponent<Entities>());
	}
	public void Update()
	{
		if (pointController != null && !isPlaced)
		{
			timer -= Time.deltaTime;

			if (timer < 0)
			{
				timer = 0.25f;
				UpdateHighlighterColour();
			}
		}
	}
	public void SetUpHQ()
	{
		SetUpBuildingOnStartUp(building.GetComponent<Entities>());
	}
	public void SetUpBuildingOnStartUp(Entities entity)
	{
		if (!building.isHQ)
			highlighterObj.SetActive(true);
		CanPlaceHighliterRed();

		PlayerController playerCon = FindObjectOfType<PlayerController>(); //set player refs here
		if (building.isPlayerOneEntity)
		{
			Debug.LogWarning("Player One HQ: " + playerCon.isPlayerOne);
		}
		else if (!building.isPlayerOneEntity)
		{
			Debug.LogWarning("Player Two HQ: " + playerCon.isPlayerOne);
		}

		if (playerCon.isPlayerOne != !entity.isPlayerOneEntity)
		{
			entity.playerController = playerCon;

			if (!building.isHQ)
			{
				playerCon.buildingPlacementManager.currentBuildingPlacement = entity;
				playerCon.buildingPlacementManager.canPlaceBuilding = this;
				playerCon.buildingPlacementManager.currentBuildingPlacementNetworkId =
					entity.GetComponent<NetworkObject>().NetworkObjectId;
			}
		}
		//set entity Minimap layer and colour
		if (building.isPlayerOneEntity)
			building.miniMapRenderObj.layer = 11;
		else
			building.miniMapRenderObj.layer = 12;

		if (building.playerController != null)
			building.miniMapRenderObj.GetComponent<SpriteRenderer>().color = Color.green;
		else
			building.miniMapRenderObj.GetComponent<SpriteRenderer>().color = Color.red;

		if (IsServer && playerCon.isPlayerOne)
			GameManager.Instance.playerBuildingsList.Add(GetComponent<BuildingManager>());
	}
	public void SetUpTurretOnStartUp(Entities entity)
	{
		highlighterObj.SetActive(true);
		CanPlaceHighliterRed();

		PlayerController playerCon = FindObjectOfType<PlayerController>(); //set player refs here

		if (playerCon.isPlayerOne != !entity.isPlayerOneEntity)
		{
			entity.playerController = playerCon;

			playerCon.buildingPlacementManager.currentBuildingPlacement = entity;
			playerCon.buildingPlacementManager.canPlaceBuilding = this;
			playerCon.buildingPlacementManager.currentBuildingPlacementNetworkId = entity.GetComponent<NetworkObject>().NetworkObjectId;
		}
		//set entity Minimap layer and colour
		if (turret.isPlayerOneEntity)
			turret.miniMapRenderObj.layer = 11;
		else
			turret.miniMapRenderObj.layer = 12;

		if (turret.playerController != null)
			turret.miniMapRenderObj.GetComponent<SpriteRenderer>().color = Color.green;
		else
			turret.miniMapRenderObj.GetComponent<SpriteRenderer>().color = Color.red;

		if (IsServer && playerCon.isPlayerOne)
			GameManager.Instance.playerBuildingsList.Add(GetComponent<BuildingManager>());
	}

	//update highlighter colour depending on bools
	public void UpdateHighlighterColour()
	{
		if (CheckCapturePointOwnership())
		{
			if (CheckPlacementHeight())
			{
				if (!isCollidingWithAnotherBuilding)
				{
					CanPlaceHighliterGreen();
				}
				else
					CanPlaceHighliterRed();
			}
			else
				CanPlaceHighliterRed();
		}
		else
			CanPlaceHighliterRed();
	}
	//final bool checks on player mouse click
	public bool CheckIfCanPlaceBuilding()
	{
		if (pointController == null)
		{
			AnnouncerSystem.Instance.PlayNegReplyInvalidBuildingLocationSFX();
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Not In Buildable Area", 2f);
			return canPlace = false;
		}
		if (!CheckCapturePointOwnership())
		{
			AnnouncerSystem.Instance.PlayNegReplyInvalidBuildingLocationSFX();
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("You Dont Own This Capturepoint", 2f);
			return canPlace = false;
		}
		if (!CheckPlacementHeight())
		{
			AnnouncerSystem.Instance.PlayNegReplyInvalidBuildingLocationSFX();
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Incorrect Placement Height", 2f);
			return canPlace = false;
		}
		if (isCollidingWithAnotherBuilding)
		{
			AnnouncerSystem.Instance.PlayNegReplyInvalidBuildingLocationSFX();
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Colliding with another Entity", 2f);
			return canPlace = false;
		}
		if (building != null)
		{
			if (pointController.energyGeneratorBuilding == null && building.isGeneratorBuilding)
			{
				return true;
			}
			else if (pointController.energyGeneratorBuilding != null && building.isGeneratorBuilding)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Energy Generator Already Exists", 1f);
				return false;
			}
			else if (pointController.RefinaryBuildings.Count < pointController.RefinaryBuildingsPlacementLimit && building.isRefineryBuilding)
			{
				return true;
			}
			else if (pointController.RefinaryBuildings.Count >= pointController.RefinaryBuildingsPlacementLimit && building.isRefineryBuilding)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max Refinery Buildings Reached in Control Point", 2f);
				return false;
			}
			else if (pointController.lightVehProdBuildings.Count < pointController.lightVehProdBuildingsPlacementLimit && building.isLightVehProdBuilding)
			{
				return true;
			}
			else if (pointController.lightVehProdBuildings.Count >= pointController.lightVehProdBuildingsPlacementLimit && building.isLightVehProdBuilding)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max Light Vehicle Buildings Reached in Control Point", 2f);
				return false;
			}
			else if (pointController.heavyVehProdBuildings.Count < pointController.heavyVehProdBuildingsPlacementLimit && building.isHeavyVehProdBuilding)
			{
				return true;
			}
			else if (pointController.heavyVehProdBuildings.Count >= pointController.heavyVehProdBuildingsPlacementLimit && building.isHeavyVehProdBuilding)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max Heavy Vehicle Buildings Reached in Control Point", 2f);
				return false;
			}
			else if (pointController.vtolProdBuildings.Count < pointController.vtolProdBuildingsPlacementLimit && building.isVTOLProdBuilding)
			{
				return true;
			}
			else if (pointController.vtolProdBuildings.Count >= pointController.vtolProdBuildingsPlacementLimit && building.isVTOLProdBuilding)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max VTOL Vehicle Buildings Reached in Control Point", 2f);
				return false;
			}
			else
			{
				Debug.LogError("Couldnt place building, this shouldnt happen");
				return false;
			}
		}
		else if (turret != null)
		{
			if (pointController.TurretDefenses.Count < pointController.TurretDefensesPlacementLimit && turret.isTurret)
			{
				return true;
			}
			else if (pointController.TurretDefenses.Count >= pointController.TurretDefensesPlacementLimit && turret.isTurret)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max Turret Defense Buildings Reached in Control Point", 2f);
				return false;
			}
			else
			{
				Debug.LogError("Couldnt place turret, this shouldnt happen");
				return false;
			}
		}
		else
		{
			Debug.LogError("building/turret reference not assigned or missing, this shouldnt happen");
			return false;
		}
	}

	//bool checks and updates
	public void OnTriggerEnter(Collider other)
	{
		if(other.GetComponent<CapturePointController>())
		{
			pointController = other.GetComponent<CapturePointController>();

			if (building != null)
				building.capturePointController = pointController;
			else if (turret != null)
				turret.capturePointController = pointController;
		}

		if (other.GetComponent<CanPlaceBuilding>() != null)
		{
			isCollidingWithAnotherBuilding = true;
		}
	}
	public void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<CapturePointController>())
		{
			pointController = null;
			CanPlaceHighliterRed();
		}

		if (other.GetComponent<CanPlaceBuilding>() != null)
		{
			isCollidingWithAnotherBuilding = false;
		}
	}
	public bool CheckCapturePointOwnership()
	{
		if (building != null)
		{
			if (!pointController.isNeutralPoint && !pointController.isPlayerOnePoint != building.isPlayerOneEntity)
				return true;
			else
				return false;
		}
		else
		{
			if (!pointController.isNeutralPoint && !pointController.isPlayerOnePoint != turret.isPlayerOneEntity)
				return true;
			else
				return false;
		}
	}
	public bool CheckPlacementHeight()
	{
		if (building != null)
		{
			if (building.transform.position.y > placementHeightMin && building.transform.position.y < placementHeightMax)
				return true;
			else
				return false;
		}
		else
		{
			if (turret.transform.position.y > placementHeightMin && turret.transform.position.y < placementHeightMax)
				return true;
			else
				return false;
		}
	}

	//change highlighter colour
	public void CanPlaceHighliterGreen()
	{
		highlighterObj.GetComponent<Renderer>().material.color = new Color(0, 1.0f, 0, 0.15f);
	}
	public void CanPlaceHighliterRed()
	{
		highlighterObj.GetComponent<Renderer>().material.color = new Color(1.0f, 0, 0, 0.15f);
	}
}
