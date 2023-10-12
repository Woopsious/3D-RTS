using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CapturePointController : MonoBehaviour
{
	[Header("CapturePointRefs")]
	public GameObject miniMapIndicatorMaterial;
	public GameObject flagMaterial;

	public int trackLastCapturePointOwnership; //0 = netural, 1 = P1, 2 = P2

	[Header("Bool Refs")]
	public bool isNeutralPoint;
	public bool isPlayerOnePoint;
	public bool isPlayerTwoPoint;

	[Header("Max Building Type Limit")]
	public readonly int energyGeneratorPlacementLimit = 1;
	public readonly int RefinaryBuildingsPlacementLimit = 2;
	public readonly int lightVehProdBuildingsPlacementLimit = 2;
	public readonly int heavyVehProdBuildingsPlacementLimit = 2;
	public readonly int vtolProdBuildingsPlacementLimit = 2;
	public readonly int TurretDefensesPlacementLimit = 3;

	[Header("Optional Refs")]
	public BuildingManager HQRef;

	[Header("Dynamic Refs")] 
	public BuildingManager energyGeneratorBuilding;

	public List<BuildingManager> RefinaryBuildings;
	public List<BuildingManager> lightVehProdBuildings;
	public List<BuildingManager> heavyVehProdBuildings;
	public List<BuildingManager> vtolProdBuildings;
	public List<TurretController> TurretDefenses;

	public List<UnitStateController> playerOneUnitList;
	public List<UnitStateController> playerTwoUnitList;

	public GameObject ResourceNodeContainerObj;
	public List<ResourceNodes> resourceNodes = new List<ResourceNodes>();

	public float timer;

	public void Start()
	{
		if (HQRef != null)
		{
			if (HQRef.isPlayerOneEntity)
			{
				isNeutralPoint = false;
				isPlayerOnePoint = true;
				isPlayerTwoPoint = false;

				UpdateFlagColour(1);
				trackLastCapturePointOwnership = 1;
			}
			else if (!HQRef.isPlayerOneEntity)
			{
				isNeutralPoint = false;
				isPlayerOnePoint = false;
				isPlayerTwoPoint = true;

				UpdateFlagColour(2);
				trackLastCapturePointOwnership = 2;
			}
		}
		SetUpResourceNodes();
	}
	public void Update()
	{
		timer -= Time.deltaTime;
		if (timer < 0)
		{
			timer = 2.5f;
			TrackPointOwnership();
		}
	}
	//check if buildings exist, check list of player units to fip point ownership 
	public void TrackPointOwnership()
	{
		// check for buildings
		if (HQRef == null && energyGeneratorBuilding == null && RefinaryBuildings.Count == 0 && lightVehProdBuildings.Count == 0 && 
			heavyVehProdBuildings.Count == 0 && vtolProdBuildings.Count == 0)
		{
			//check for units in building area
			if (playerOneUnitList.Count == 0 && playerTwoUnitList.Count == 0)
			{
				isNeutralPoint = true;
				isPlayerOnePoint = false;
				isPlayerTwoPoint = false;

				UpdateFlagColour(0);
				NotifyPlayersOfOwnershipChanges(0);

				trackLastCapturePointOwnership = 0;
			}
			if (playerOneUnitList.Count != 0 && playerTwoUnitList.Count == 0)
			{
				isNeutralPoint = false;
				isPlayerOnePoint = true;
				isPlayerTwoPoint = false;

				UpdateFlagColour(1);
				NotifyPlayersOfOwnershipChanges(1);

				trackLastCapturePointOwnership = 1;
			}
			else if (playerOneUnitList.Count == 0 && playerTwoUnitList.Count != 0)
			{
				isNeutralPoint = false;
				isPlayerOnePoint = false;
				isPlayerTwoPoint = true;

				UpdateFlagColour(2);
				NotifyPlayersOfOwnershipChanges(2);

				trackLastCapturePointOwnership = 2;
			}
			ChangeOwnershipOfResourceNodes();
		}
	}
	public void UpdateFlagColour(int newFlagOwnership)
	{
		if (newFlagOwnership == 0 && trackLastCapturePointOwnership != 0)
		{
			flagMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
			miniMapIndicatorMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
		}

		else if (newFlagOwnership == 1)
		{
			if (GameManager.Instance.isPlayerOne)
			{
				flagMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
				miniMapIndicatorMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
			}
			else if (!GameManager.Instance.isPlayerOne)
			{
				flagMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
				miniMapIndicatorMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
			}
		}
		else if (newFlagOwnership == 2)
		{
			if (!GameManager.Instance.isPlayerOne)
			{
				flagMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
				miniMapIndicatorMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
			}
			else if (GameManager.Instance.isPlayerOne)
			{
				flagMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
				miniMapIndicatorMaterial.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
			}
		}
	}
	public void NotifyPlayersOfOwnershipChanges(int newPointOwnership)
	{
		if (trackLastCapturePointOwnership == 0 && newPointOwnership == 2)
		{
			if (GameManager.Instance.isPlayerOne)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Neutral Point Gained By Another Player", 3f);
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("Neutral Point Gained", gameObject.transform.position);
			}
			else if (!GameManager.Instance.isPlayerOne)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Neutral Point Gained", 3f);
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("Neutral Point Gained", gameObject.transform.position);
			}
		}
		if (trackLastCapturePointOwnership == 0 && newPointOwnership == 1)
		{
			if (GameManager.Instance.isPlayerOne)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Neutral Point Gained", 3f);
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("Neutral Point Gained", gameObject.transform.position);
			}
			else if (!GameManager.Instance.isPlayerOne)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Neutral Point Gained By Another Player", 3f);
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("Neutral Point Gained", gameObject.transform.position);
			}
		}

		if (trackLastCapturePointOwnership == 1 && newPointOwnership == 2)
		{
			if (GameManager.Instance.isPlayerOne)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Capture Point Lost", 3f);
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("Capture Point Lost", gameObject.transform.position);
			}
			else if (!GameManager.Instance.isPlayerOne)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Capture Point Gained", 3f);
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("Capture Point Gained", gameObject.transform.position);
			}
		}

		else if (trackLastCapturePointOwnership == 2 && newPointOwnership == 1)
		{
			if (GameManager.Instance.isPlayerOne)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Capture Point Gained", 3f);
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("Capture Point Gained", gameObject.transform.position);
			}
			else if (!GameManager.Instance.isPlayerOne)
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Capture Point Lost", 3f);
				GameManager.Instance.playerNotifsManager.DisplayEventMessage("Capture Point Lost", gameObject.transform.position);
			}
		}
	}

	//track units in area
	public void OnTriggerEnter(Collider other)
	{
		GrabUnitRefs(other);
	}
	public void OnTriggerExit(Collider other)
	{
		RemoveUnitRefs(other);
	}
	public void GrabUnitRefs(Collider other)
	{
		if (other.gameObject.GetComponent<UnitStateController>())
		{
			UnitStateController unit = other.GetComponent<UnitStateController>();
			unit.capturePoint = this;

			if (!unit.isTurret && !unit.isCargoShip && unit.isPlayerOneEntity)
				playerOneUnitList.Add(unit);
			else if (!unit.isTurret && !unit.isCargoShip && !unit.isPlayerOneEntity)
				playerTwoUnitList.Add(unit);
		}
	}
	public void RemoveUnitRefs(Collider other)
	{
		if (other.gameObject.GetComponent<UnitStateController>())
		{
			UnitStateController unit = other.GetComponent<UnitStateController>();
			unit.capturePoint = null;

			if (!unit.isTurret && !unit.isCargoShip && unit.isPlayerOneEntity)
				playerOneUnitList.Remove(unit);
			else if (!unit.isTurret && !unit.isCargoShip && !unit.isPlayerOneEntity)
				playerTwoUnitList.Remove(unit);
		}
	}
	public void RemoveUnitRefsOnUnitDeath(UnitStateController unit)
	{
		if (playerOneUnitList.Contains(unit))
			playerOneUnitList.Remove(unit);
		else if (playerTwoUnitList.Contains(unit))
			playerTwoUnitList.Remove(unit);
	}

	//Manager Capturepont ResourceNodes
	public void SetUpResourceNodes()
	{
		foreach (Transform resourceNodeObj in ResourceNodeContainerObj.transform)
			resourceNodes.Add(resourceNodeObj.GetComponent<ResourceNodes>());

		ChangeOwnershipOfResourceNodes();
	}
	public void ChangeOwnershipOfResourceNodes()
	{
		foreach (ResourceNodes resourceNode in resourceNodes)
		{
			if (isNeutralPoint)
			{
				resourceNode.canPOneMine = false;
				resourceNode.canPTwoMine = false;
			}
			else if (isPlayerOnePoint)
			{
				resourceNode.canPOneMine = true;
				resourceNode.canPTwoMine = false;
			}
			else if (isPlayerTwoPoint)
			{
				resourceNode.canPOneMine = false;
				resourceNode.canPTwoMine = true;
			}
		}
	}
}