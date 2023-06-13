using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UnitStateController : MonoBehaviour
{
	public LayerMask ignoreMe;
	public UnitBaseState currentState;
	public UnitIdleState idleState = new UnitIdleState();
	public UnitMovingState movingState = new UnitMovingState();
	public UnitStateAttacking attackState = new UnitStateAttacking();

	public WeaponSystem weaponSystem;

	[Header("Unit Refs")]
	public Animator animatorController;
	public List<AudioSource> audioSFXs = new List<AudioSource>();
	public AudioSource movingSFX;
	public NavMeshAgent agentNav;
	public Rigidbody rb;
	public GameObject CenterPoint;
	public GameObject unitDeathObj;
	public GameObject selectedHighlighter;
	public GameObject miniMapRenderObj;
	public GameObject FoVMeshObj;

	public GameObject unitUiObj;
	public UnityEngine.UI.Slider HealthSlider;
	public Text HealthText;

	public bool isPlayerOneUnit;
	public bool isSelected;
	public bool isUnitArmed;
	public bool isFlying;
	public bool hasAnimation;
	public bool hasRadar;
	public bool isCargoShip;
	public bool isSpotted;

	[Header("Unit Stat Refs")]
	public string unitName;
	public int moneyCost; 
	public int alloyCost; 
	public int crystalCost;
	public int maxHealth;
	public int currentHealth;
	public int armour;
	public float attackRange;
	public float ViewRange;

	[Header("Unit Dynamic Refs")]
	public int GroupNum;
	public PlayerController playerController;

	public List<GameObject> targetList;
	public List<BuildingManager> buildingTargetList;
	public BuildingManager currentBuildingTarget;
	public List<UnitStateController> unitTargetList;
	public UnitStateController currentUnitTarget;
	public Vector3 targetPos;
	public Vector3 movePos;
	public NavMeshPath navMeshPath;

	public virtual void Start()
	{
		ChangeStateIdle();
		UpdateAudioVolume();
		//assign correct playercontroller to unit on start
		PlayerController[] controllers = FindObjectsOfType<PlayerController>();
		foreach(PlayerController controller in controllers)
		{
			if(controller.isPlayerOne == isPlayerOneUnit)
			{
				playerController = controller;
				playerController.unitListForPlayer.Add(this);
			}
			else if (controller.isPlayerOne == !isPlayerOneUnit)
			{
				playerController = controller;
				playerController.unitListForPlayer.Add(this);
			}
		}
		unitUiObj.transform.SetParent(FindObjectOfType<GameUIManager>().gameObject.transform);
		unitUiObj.SetActive(false);
		UpdateHealthBar();
		unitUiObj.transform.rotation = Quaternion.identity;

		if (isPlayerOneUnit)
			miniMapRenderObj.layer = 11;

		else if (!isPlayerOneUnit)
			miniMapRenderObj.layer = 12;
		//FoVMeshObj.SetActive(true);
	}
	public virtual void Update()
	{
		if (isSelected)
		{
			if (!unitUiObj.activeInHierarchy)
				unitUiObj.SetActive(true);
			unitUiObj.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position + new Vector3(0, 5, 0));
		}
		if (!isSelected && unitUiObj.activeInHierarchy)
			unitUiObj.SetActive(false);

		currentState.UpdateLogic(this);
	}
	public virtual void FixedUpdate()
	{
		currentState.UpdatePhysics(this);

		if (targetList.Count != 0 && isUnitArmed && !isCargoShip && currentState != attackState) //switch to attack state if targets found
			ChangeStateAttacking();

		else if (targetList.Count == 0 && currentState == attackState)
			ChangeStateIdle();
	}
	//filter out everything but enemy Entities
	public void AddTargetsOnFOVEnter(GameObject triggerObj)
	{
		if (triggerObj.GetComponent<UnitStateController>() != null && isPlayerOneUnit != triggerObj.GetComponent<UnitStateController>().isPlayerOneUnit)
		{
			if (!unitTargetList.Contains(triggerObj.GetComponent<UnitStateController>()))
				targetList.Add(triggerObj);
		}
		else if (triggerObj.GetComponent<BuildingManager>() != null && isPlayerOneUnit != triggerObj.GetComponent<BuildingManager>().isPlayerOneBuilding
			&& triggerObj.GetComponent<CanPlaceBuilding>().isPlaced)    //filter out non placed buildings
		{
			if (!buildingTargetList.Contains(triggerObj.GetComponent<BuildingManager>()))
				targetList.Add(triggerObj);
		}
	}
	public void RemoveTargetsOnFOVExit(GameObject triggerObj)
	{
		if (targetList.Contains(triggerObj))
		{
			targetList.Remove(triggerObj);
			triggerObj.GetComponent<UnitStateController>().HideUnit();
		}

		else if (targetList.Contains(triggerObj))
		{
			targetList.Remove(triggerObj);
			triggerObj.GetComponent<BuildingManager>().HideBuilding();
		}

		if (unitTargetList.Contains(triggerObj.GetComponent<UnitStateController>()))
			unitTargetList.Remove(triggerObj.GetComponent<UnitStateController>());

		if (buildingTargetList.Contains(triggerObj.GetComponent<BuildingManager>()))
			buildingTargetList.Remove(triggerObj.GetComponent<BuildingManager>());
	}

	//HEALTH FUNCTIONS
	public void RecieveDamage(int dmg)
	{
		dmg -= armour;
		if (dmg < 0)
			dmg = 0;
		currentHealth -= dmg;
		UpdateHealthBar();
		OnDeath();

		//notify player when unit is under attack
	}
	public void UpdateHealthBar()
	{
		float healthPercentage = (float)currentHealth / (float)maxHealth * 100;
		HealthSlider.value = healthPercentage;
		HealthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
	}
	public void OnDeath()
	{
		if(currentHealth <= 0)
		{
			RemoveRefs();

			Instantiate(unitDeathObj, transform.position, Quaternion.identity);

			Destroy(unitUiObj);
			Destroy(gameObject);
		}
	}

	//UTILITY FUNCTIONS
	public void RemoveNullRefsFromLists(List<GameObject> targetList, List<UnitStateController> unitList, List<BuildingManager> buildingList)
	{
		for (int i = targetList.Count - 1; i >= 0; i--)
		{
			if (targetList[i] == null)
				targetList.RemoveAt(i);
		}
		for (int i = unitList.Count - 1; i >= 0; i--)
		{
			if (unitList[i] == null)
				unitList.RemoveAt(i);
		}
		for (int i = buildingList.Count - 1; i >= 0; i--)
		{
			if (buildingList[i] == null)
				buildingList.RemoveAt(i);
		}
	}
	public IEnumerator DelaySecondaryAttack(UnitStateController unit, float seconds)
	{
		unit.weaponSystem.secondaryWeaponAttackSpeedTimer++;
		unit.weaponSystem.secondaryWeaponAttackSpeedTimer %= unit.weaponSystem.secondaryWeaponAttackSpeed - 1;
		yield return new WaitForSeconds(seconds);
		unit.weaponSystem.ShootSecondaryWeapon();
	}
	public void RefundUnit()
	{
		currentHealth = 0;
		int refundMoney = (int)(moneyCost / 1.5);
		int refundAlloy = (int)(alloyCost / 1.5);
		int refundCrystal = (int)(crystalCost / 1.5);

		if (isPlayerOneUnit)
		{
			GameManager.Instance.playerOneCurrentMoney += refundMoney;
			GameManager.Instance.playerOneCurrentAlloys += refundAlloy;
			GameManager.Instance.playerOneCurrentCrystals += refundCrystal;
		}
		else if (!isPlayerOneUnit)
		{
			GameManager.Instance.aiCurrentMoney += refundMoney;
			GameManager.Instance.aiCurrentAlloys += refundAlloy;
			GameManager.Instance.aiCurrentCrystals += refundCrystal;
		}
		playerController.gameUIManager.UpdateCurrentResourcesUI();
		OnDeath();
	}
	public void UpdateAudioVolume()
	{
		foreach (AudioSource audio in audioSFXs)
			audio.volume = AudioManager.Instance.gameSFX.volume;
	}
	public virtual void RemoveRefs()
	{
		if (GroupNum == 1)
		{
			playerController.unitSelectionManager.unitGroupOne.Remove(this);
			playerController.gameUIManager.UpdateGroupUi(playerController.unitSelectionManager.unitGroupOne, 1);
		}
		if (GroupNum == 2)
		{
			playerController.unitSelectionManager.unitGroupTwo.Remove(this);
			playerController.gameUIManager.UpdateGroupUi(playerController.unitSelectionManager.unitGroupTwo, 2);
		}
		if (GroupNum == 3)
		{
			playerController.unitSelectionManager.unitGroupThree.Remove(this);
			playerController.gameUIManager.UpdateGroupUi(playerController.unitSelectionManager.unitGroupThree, 3);
		}
		if (GroupNum == 4)
		{
			playerController.unitSelectionManager.unitGroupFour.Remove(this);
			playerController.gameUIManager.UpdateGroupUi(playerController.unitSelectionManager.unitGroupFour, 4);
		}
		if (GroupNum == 5)
		{
			playerController.unitSelectionManager.unitGroupFive.Remove(this);
			playerController.gameUIManager.UpdateGroupUi(playerController.unitSelectionManager.unitGroupFive, 5);
		}

		playerController.unitSelectionManager.RemoveDeadUnitFromSelectedUnits(this);
		playerController.unitListForPlayer.Remove(this);
	}
	public void ShowUnit()
	{
		miniMapRenderObj.layer = 13;
		//notifiy enemy player when enemy unit is spotted
	}
	public void HideUnit()
	{
		if (isPlayerOneUnit)
			miniMapRenderObj.layer = 11;

		else if (!isPlayerOneUnit)
			miniMapRenderObj.layer = 12;
		//notifiy enemy player when enemy unit is unspotted
	}
	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);
		Gizmos.DrawWireSphere(transform.position, ViewRange);
	}

	//STATE CHANGE FUNCTIONS
	public void ChangeStateIdle()
	{
		currentState = idleState;
		currentState.Enter(this);
	}
	public void ChangeStateMoving()
	{
		currentState = movingState;
		currentState.Enter(this);
	}
	public void ChangeStateAttacking()
	{
		currentState = attackState;
		currentState.Enter(this);
	}
}
