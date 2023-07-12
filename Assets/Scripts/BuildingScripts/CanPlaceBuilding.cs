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
			highlighterObj.SetActive(true);
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

	//track if colliding with another building or capture point and placement height CAN PLACE HIGHLIGHTER DOESNT ALWAYS WORK
	//functions are called correctly as a console log is produced and can place bool gets ticked/unticked in inspector
	//CanPlaceHighliterRed/Green also works as it changes colour when building y axis is too high or low or when not colliding with capture point
	public void OnTriggerEnter(Collider other)
	{
		if(other.GetComponent<CapturePointController>())
		{
			Debug.Log("capturepoint trigger enter");
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

		if (other.GetComponent<CanPlaceBuilding>() != null)
		{
			Debug.Log("building trigger enter");
			CanPlaceHighliterRed();
			isCollidingWithAnotherBuilding = true;
		}
		else
		{
			Debug.Log("trigger enter");
		}
	}
	public void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<CapturePointController>())
		{
			Debug.Log("capturepoint trigger exit");
			CanPlaceHighliterRed();
			pointController = null;
		}

		if (other.GetComponent<CanPlaceBuilding>() != null)
		{
			Debug.Log("building trigger exit");
			CanPlaceHighliterGreen();
			isCollidingWithAnotherBuilding = false;
		}
		else
		{
			Debug.Log("trigger exit");
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
		{
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Not In Buildable Area", 1f);
			return canPlace = false;
		}

		if (building != null)
		{
			if (!pointController.isPlayerOnePoint != building.isPlayerOneEntity && pointController != null && !isCollidingWithAnotherBuilding &&
				building.transform.position.y > 9f && building.transform.position.y < 10.5f)
			{
				if (pointController.energyGeneratorBuilding == null && building.isGeneratorBuilding)
				{
					return canPlace = true;
				}
				else if (pointController.energyGeneratorBuilding != null && building.isGeneratorBuilding)
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Energy Generator Already Exists", 1f);
					return canPlace = false;
				}
				else if (pointController.RefinaryBuildings.Count <= 1 && building.isRefineryBuilding)
				{
					return canPlace = true;
				}
				else if (pointController.RefinaryBuildings.Count >= 1 && building.isRefineryBuilding)
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max Refinery Buildings Reached in Control Point", 2f);
					return canPlace = false;
				}
				else if (pointController.lightVehProdBuildings.Count <= 1 && building.isLightVehProdBuilding)
				{
					return canPlace = true;
				}
				else if (pointController.lightVehProdBuildings.Count >= 1 && building.isLightVehProdBuilding)
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max Light Vehicle Buildings Reached in Control Point", 2f);
					return canPlace = false;
				}
				else if (pointController.heavyVehProdBuildings.Count <= 1 && building.isHeavyVehProdBuilding)
				{
					return canPlace = true;
				}
				else if (pointController.heavyVehProdBuildings.Count >= 1 && building.isHeavyVehProdBuilding)
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max Heavy Vehicle Buildings Reached in Control Point", 2f);
					return canPlace = false;
				}
				else if (pointController.vtolProdBuildings.Count <= 1 && building.isVTOLProdBuilding)
				{
					return canPlace = true;
				}
				else if (pointController.vtolProdBuildings.Count >= 1 && building.isVTOLProdBuilding)
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max VTOL Vehicle Buildings Reached in Control Point", 2f);
					return canPlace = false;
				}
				else
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Couldnt place building", 1f);
					return canPlace = false;
				}
			}
			else 
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Couldnt place building", 1f);
				return false;
			}
		}
		else
		{
			if (!pointController.isPlayerOnePoint != turret.isPlayerOneEntity && pointController != null && !isCollidingWithAnotherBuilding &&
				turret.transform.position.y > 9f && turret.transform.position.y < 10.5f)
			{
				if (pointController.TurretDefenses.Count <= 1 && turret.isTurret)
				{
					return canPlace = true;
				}
				else if (pointController.TurretDefenses.Count >= 1 && turret.isTurret)
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Max Turret Defense Buildings Reached in Control Point", 2f);
					return canPlace = false;
				}
				else
				{
					GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Couldnt place building", 1f);
					return canPlace = false;
				}
			}
			else
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Couldnt place building", 1f);
				return false;
			}
		}
	}
}
