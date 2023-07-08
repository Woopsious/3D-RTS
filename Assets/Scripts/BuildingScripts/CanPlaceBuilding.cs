using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CanPlaceBuilding : MonoBehaviour
{
	public CapturePointController pointController;

	public TurretController turret;
	public BuildingManager building;
	public GameObject highlighterObj;
	public NavMeshObstacle navMeshObstacle;
	public bool isPlayerOneBuilding;
	public bool isCollidingWithAnotherBuilding;
	public bool canPlace;
	public bool isPlaced;

	public void Start()
	{
		if (building != null)
		{
			if (!building.isHQ)
				highlighterObj.SetActive(true);
			CanPlaceHighliterRed();

			if (building.isPlayerOneEntity)
				building.miniMapRenderObj.layer = 11;

			else if (!building.isPlayerOneEntity)
				building.miniMapRenderObj.layer = 12;
		}
		else
		{
			CanPlaceHighliterRed();

			if (turret.isPlayerOneEntity)
				turret.miniMapRenderObj.layer = 11;

			else if (!turret.isPlayerOneEntity)
				turret.miniMapRenderObj.layer = 12;
		}
	}
	public void Update()
	{
		TrackPlacementHeight();
	}

	//track if colliding with another building or capture point and placement height
	public void OnTriggerEnter(Collider other)
	{
		if(other.GetComponent<CapturePointController>())
		{
			pointController = other.GetComponent<CapturePointController>();
			if (!CheckIfCapturePointIsNeutral())
				CanPlaceHighliterGreen();

			if (pointController.isNeutralPoint)
				CanPlaceHighliterRed();

			else
				CanPlaceHighliterRed();

			if (turret != null)
				turret.capturePointController = pointController;
			else if (building != null)
				building.capturePointController = pointController;
		}

		if (other.GetComponent<BuildingManager>())
		{
			CanPlaceHighliterRed();
			isCollidingWithAnotherBuilding = true;
		}
	}
	public void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<CapturePointController>())
		{
			CanPlaceHighliterRed();
			pointController = null;
		}

		if (other.GetComponent<BuildingManager>())
		{
			CanPlaceHighliterGreen();
			isCollidingWithAnotherBuilding = false;
		}
	}
	public void TrackPlacementHeight()
	{
		if (pointController != null && !CheckIfCapturePointIsNeutral())
		{
			if (building != null)
			{
				if (building.transform.position.y > 9f && building.transform.position.y < 10.5f)
					CanPlaceHighliterGreen();

				else
					CanPlaceHighliterRed();
			}
			else
			{
				if (turret.transform.position.y > 9f && turret.transform.position.y < 10.5f)
					CanPlaceHighliterGreen();

				else
					CanPlaceHighliterRed();
			}
		}
	}
	public bool CheckIfCapturePointIsNeutral()
	{
		if (building != null)
		{
			if (pointController.isPlayerOnePoint == building.isPlayerOneEntity && !pointController.isNeutralPoint ||
				pointController.isPlayerTwoPoint == !building.isPlayerOneEntity && !pointController.isNeutralPoint)
			{
				return false;
			}
			else
				return true;
		}
		else
		{
			if (pointController.isPlayerOnePoint == turret.isPlayerOneEntity && !pointController.isNeutralPoint ||
				pointController.isPlayerTwoPoint == !turret.isPlayerOneEntity && !pointController.isNeutralPoint)
			{
				return false;
			}
			else
				return true;
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

	//bool check on mouse click
	public bool CheckIfCanPlace()
	{
		if (pointController == null)
			return canPlace = false;

		if (building != null)
		{
			if (!pointController.isPlayerOnePoint != building.isPlayerOneEntity && pointController != null && !isCollidingWithAnotherBuilding &&
				building.transform.position.y > 9f && building.transform.position.y < 10.5f)
			{
				if (pointController.energyGeneratorBuilding == null && building.isGeneratorBuilding)
				{
					pointController.energyGeneratorBuilding = building;
					return canPlace = true;
				}
				else if (pointController.energyGeneratorBuilding != null && building.isGeneratorBuilding)
				{
					return canPlace = false;
				}
				else if (pointController.RefinaryBuildings.Count <= 1 && building.isRefineryBuilding)
				{
					pointController.RefinaryBuildings.Add(building);
					return canPlace = true;
				}
				else if (pointController.RefinaryBuildings.Count >= 1 && building.isRefineryBuilding)
				{
					return canPlace = false;
				}
				else if (pointController.lightVehProdBuildings.Count <= 1 && building.isLightVehProdBuilding)
				{
					pointController.lightVehProdBuildings.Add(building);
					building.playerController.lightVehProdBuildingsList.Add(building);
					return canPlace = true;
				}
				else if (pointController.lightVehProdBuildings.Count >= 1 && building.isLightVehProdBuilding)
				{
					return canPlace = false;
				}
				else if (pointController.heavyVehProdBuildings.Count <= 1 && building.isHeavyVehProdBuilding)
				{
					pointController.heavyVehProdBuildings.Add(building);
					building.playerController.heavyVehProdBuildingsList.Add(building);
					return canPlace = true;
				}
				else if (pointController.heavyVehProdBuildings.Count >= 1 && building.isHeavyVehProdBuilding)
				{
					return canPlace = false;
				}
				else if (pointController.vtolProdBuildings.Count <= 1 && building.isVTOLProdBuilding)
				{
					pointController.vtolProdBuildings.Add(building);
					building.playerController.vtolVehProdBuildingsList.Add(building);
					return canPlace = true;
				}
				else if (pointController.vtolProdBuildings.Count >= 1 && building.isVTOLProdBuilding)
				{
					return canPlace = false;
				}
				else
					return canPlace = false;
			}
			else return false;
		}
		else
		{
			if (!pointController.isPlayerOnePoint != turret.isPlayerOneEntity && pointController != null && !isCollidingWithAnotherBuilding &&
				turret.transform.position.y > 9f && turret.transform.position.y < 10.5f)
			{
				if (pointController.TurretDefenses.Count <= 1 && turret.isTurret)
				{
					pointController.TurretDefenses.Add(turret);
					turret.playerController.turretDefensesList.Add(turret);
					return canPlace = true;
				}
				else if (pointController.TurretDefenses.Count >= 1 && turret.isTurret)
				{
					return canPlace = false;
				}
				else
					return canPlace = false;
			}
			else return false;
		}
	}
}
