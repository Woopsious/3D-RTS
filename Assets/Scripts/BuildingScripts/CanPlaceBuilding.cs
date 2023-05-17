using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CanPlaceBuilding : MonoBehaviour
{
	public CapturePointController pointController;

	public BuildingManager building;
	public GameObject highlighterObj;
	public NavMeshObstacle navMeshObstacle;
	public bool isPlayerOneBuilding;
	public bool isCollidingWithAnotherBuilding;
	public bool canPlace;
	public bool isPlaced;

	public void Start()
	{
		if (gameObject.layer == 2)
		{
			highlighterObj.SetActive(true);
			highlighterObj.GetComponent<Renderer>().material.color = new Color(1.0f, 0, 0, 0.15f);
		}
		if (building.isPlayerOneBuilding)
			building.miniMapRenderObj.layer = 11;

		else if (!building.isPlayerOneBuilding)
			building.miniMapRenderObj.layer = 12;
	}

	//track if colliding with another building or capture point
	public void OnTriggerEnter(Collider other)
	{
		if(other.GetComponent<CapturePointController>())
		{
			pointController = other.GetComponent<CapturePointController>();
			if (pointController.isPlayerOnePoint == building.isPlayerOneBuilding || pointController.isPlayerTwoPoint == !building.isPlayerOneBuilding)
				highlighterObj.GetComponent<Renderer>().material.color = new Color(0, 1.0f, 0, 0.15f);
			else
				highlighterObj.GetComponent<Renderer>().material.color = new Color(1.0f, 0, 0, 0.15f);

			building.capturePointController = pointController;
		}

		if (other.GetComponent<BuildingManager>())
		{
			highlighterObj.GetComponent<Renderer>().material.color = new Color(1.0f, 0, 0, 0.15f);
			isCollidingWithAnotherBuilding = true;
		}
	}
	public void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<CapturePointController>())
		{
			highlighterObj.GetComponent<Renderer>().material.color = new Color(1.0f, 0, 0, 0.15f);
			pointController = null;
		}

		if (other.GetComponent<BuildingManager>())
		{
			highlighterObj.GetComponent<Renderer>().material.color = new Color(0, 1.0f, 0, 0.15f);
			isCollidingWithAnotherBuilding = false;
		}
	}
	//bool check on mouse click
	public bool CheckIfCanPlace()
	{
		if (pointController == null)
			return canPlace = false;

		if (!pointController.isPlayerOnePoint != building.isPlayerOneBuilding && pointController != null && !isCollidingWithAnotherBuilding)
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
		else
			return canPlace = false;
	}
}
