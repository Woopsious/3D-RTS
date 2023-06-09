using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class UnitSelectionManager : MonoBehaviour
{
	NavMeshQueryFilter filter = new NavMeshQueryFilter();

	Color transparentGreen;
	Color transparentRed;

	[Header("Game Ui + Refs")]
	public PlayerController playerController;
	public CanvasScaler canvasScaler;
	public RectTransform selectionBoxRect;
	public RectTransform selectionBoxUi;
	public Button refundSelectedUnitsButton;

	[Header("Dynamic Refs")]
	public Bounds bounds;
	private Vector2 startMousePos;
	private float dragDelay = 0.2f;
	private float mouseDownTime;

	public List<UnitStateController> selectedUnitList;
	public List<UnitStateController> dragSelectedUnitList;
	int unitCount = 0;

	public BuildingManager selectedBuilding;
	public CargoShipController SelectedCargoShip;

	[Header("Player Unit Groups")]
	public int maxUnitGroupSize;
	public List<UnitStateController> unitGroupOne;
	public List<UnitStateController> unitGroupTwo;
	public List<UnitStateController> unitGroupThree;
	public List<UnitStateController> unitGroupFour;
	public List<UnitStateController> unitGroupFive;
	public List<UnitStateController> unitListForPlayer;

	public List<Vector3> movePosOffset;
	public GameObject movePosHighlighterParentObj;
	public List<GameObject> movePosHighlighterObj;

	public void Start()
	{
		filter.areaMask = 1 << NavMesh.GetAreaFromName("Walkable");
		transparentGreen = new Color(0, 1, 0, 0.1f);
		transparentRed = new Color(1, 0, 0, 0.1f);
	}

	public void Update()
	{
		ShowUnitGhostProjections();
		//unit selection
		if (Input.GetMouseButtonDown(0))
		{
			startMousePos = Input.mousePosition;
			mouseDownTime += Time.deltaTime;
			TrySelectEntity();
		}
		//start drag select
		if (Input.GetMouseButton(0))
		{
			mouseDownTime += Time.deltaTime;
			if (mouseDownTime > dragDelay)
				UpdateSelectionBox(Input.mousePosition);
		}
		//end drag select
		if (Input.GetMouseButtonUp(0))
		{
			ReleaseSelectionBox();
			mouseDownTime = 0;
		}
		//clear selected list
		if (Input.GetMouseButtonDown(1) && selectedUnitList.Count != 0)
		{
			DeselectUnits();
			SetUnitRefundButtonActiveUnactive();
		}
		if (Input.GetMouseButtonDown(1) && selectedBuilding != null)
		{
			DeselectBuilding();
		}
		if (Input.GetMouseButtonDown(1) && SelectedCargoShip != null)
		{
			DeselectCargoShip();
		}
		//add selected list to group list
		if (Input.GetKey(KeyCode.LeftShift))
		{
			ManageSelectedUnitsAndGroups();
		}

		if (movePosHighlighterObj[0].activeInHierarchy)
		{
			for (int i = 0; i < movePosHighlighterObj.Count; i++)
			{
				if (movePosHighlighterObj[i].activeInHierarchy)
				{
					GameObject obj = movePosHighlighterObj[i].gameObject;
					Vector3 targetPos = new Vector3(obj.transform.position.x, obj.transform.position.y - 5, obj.transform.position.z);
					NavMesh.SamplePosition(obj.transform.position, out NavMeshHit hit, 2.5f, filter);

					if (Mathf.Approximately(obj.transform.position.x, hit.position.x) && Mathf.Approximately(obj.transform.position.z, hit.position.z))
					{
						if (obj.transform.position.y >= hit.position.y)
							obj.GetComponent<Renderer>().material.SetColor("_Color", transparentGreen);
					}
					else
						obj.GetComponent<Renderer>().material.SetColor("_Color", transparentRed);
				}
			}
		}
	}
	public void RefundSelectedUnits()
	{
		for (int i = selectedUnitList.Count- 1; i >= 0;i--)
		{
			selectedUnitList[i].RefundEntity();
		}
		foreach (GameObject obj in movePosHighlighterObj)
		{
			if (obj.activeInHierarchy)
				obj.SetActive(false);
		}
	}
	public void SetUnitRefundButtonActiveUnactive()
	{
		if (selectedUnitList.Count == 0 && refundSelectedUnitsButton.gameObject.activeInHierarchy)
			refundSelectedUnitsButton.gameObject.SetActive(false);

		else if (selectedUnitList.Count != 0 && !refundSelectedUnitsButton.gameObject.activeInHierarchy)
			refundSelectedUnitsButton.gameObject.SetActive(true);
	}

	//UNIT GHOST PROJECTION WHEN UNITS ARE HIGHLIGHTED
	public void ShowUnitGhostProjections()
	{
		if (selectedUnitList.Count != 0)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit hitInfo, 250f, playerController.ignoreMe))
			{
				movePosHighlighterParentObj.transform.position = hitInfo.point;
				//not working properly
				float mouseWheelRotation = Input.mouseScrollDelta.y;
				movePosHighlighterParentObj.transform.Rotate(10 * mouseWheelRotation * Vector3.up);

				if (selectedUnitList.Count < 9)
				{
					for (int i = 0; i < selectedUnitList.Count; i++)
					{
						if (!movePosHighlighterObj[i].activeSelf)
						{
							movePosHighlighterObj[i].SetActive(true);
						}
					}
				}
				else if (selectedUnitList.Count > 9)
				{
					for (int i = 0; i < 8; i++)
					{
						if (!movePosHighlighterObj[i].activeSelf)
						{
							movePosHighlighterObj[i].SetActive(true);
						}
					}
				}
			}
		}
	}

	//SELECTION AND DESELECTION FUNCTIONS
	public void TrySelectEntity()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out RaycastHit hitInfo, 250f, playerController.ignoreMe) && !playerController.IsMouseOverUI())
		{	
			//handle selecting of entities
			if (hitInfo.collider.gameObject.GetComponent<Entities>() != null)
			{
				Entities entity = hitInfo.collider.gameObject.GetComponent<Entities>();
				if (entity.GetComponent<UnitStateController>() != null && entity.isPlayerOneEntity != playerController.isPlayerOne)
					TryAttackEnemyEntity(entity);

				else if (entity.GetComponent<CargoShipController>() != null)
					TrySelectCargoShip(entity.GetComponent<CargoShipController>());

				else if (entity.GetComponent<BuildingManager>() != null)
					TrySelectBuilding(entity.GetComponent<BuildingManager>());

				else if (entity.GetComponent<UnitStateController>() != null)
					TrySelectUnits(entity.GetComponent<UnitStateController>());
			}
			//handle anything else
			else
			{
				if (SelectedCargoShip != null && hitInfo.collider.gameObject.GetComponent<ResourceNodes>() != null)
					TryMoveSelectedEntities(hitInfo.collider.gameObject);

				else if (selectedUnitList.Count != 0)
					TryMoveSelectedEntities(hitInfo.collider.gameObject);

				else
				{
					DeselectUnits();
					DeselectBuilding();
					DeselectCargoShip();
				}
			}
		}
	}
	public void TrySelectUnits(UnitStateController unit)
	{
		DeselectBuilding();
		DeselectCargoShip();

		if(unit.isPlayerOneEntity != !playerController.isPlayerOne)
		{
			//reselect new unit
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				DeselectUnits();
				unit.ShowUIHealthBar();
				unit.selectedHighlighter.SetActive(true);
				unit.isSelected = true;
				selectedUnitList.Add(unit);
			}
			//check if unit is already in selectedUnitList, if it was remove it, else add it
			if (Input.GetKey(KeyCode.LeftShift))
			{
				foreach (UnitStateController selectedUnit in selectedUnitList)
				{
					if (UnitAlreadyInList(selectedUnit, unit))
					{
						selectedUnit.HideUIHealthBar();
						selectedUnit.selectedHighlighter.SetActive(false);
						selectedUnit.isSelected = false;
						selectedUnitList.Remove(selectedUnit);
					}
					foreach (GameObject obj in movePosHighlighterObj)
					{
						if (obj.activeInHierarchy)
							obj.SetActive(false);
					}
				}
				unit.ShowUIHealthBar();
				unit.selectedHighlighter.SetActive(true);
				unit.isSelected = true;
				selectedUnitList.Add(unit);
			}
			SetUnitRefundButtonActiveUnactive();
		}
	}
	public void TrySelectBuilding(BuildingManager building)
	{
		if (BuildingExists(building) && building.isPlayerOneEntity != !playerController.isPlayerOne)
		{
			DeselectBuilding();
			DeselectUnits();
			DeselectCargoShip();
			selectedBuilding = building;
			selectedBuilding.ShowUIHealthBar();
			selectedBuilding.ShowRefundButton();
			selectedBuilding.selectedHighlighter.SetActive(true);
			selectedBuilding.isSelected = true;
		}
	}
	public void TrySelectCargoShip(CargoShipController cargoShip)
	{
		if (CheckForCargoShip(cargoShip) && cargoShip.isPlayerOneEntity != !playerController.isPlayerOne)
		{
			DeselectBuilding();
			DeselectUnits();
			DeselectCargoShip();
			SelectedCargoShip = cargoShip;
			SelectedCargoShip.selectedHighlighter.SetActive(true);
			SelectedCargoShip.isSelected = true;
		}
	}
	public void TryMoveSelectedEntities(GameObject Obj)
	{
		//move selected cargoShip
		if(SelectedCargoShip != null)
		{
			ResourceNodes resourceNode = Obj.GetComponent<ResourceNodes>();

			if (resourceNode.isBeingMined)
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Resource node already being mined!", 2f);

			else if (resourceNode.isEmpty)
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Resource node is empty!", 2f);

			else //else mine selected node
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Orders Recieved", 2f);
				SelectedCargoShip.SetResourceNodeFromPlayerInput(resourceNode);
			}
		}
		//move selected units to mouse pos
		else if (selectedUnitList.Count != 0)
		{
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Moving!", 1f);

			for (int i = 0; i < selectedUnitList.Count; i++)
			{
				UnitStateController unit = selectedUnitList[i];
				Vector3 movePos = movePosHighlighterObj[i].transform.position;
				unit.MoveToDestination(movePos);
			}
		}
	}
	public void TryAttackEnemyEntity(Entities entity)
	{
		//move selected units closer to target and attack it
		for (int i = 0; i < selectedUnitList.Count; i++)
		{
			UnitStateController unit = selectedUnitList[i];
			unit.TryAttackPlayerSetTarget(entity);
		}
	}

	//remove selected unit from selectedunitlist if it died whilst selected
	public void RemoveDeadUnitFromSelectedUnits(UnitStateController unit)
	{
		selectedUnitList.Remove(unit);
		movePosHighlighterObj[selectedUnitList.Count].SetActive(false);
	}

	//DRAG SELECT FUNCTIONS
	//drag selection logic to generate list of drag selected units, then see what player unit is inside bounds of drag select
	public void UpdateSelectionBox(Vector2 currentMousePos)
	{
		if (!selectionBoxRect.gameObject.activeInHierarchy)
		{
			selectionBoxRect.gameObject.SetActive(true);
			selectionBoxUi.gameObject.SetActive(true);
		}

		float width = currentMousePos.x - startMousePos.x;
		float height = currentMousePos.y - startMousePos.y;

		selectionBoxRect.anchoredPosition = startMousePos + new Vector2(width / 2, height / 2);
		selectionBoxRect.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
		selectionBoxUi.anchoredPosition = (startMousePos + new Vector2(width < 0 ? width : 0f, height < 0 ? height : 0f));
		selectionBoxUi.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

		bounds = new Bounds(selectionBoxRect.anchoredPosition, selectionBoxRect.sizeDelta);
		foreach (UnitStateController unit in playerController.unitListForPlayer)
		{
			if (UnitInSelectionBox(Camera.main.WorldToScreenPoint(unit.transform.position), bounds) && !unit.selectedHighlighter.activeSelf && 
				unitCount < 8 && !unit.isPlayerOneEntity != playerController.isPlayerOne)
			{
				unit.ShowUIHealthBar();
				unit.selectedHighlighter.SetActive(true);
				unit.isSelected = true;
				dragSelectedUnitList.Add(unit);
				unitCount++;
			}
			else if (!UnitInSelectionBox(Camera.main.WorldToScreenPoint(unit.transform.position), bounds) && unit.selectedHighlighter.activeSelf)
			{
				unit.HideUIHealthBar();
				unit.selectedHighlighter.SetActive(false);
				unit.isSelected = false;
				dragSelectedUnitList.Remove(unit);
				unitCount--;
			}
		}
	}
	//add drag selected units to selected units list and hid drag select box
	public void ReleaseSelectionBox()
	{
		if (dragSelectedUnitList.Count != 0)
		{
			DeselectUnits();
			DeselectBuilding();
			DeselectCargoShip();
		}
		foreach (UnitStateController unit in dragSelectedUnitList)
		{
			if(unit.isPlayerOneEntity != !playerController.isPlayerOne)
				selectedUnitList.Add(unit);
		}
		dragSelectedUnitList.Clear();
		selectionBoxRect.gameObject.SetActive(false);
		selectionBoxUi.gameObject.SetActive(false);
		unitCount = 0;
		SetUnitRefundButtonActiveUnactive();
	}

	//DESELECT ENTITY FUNCTIONS
	public void DeselectUnits()
	{
		if (selectedUnitList.Count != 0)
		{
			SetUnitRefundButtonActiveUnactive();
			foreach (UnitStateController selectedUnit in selectedUnitList)
			{
				selectedUnit.HideUIHealthBar();
				selectedUnit.selectedHighlighter.SetActive(false);
				selectedUnit.isSelected = false;
			}

			foreach (GameObject obj in movePosHighlighterObj)
			{
				if (obj.activeInHierarchy)
					obj.SetActive(false);
			}
			selectedUnitList.Clear();
		}
	}
	public void DeselectBuilding()
	{
		if (selectedBuilding != null)
		{
			if(!selectedBuilding.wasRecentlyHit)
				selectedBuilding.HideUIHealthBar();
			selectedBuilding.HideRefundButton();
			selectedBuilding.selectedHighlighter.SetActive(false);
			selectedBuilding.isSelected = false;
			selectedBuilding = null;
		}
	}
	public void DeselectCargoShip()
	{
		if (SelectedCargoShip != null)
		{
			SelectedCargoShip.selectedHighlighter.SetActive(false);
			SelectedCargoShip.isSelected = false;
			SelectedCargoShip = null;
		}
	}

	//UNIT GROUP SAVING AND SELECTING FUNCTIONS
	public void ManageSelectedUnitsAndGroups()
	{
		if (selectedUnitList.Count != 0)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				AssignUnitsToGroupOne();
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				AssignUnitsToGroupTwo();
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				AssignUnitsToGroupThree();
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				AssignUnitsToGroupFour();
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				AssignUnitsToGroupFive();
			}
		}
		else if (selectedUnitList.Count == 0)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				SelectUnitsFromGroup(unitGroupOne);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				SelectUnitsFromGroup(unitGroupTwo);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				SelectUnitsFromGroup(unitGroupThree);
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				SelectUnitsFromGroup(unitGroupFour);
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				SelectUnitsFromGroup(unitGroupFive);
			}
		}
	}
	//reset ui list count, recount units in group to update ui
	public void AssignUnitsToGroupOne()
	{
		playerController.gameUIManager.ResetUnitGroupUI();
		CheckForUnitInList(unitGroupOne, 1);
		CheckForUnitInList(unitGroupTwo, 2);
		CheckForUnitInList(unitGroupThree, 3);
		CheckForUnitInList(unitGroupFour, 4);
		CheckForUnitInList(unitGroupFive, 5);
		AddSelectedUnitsToNewGroup(unitGroupOne, 1);
		playerController.gameUIManager.ShowGroupedUnitsWhenCreatingGroup();
	}
	public void AssignUnitsToGroupTwo()
	{
		playerController.gameUIManager.ResetUnitGroupUI();
		CheckForUnitInList(unitGroupOne, 1);
		CheckForUnitInList(unitGroupTwo, 2);
		CheckForUnitInList(unitGroupThree, 3);
		CheckForUnitInList(unitGroupFour, 4);
		CheckForUnitInList(unitGroupFive, 5);
		AddSelectedUnitsToNewGroup(unitGroupTwo, 2);
		playerController.gameUIManager.ShowGroupedUnitsWhenCreatingGroup();
	}
	public void AssignUnitsToGroupThree()
	{
		playerController.gameUIManager.ResetUnitGroupUI();
		CheckForUnitInList(unitGroupOne, 1);
		CheckForUnitInList(unitGroupTwo, 2);
		CheckForUnitInList(unitGroupThree, 3);
		CheckForUnitInList(unitGroupFour, 4);
		CheckForUnitInList(unitGroupFive, 5);
		AddSelectedUnitsToNewGroup(unitGroupThree, 3);
		playerController.gameUIManager.ShowGroupedUnitsWhenCreatingGroup();
	}
	public void AssignUnitsToGroupFour()
	{
		playerController.gameUIManager.ResetUnitGroupUI();
		CheckForUnitInList(unitGroupOne, 1);
		CheckForUnitInList(unitGroupTwo, 2);
		CheckForUnitInList(unitGroupThree, 3);
		CheckForUnitInList(unitGroupFour, 4);
		CheckForUnitInList(unitGroupFive, 5);
		AddSelectedUnitsToNewGroup(unitGroupFour, 4);
		playerController.gameUIManager.ShowGroupedUnitsWhenCreatingGroup();
	}
	public void AssignUnitsToGroupFive()
	{
		playerController.gameUIManager.ResetUnitGroupUI();
		CheckForUnitInList(unitGroupOne, 1);
		CheckForUnitInList(unitGroupTwo, 2);
		CheckForUnitInList(unitGroupThree, 3);
		CheckForUnitInList(unitGroupFour, 4);
		CheckForUnitInList(unitGroupFive, 5);
		AddSelectedUnitsToNewGroup(unitGroupFive, 5);
		playerController.gameUIManager.ShowGroupedUnitsWhenCreatingGroup();
	}
	//checks group for unit, if unit in group remove unit from group, then add unit to new group, update ui for changes 
	public void CheckForUnitInList(List<UnitStateController> unitGroup, int groupToUpdate)
	{
		foreach (UnitStateController selectedUnit in selectedUnitList)
		{
			for (int i = 0; i < unitGroup.Count;)
			{
				if (UnitAlreadyInList(selectedUnit, unitGroup[i]))
				{
					unitGroup[i].selectedHighlighter.SetActive(false);
					unitGroup[i].isSelected = false;
					unitGroup.RemoveAt(i);
					continue;
				}
				if (!UnitAlreadyInList(selectedUnit, unitGroup[i]))
				{
					i++;
					continue;
				}
			}
		}
		playerController.gameUIManager.UpdateUnitGroupUi(unitGroup, groupToUpdate);
	}
	public void AddSelectedUnitsToNewGroup(List<UnitStateController> unitGroup, int groupToUpdate)
	{
		if (selectedUnitList.Count < maxUnitGroupSize)
		{
			for (int i = 0; i < selectedUnitList.Count; i++)
			{
				selectedUnitList[i].selectedHighlighter.SetActive(true);
				selectedUnitList[i].isSelected = true;
				unitGroup.Add(selectedUnitList[i]);
				Debug.Log("unit added: " + i);
			}
		}
		else if (selectedUnitList.Count >= maxUnitGroupSize)
		{
			for (int i = 0; i < 8; i++)
			{
				selectedUnitList[i].selectedHighlighter.SetActive(true);
				selectedUnitList[i].isSelected = true;
				unitGroup.Add(selectedUnitList[i]);
				Debug.Log("unit added: " + i);
			}
		}
		foreach (UnitStateController unit in unitGroup)
		{
			unit.GroupNum = groupToUpdate;
		}
		playerController.gameUIManager.UpdateUnitGroupUi(unitGroup, groupToUpdate);
	}
	//if no units selected then select all units in group
	public void SelectUnitsFromGroup(List<UnitStateController> unitGroup)
	{
		foreach (UnitStateController unit in unitGroup)
		{
			unit.selectedHighlighter.SetActive(true);
			unit.isSelected = true;
			selectedUnitList.Add(unit);
		}
		SetUnitRefundButtonActiveUnactive();
	}

	//BOOL CHECKS
	public bool BuildingExists(BuildingManager building)
	{
		if (building != null)
			return true;
		else return false;
	}
	public bool UnitExists(UnitStateController unit)
	{
		if (unit != null)
			return true;
		else return false;
	}
	public bool CheckForCargoShip(CargoShipController cargoShip)
	{
		if (cargoShip != null)
			return true;
		else return false;
	}
	public bool UnitAlreadyInList(UnitStateController selectedUnit, UnitStateController unit)
	{
		if (selectedUnit.gameObject.GetInstanceID() == unit.gameObject.GetInstanceID())
			return true;
		else return false;
	}
	public bool UnitInSelectionBox(Vector2 position, Bounds bounds)
	{
		return position.x > bounds.min.x && position.x < bounds.max.x
			&& position.y > bounds.min.y && position.y < bounds.max.y;
	}
}
