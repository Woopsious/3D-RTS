using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class TechTreeManager : NetworkBehaviour
{
	public GameUIManager gameUIManager;

	[Header("UI Refs")]
	public Technology currentReseachingTech;
	public Text currentResearchInfoText;
	public GameObject buildingTechTreeParentObj;
	public GameObject unitTechTreeParentObj;

	public GameObject techTemplate;
	public GameObject techTemplateEmpty;

	[Header("Building Tech Tree Info")]
	public List<Technology> buildingTechList;
	public List<bool> hasResearchedBuildingTechlist;

	Technology buildingTechHealthOne = new Technology
	{
		TechName = "BuildingTech HealthOne",
		TechInfo = "Increases Health by 10%",
		TimeToResearchSec = 10,
		canBeReseached = true,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 1000,
		techCostAlloys = 50,
		techCostCrystals = 10
	};
	Technology buildingTechArmourOne = new Technology
	{
		TechName = "BuildingTech ArmourOne",
		TechInfo = "Increases Armour by 10%",
		TimeToResearchSec = 10,
		canBeReseached = true,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 1000,
		techCostAlloys = 250,
		techCostCrystals = 100
	};
	Technology buildingTechRefineryBoost = new Technology
	{
		TechName = "BuildingTech RefineryBoost",
		TechInfo = "Increases Resource Income by 10%",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 5000,
		techCostAlloys = 0,
		techCostCrystals = 0
	};
	Technology buildingTechHeavyMechs = new Technology
	{
		TechName = "BuildingTech HeavyMechs",
		TechInfo = "Unlocks Heavy Mechs",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 1000,
		techCostAlloys = 50,
		techCostCrystals = 50
	};
	Technology buildingTechVTOLS = new Technology
	{
		TechName = "BuildingTech VTOLS",
		TechInfo = "Unlocks Flying VTOL Gunships",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 1000,
		techCostAlloys = 50,
		techCostCrystals = 50
	};
	Technology buildingTechHealthTwo = new Technology
	{
		TechName = "BuildingTech HealthTwo",
		TechInfo = "Increases Health by 15%",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 2,
		techCostMoney = 2500,
		techCostAlloys = 100,
		techCostCrystals = 25
	};
	Technology buildingTechArmourTwo = new Technology
	{
		TechName = "BuildingTech ArmourTwo",
		TechInfo = "Increases Armour by 15%",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 0,
		techCostMoney = 5000,
		techCostAlloys = 500,
		techCostCrystals = 250
	};

	[Header("building Base Stats Info")]

	[Header("Unit Tech Tree Info")]
	public List<Technology> unitTechList;
	public List<bool> hasResearchedUnitTechlist;

	Technology unitTechHealthOne = new Technology
	{
		TechName = "UnitTech HealthOne",
		TechInfo = "Increases Health by 5%",
		TimeToResearchSec = 10,
		canBeReseached = true,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 750,
		techCostAlloys = 50,
		techCostCrystals = 0
	};
	Technology unitTechArmourOne = new Technology
	{
		TechName = "UnitTech ArmourOne",
		TechInfo = "Increases Armour by 10%",
		TimeToResearchSec = 10,
		canBeReseached = true,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 1000,
		techCostAlloys = 50,
		techCostCrystals = 25
	};
	Technology unitTechSpeedOne = new Technology
	{
		TechName = "UnitTech SpeedOne",
		TechInfo = "Increases Unit Speeds by 10MPH",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 2,
		techCostMoney = 650,
		techCostAlloys = 0,
		techCostCrystals = 50
	};
	Technology unitTechHealthTwo = new Technology
	{
		TechName = "UnitTech HealthTwo",
		TechInfo = "Increases Health by 10%",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 2000,
		techCostAlloys = 100,
		techCostCrystals = 10
	};
	Technology unitTechAttackRangeOne = new Technology
	{
		TechName = "UnitTech RangeOne",
		TechInfo = "Increases Range by 100M",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 2500,
		techCostAlloys = 50,
		techCostCrystals = 100
	};
	Technology unitTechAttackDamageOne = new Technology
	{
		TechName = "UnitTech DamageOne",
		TechInfo = "Increases Damage by 5%",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 0,
		techCostMoney = 2500,
		techCostAlloys = 100,
		techCostCrystals = 50
	};
	Technology unitTechAttackRangeTwo = new Technology
	{
		TechName = "UnitTech RangeTwo",
		TechInfo = "Increases Range by 100M",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1,
		techCostMoney = 4000,
		techCostAlloys = 75,
		techCostCrystals = 200
	};
	Technology unitTechAttackDamageTwo = new Technology
	{
		TechName = "UnitTech DamageTwo",
		TechInfo = "Increases Damage by 10%",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 0,
		techCostMoney = 4000,
		techCostAlloys = 250,
		techCostCrystals = 50
	};

	[Header("STATS")]
	public bool isCurrentlyReseaching;

	//SET UP TECH TREE AND THE UI
	public void SetUpTechTrees()
	{
		SetUpBuildingTechTree();
		SetUpUnitTechTree();
	}
	public void SetUpBuildingTechTree()
	{
		buildingTechList = new List<Technology>
		{
			buildingTechHealthOne,
			buildingTechArmourOne,
			buildingTechRefineryBoost,
			buildingTechHeavyMechs,
			buildingTechVTOLS,
			buildingTechHealthTwo,
			buildingTechArmourTwo
		};

		foreach (Technology tech in buildingTechList)
			hasResearchedBuildingTechlist.Add(tech.hasResearched);

		for (int i = 0; i < buildingTechList.Count; i++)
		{
			GameObject go = Instantiate(techTemplate, buildingTechTreeParentObj.transform);
			SetUpTechTemplate(go, buildingTechList[i], i);

			if (buildingTechList[i].hasSpaceBetweenNextTech == 1)
			{
				Instantiate(techTemplateEmpty, buildingTechTreeParentObj.transform);
			}
			if (buildingTechList[i].hasSpaceBetweenNextTech == 2)
			{
				Instantiate(techTemplateEmpty, buildingTechTreeParentObj.transform);
				Instantiate(techTemplateEmpty, buildingTechTreeParentObj.transform);
			}
		}
	}
	public void SetUpUnitTechTree()
	{
		unitTechList = new List<Technology>
		{
			unitTechHealthOne,
			unitTechArmourOne,
			unitTechSpeedOne,
			unitTechHealthTwo,
			unitTechAttackRangeOne,
			unitTechAttackDamageOne,
			unitTechAttackRangeTwo,
			unitTechAttackDamageTwo
		};
		foreach (Technology tech in unitTechList)
			hasResearchedUnitTechlist.Add(tech.hasResearched);

		for (int i = 0; i < unitTechList.Count; i++)
		{
			GameObject go = Instantiate(techTemplate, unitTechTreeParentObj.transform);
			SetUpTechTemplate(go, unitTechList[i], i);

			if (unitTechList[i].hasSpaceBetweenNextTech == 1)
			{
				Instantiate(techTemplateEmpty, unitTechTreeParentObj.transform);
			}
			if (unitTechList[i].hasSpaceBetweenNextTech == 2)
			{
				Instantiate(techTemplateEmpty, unitTechTreeParentObj.transform);
				Instantiate(techTemplateEmpty, unitTechTreeParentObj.transform);
			}
		}
	}
	public void SetUpTechTemplate(GameObject obj, Technology tech, int index)
	{
		Text TitleText = obj.transform.GetChild(0).GetComponent<Text>();
		TitleText.text = tech.TechName;
		TitleText.color = new Color(0.8f, 0, 0, 1);

		Text infoText = obj.transform.GetChild(1).GetComponent<Text>();
		infoText.text = tech.TechInfo + "\n Research Cost: \n" + tech.techCostMoney + " Money, " + 
			tech.techCostAlloys + " Alloys, " + tech.techCostCrystals + " Crystals";
		infoText.color = new Color(0.8f, 0, 0, 1);

		if (tech.canBeReseached)
		{
			TitleText.color = new Color(0.8f, 0.8f, 0, 1);
			infoText.color = new Color(0.8f, 0.8f, 0, 1);
		}

		obj.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { SetUpReseachButton(tech, index, obj); });
	}
	public void SetUpReseachButton(Technology tech, int index, GameObject UiElement)
	{
		if (tech.isBuildingTech)
			ResearchTech(buildingTechList, index, UiElement);
		if (!tech.isBuildingTech)
			ResearchTech(unitTechList, index, UiElement);
	}

	//TECH RESEARCH FUNCTIONS
	public void ResearchTech(List<Technology> techList, int index, GameObject UiElement)
	{
		if (techList == buildingTechList)
		{
			if (CheckIfCanReseachTech(techList, index))
			{
				currentReseachingTech = techList[index];
				isCurrentlyReseaching = true;
				StartCoroutine(ResearchCountdownTimer(techList, index, buildingTechList[index].TimeToResearchSec, UiElement));
			}
		}
		if (techList == unitTechList)
		{
			if (CheckIfCanReseachTech(techList, index))
			{
				currentReseachingTech = techList[index];
				isCurrentlyReseaching = true;
				StartCoroutine(ResearchCountdownTimer(techList, index, unitTechList[index].TimeToResearchSec, UiElement));
			}
		}
		gameUIManager.playerController.EntityCostServerRPC(gameUIManager.playerController.isPlayerOne,
			techList[index].techCostMoney, techList[index].techCostAlloys, techList[index].techCostCrystals);
	}
	public IEnumerator ResearchCountdownTimer(List<Technology> techList, int index, float researchTime, GameObject UiElement)
	{
		yield return new WaitForSeconds(researchTime);

		techList[index].hasResearched = true;
		isCurrentlyReseaching = false;
		GameManager.Instance.playerNotifsManager.DisplayNotifisMessage(techList[index].TechName + " Researched", 3f);
		UiElement.transform.GetChild(0).GetComponent<Text>().color = new Color(0, 0.8f, 0, 1);
		UiElement.transform.GetChild(1).GetComponent<Text>().color = new Color(0, 0.8f, 0, 1);

		CompleteResearch(techList, index);
		UnlockNextResearch(techList, index);
		ApplyTechUpgradesToExistingEntities();
	}
	public void CompleteResearch(List<Technology> techList, int index) //update bonus values provided by research
	{
		if (techList == buildingTechList)
		{
			UpdateTechBonusesServerRPC(true, index);
		}
		else if (techList == unitTechList)
		{
			UpdateTechBonusesServerRPC(false, index);
		}
	}
	[ServerRpc(RequireOwnership = false)]
	public void UpdateTechBonusesServerRPC(bool isBuildingTech, int index, ServerRpcParams serverRpcParams = default)
	{
		if (isBuildingTech && serverRpcParams.Receive.SenderClientId == 0)
		{
			if (index == 0)
			{
				gameUIManager.gameManager.playerOneBuildingHealthPercentageBonus.Value += 0.1f;
			}
			if (index == 1)
			{
				gameUIManager.gameManager.playerOneBuildingArmourPercentageBonus.Value+= 0.1f;
			}
			if (index == 2)
			{
				gameUIManager.gameManager.playerOneBuildingBonusToResourceIncome.Value += 0.1f;
			}
			if (index == 3)
			{
				gameUIManager.gameManager.playerOneBuildingHasUnlockedHeavyMechs.Value = true;
			}
			if (index == 4)
			{
				gameUIManager.gameManager.playerOneBuildingHasUnlockedVtols.Value = true;
			}
			if (index == 5)
			{
				gameUIManager.gameManager.playerOneBuildingHealthPercentageBonus.Value += 0.15f;
			}
			if (index == 6)
			{
				gameUIManager.gameManager.playerOneBuildingArmourPercentageBonus.Value += 0.15f;
			}
		}
		else if (!isBuildingTech && serverRpcParams.Receive.SenderClientId == 0)
		{
			if (index == 0)
			{
				gameUIManager.gameManager.playerOneUnitHealthPercentageBonus.Value += 0.05f;
			}
			if (index == 1)
			{
				gameUIManager.gameManager.playerOneUnitArmourPercentageBonus.Value += 0.1f;
			}
			if (index == 2)
			{
				gameUIManager.gameManager.playerOneUnitSpeedBonus.Value = 1;
			}
			if (index == 3)
			{
				gameUIManager.gameManager.playerOneUnitHealthPercentageBonus.Value += 0.1f;
			}
			if (index == 4)
			{
				gameUIManager.gameManager.playerOneUnitAttackRangeBonus.Value += 1;
			}
			if (index == 6)
			{
				gameUIManager.gameManager.playerOneUnitAttackRangeBonus.Value += 1;
			}
			if (index == 5)
			{
				gameUIManager.gameManager.playerOneUnitDamagePercentageBonus.Value += 0.05f;
			}
			if (index == 7)
			{
				gameUIManager.gameManager.playerOneUnitDamagePercentageBonus.Value += 0.1f;
			}
		}
		else if (isBuildingTech && serverRpcParams.Receive.SenderClientId == 1)
		{
			if (index == 0)
			{
				gameUIManager.gameManager.playerTwoBuildingHealthPercentageBonus.Value += 0.1f;
			}
			if (index == 1)
			{
				gameUIManager.gameManager.playerTwoBuildingArmourPercentageBonus.Value += 0.1f;
			}
			if (index == 2)
			{
				gameUIManager.gameManager.playerTwoBuildingBonusToResourceIncome.Value += 0.1f;
			}
			if (index == 3)
			{
				gameUIManager.gameManager.playerTwoBuildingHasUnlockedHeavyMechs.Value = true;
			}
			if (index == 4)
			{
				gameUIManager.gameManager.playerTwoBuildingHasUnlockedVtols.Value = true;
			}
			if (index == 5)
			{
				gameUIManager.gameManager.playerTwoBuildingHealthPercentageBonus.Value += 0.15f;
			}
			if (index == 6)
			{
				gameUIManager.gameManager.playerTwoBuildingArmourPercentageBonus.Value += 0.15f;
			}
		}
		else if (!isBuildingTech && serverRpcParams.Receive.SenderClientId == 1)
		{
			if (index == 0)
			{
				gameUIManager.gameManager.playerTwoUnitHealthPercentageBonus.Value += 0.05f;
			}
			if (index == 1)
			{
				gameUIManager.gameManager.playerTwoUnitArmourPercentageBonus.Value += 0.1f;
			}
			if (index == 2)
			{
				gameUIManager.gameManager.playerTwoUnitSpeedBonus.Value = 1;
			}
			if (index == 3)
			{
				gameUIManager.gameManager.playerTwoUnitHealthPercentageBonus.Value += 0.1f;
			}
			if (index == 4)
			{
				gameUIManager.gameManager.playerTwoUnitAttackRangeBonus.Value += 1;
			}
			if (index == 6)
			{
				gameUIManager.gameManager.playerTwoUnitAttackRangeBonus.Value += 1;
			}
			if (index == 5)
			{
				gameUIManager.gameManager.playerTwoUnitDamagePercentageBonus.Value += 0.05f;
			}
			if (index == 7)
			{
				gameUIManager.gameManager.playerTwoUnitDamagePercentageBonus.Value += 0.1f;
			}
		}
	}
	public void UnlockNextResearch(List<Technology> techList, int index) //unlock next techs in respective trees
	{
		if (techList == buildingTechList)
		{
			Debug.Log("building tech list");
			if (index == 0 || index == 1)
			{
				techList[2].canBeReseached = true;
				UpdateUiTextColour(4, techList, 2);
			}
			else if (index == 2)
			{
				techList[3].canBeReseached = true;
				UpdateUiTextColour(6, techList, 3);

				techList[4].canBeReseached = true;
				UpdateUiTextColour(8, techList, 4);
			}
			else if (index == 3 || index == 4)
			{
				techList[5].canBeReseached = true;
				UpdateUiTextColour(10, techList, 5);
			}
			else if (index == 5)
			{
				techList[6].canBeReseached = true;
				UpdateUiTextColour(13, techList, 6);
			}
		}
		else if (techList == unitTechList)
		{
			Debug.Log("unit tech list");
			if (index == 0 || index == 1)
			{
				techList[2].canBeReseached = true;
				UpdateUiTextColour(4, techList, 2);
			}
			else if (index == 2)
			{
				techList[3].canBeReseached = true;
				UpdateUiTextColour(7, techList, 3);
			}
			else if (index == 3)
			{
				techList[4].canBeReseached = true;
				UpdateUiTextColour(9, techList, 4);

				techList[5].canBeReseached = true;
				UpdateUiTextColour(11, techList, 5);
			}
			else if (index == 4)
			{
				techList[6].canBeReseached = true;
				UpdateUiTextColour(12, techList, 6);
			}
			else if (index == 5)
			{
				techList[7].canBeReseached = true;
				UpdateUiTextColour(14, techList, 7);
			}
		}
	} 
	public bool CheckIfCanReseachTech(List<Technology> techList, int index)
	{
		if (!isCurrentlyReseaching && techList[index].hasResearched)
		{
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Technology Already Researched", 2f);
			return false;
		}
		else if (techList[index].canBeReseached)
		{
			if (!isCurrentlyReseaching)
			{
				if (gameUIManager.playerController.isPlayerOne) //check if can afford cost depending on if is player one or two/ai
				{
					int moneyCost = gameUIManager.gameManager.playerOneCurrentMoney.Value;
					int alloyCost = gameUIManager.gameManager.playerOneCurrentAlloys.Value;
					int crystalCost = gameUIManager.gameManager.playerOneCurrentCrystals.Value;

					if (techList[index].techCostMoney <= moneyCost && techList[index].techCostAlloys <= alloyCost && 
						techList[index].techCostCrystals <= crystalCost)
					{
						GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Researching Tech", 1.5f);
						return true;
					}
					else
					{
						GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Cant Afford To Research Tech", 2f);
						return false;
					}
				}
				else
				{
					int moneyCost = gameUIManager.gameManager.playerTwoCurrentMoney.Value;
					int alloyCost = gameUIManager.gameManager.playerTwoCurrentAlloys.Value;
					int crystalCost = gameUIManager.gameManager.playerTwoCurrentCrystals.Value;

					if (techList[index].techCostMoney <= moneyCost && techList[index].techCostAlloys <= alloyCost && 
						techList[index].techCostCrystals <= crystalCost)
					{
						GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Researching Tech", 1.5f);
						return true;
					}
					else
					{
						GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Cant Afford To Research Tech", 2f);
						return false;
					}
				}
			}
			else
			{
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Already Researching a Technology", 2f);
				return false;
			}
		}
		else
		{
			GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Research Atleast One Previous Tech", 2f);
			return false;
		}
	}
	//update ui color only if not already green (tech researched)
	public void UpdateUiTextColour(int TechUiindex, List<Technology> techList, int techListIndex)
	{
		if (techList[techListIndex].canBeReseached && !techList[techListIndex].hasResearched)
		{
			GameObject uiObj;
			if (techList == buildingTechList)
				uiObj = buildingTechTreeParentObj.transform.GetChild(TechUiindex).gameObject;
			else
				uiObj = unitTechTreeParentObj.transform.GetChild(TechUiindex).gameObject;

			uiObj.transform.GetChild(0).GetComponent<Text>().color = new Color(0.8f, 0.8f, 0, 1);
			uiObj.transform.GetChild(1).GetComponent<Text>().color = new Color(0.8f, 0.8f, 0, 1);
		}
	}

	//APPLY TECH UPGRADES TO ENTITIES
	[ServerRpc(RequireOwnership = false)]
	public void ApplyTechUpgradesToNewUnitsServerRPC(ulong unitNetworkedId)
	{
		UnitStateController unit = NetworkManager.SpawnManager.SpawnedObjects[unitNetworkedId].GetComponent<UnitStateController>();

		if (unit.isPlayerOneEntity) //apply tech upgrades to player one units using player one bonus tech
		{
			unit.currentHealth.Value = (int)(unit.currentHealth.Value * gameUIManager.gameManager.playerOneUnitHealthPercentageBonus.Value);
			unit.maxHealth.Value = (int)(unit.maxHealth.Value * gameUIManager.gameManager.playerOneUnitHealthPercentageBonus.Value);
			unit.armour.Value = (int)(unit.armour.Value * gameUIManager.gameManager.playerOneUnitArmourPercentageBonus.Value);

			if (unit.isUnitArmed)
			{
				unit.weaponSystem.mainWeaponDamage.Value *= gameUIManager.gameManager.playerOneUnitDamagePercentageBonus.Value;
				unit.weaponSystem.secondaryWeaponDamage.Value *= gameUIManager.gameManager.playerOneUnitDamagePercentageBonus.Value;
				unit.attackRange += gameUIManager.gameManager.playerOneUnitAttackRangeBonus.Value;
				if (!unit.isTurret)
					unit.agentNav.speed += gameUIManager.gameManager.playerOneUnitSpeedBonus.Value;
			}
		}
		if (!unit.isPlayerOneEntity) //apply tech upgrades to player two units using player two bonus tech
		{
			unit.currentHealth.Value = (int)(unit.currentHealth.Value * gameUIManager.gameManager.playerTwoUnitHealthPercentageBonus.Value);
			unit.maxHealth.Value = (int)(unit.maxHealth.Value * gameUIManager.gameManager.playerTwoUnitHealthPercentageBonus.Value);
			unit.armour.Value = (int)(unit.armour.Value * gameUIManager.gameManager.playerTwoUnitArmourPercentageBonus.Value);

			if (unit.isUnitArmed)
			{
				unit.weaponSystem.mainWeaponDamage.Value *= gameUIManager.gameManager.playerTwoUnitDamagePercentageBonus.Value;
				unit.weaponSystem.secondaryWeaponDamage.Value *= gameUIManager.gameManager.playerTwoUnitDamagePercentageBonus.Value;
				unit.attackRange += gameUIManager.gameManager.playerTwoUnitAttackRangeBonus.Value;
				if (!unit.isTurret)
					unit.agentNav.speed += gameUIManager.gameManager.playerTwoUnitSpeedBonus.Value;
			}
		}
	}
	[ServerRpc(RequireOwnership = false)]
	public void ApplyTechUpgradesToNewBuildingsServerRPC(ulong buildingNetworkedId)
	{
		Debug.LogWarning(buildingNetworkedId);

		if (NetworkManager.SpawnManager.SpawnedObjects[buildingNetworkedId].GetComponent<BuildingManager>() != null)
		{
			Debug.LogWarning("BuildingManager Script Found");
			BuildingManager building = NetworkManager.SpawnManager.SpawnedObjects[buildingNetworkedId].GetComponent<BuildingManager>();

			if (building.isPlayerOneEntity)
			{
				building.currentHealth.Value = (int)(building.currentHealth.Value *
					gameUIManager.gameManager.playerOneBuildingHealthPercentageBonus.Value);
				building.maxHealth.Value = (int)(building.maxHealth.Value * gameUIManager.gameManager.playerOneBuildingHealthPercentageBonus.Value);
				building.armour.Value = (int)(building.armour.Value * gameUIManager.gameManager.playerOneBuildingArmourPercentageBonus.Value);
			}
			if (!building.isPlayerOneEntity)
			{
				building.currentHealth.Value = (int)(building.currentHealth.Value *
					gameUIManager.gameManager.playerTwoBuildingHealthPercentageBonus.Value);
				building.maxHealth.Value = (int)(building.maxHealth.Value * gameUIManager.gameManager.playerTwoBuildingHealthPercentageBonus.Value);
				building.armour.Value = (int)(building.armour.Value * gameUIManager.gameManager.playerTwoBuildingArmourPercentageBonus.Value);
			}
		}
		Debug.LogWarning("BuildingManager Script Found");
		ApplyTechUpgradesToNewUnitsServerRPC(buildingNetworkedId);
	}

	//using list of all player units, first reset values to base then recalculate values
	public void ApplyTechUpgradesToExistingEntities()
	{
		/*
		foreach (BuildingManager building in gameUIManager.playerController.buildingListForPlayer)
		{
			if (building.entityName == "Energy Generator")
			{
				building.maxHealth = 1 + (int)(GameManager.Instance.buildingEnergyGenStats.health * buildingHealthPercentageBonusValue);
				building.armour = 1 + (int)(GameManager.Instance.buildingEnergyGenStats.armour * buildingArmourPercentageBonusValue);
			}
			if (building.entityName == "Refinery Building")
			{
				building.maxHealth = 1 + (int)(GameManager.Instance.buildingRefineryStats.health * buildingHealthPercentageBonusValue);
				building.armour = 1 + (int)(GameManager.Instance.buildingRefineryStats.armour * buildingArmourPercentageBonusValue);
			}
			if (building.entityName == "Light Vehicle Production Building")
			{
				building.maxHealth = 1 + (int)(GameManager.Instance.buildingLightVehProdStats.health * buildingHealthPercentageBonusValue);
				building.armour = 1 + (int)(GameManager.Instance.buildingLightVehProdStats.armour * buildingArmourPercentageBonusValue);
			}
			if (building.entityName == "Heavy Vehicle Production Building")
			{
				building.maxHealth = 1 + (int)(GameManager.Instance.buildingHeavyVehProdStats.health * buildingHealthPercentageBonusValue);
				building.armour = 1 + (int)(GameManager.Instance.buildingHeavyVehProdStats.armour * buildingArmourPercentageBonusValue);
			}
			if (building.entityName == "VTOL Production Pad")
			{
				building.maxHealth = 1 + (int)(GameManager.Instance.buildingVtolVehProdStats.health * buildingHealthPercentageBonusValue);
				building.armour = 1 + (int)(GameManager.Instance.buildingVtolVehProdStats.armour * buildingArmourPercentageBonusValue);
			}
			if (building.entityName == "Player HQ")
			{
				building.maxHealth = 1 + (int)(GameManager.Instance.buildingHQStats.health * buildingHealthPercentageBonusValue);
				building.armour = 1 + (int)(GameManager.Instance.buildingHQStats.armour * buildingArmourPercentageBonusValue);
			}

			building.UpdateHealthBar();
		}

		foreach (UnitStateController unit in gameUIManager.playerController.unitListForPlayer)
		{
			if (unit.entityName == "Scout Vehicle")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.unitScoutVehStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.unitScoutVehStats.armour * unitArmourPercentageBonusValue);
				unit.agentNav.speed = GameManager.Instance.unitScoutVehStats.speed + unitSpeedBonusValue;
			}
			if (unit.entityName == "Radar Vehicle")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.unitRadarVehStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.unitRadarVehStats.armour * unitArmourPercentageBonusValue);
				unit.agentNav.speed = GameManager.Instance.unitRadarVehStats.speed + unitSpeedBonusValue;
			}
			if (unit.entityName == "Light Mech")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.unitMechLightStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.unitMechLightStats.armour * unitArmourPercentageBonusValue);
				unit.weaponSystem.mainWeaponDamage = 1 + (int)(GameManager.Instance.unitMechLightStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.weaponSystem.secondaryWeaponDamage = 1 + (int)(GameManager.Instance.unitMechLightStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.attackRange = GameManager.Instance.unitMechLightStats.attackRange + unitAttackRangeBonusValue;
				unit.agentNav.speed = GameManager.Instance.unitMechLightStats.speed + unitSpeedBonusValue;
			}
			if (unit.entityName == "Heavy Mech Knight")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.unitMechHvyKnightStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.unitMechHvyKnightStats.armour * unitArmourPercentageBonusValue);
				unit.weaponSystem.mainWeaponDamage = 1 + (int)(GameManager.Instance.unitMechHvyKnightStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.weaponSystem.secondaryWeaponDamage = 1 + (int)(GameManager.Instance.unitMechHvyKnightStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.attackRange = GameManager.Instance.unitMechHvyKnightStats.attackRange + unitAttackRangeBonusValue;
				unit.agentNav.speed = GameManager.Instance.unitMechHvyKnightStats.speed + unitSpeedBonusValue;
			}
			if (unit.entityName == "Heavy Mech Support")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.unitMechHvyTankStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.unitMechHvyTankStats.armour * unitArmourPercentageBonusValue);
				unit.weaponSystem.mainWeaponDamage = 1 + (int)(GameManager.Instance.unitMechHvyTankStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.weaponSystem.secondaryWeaponDamage = 1 + (int)(GameManager.Instance.unitMechHvyTankStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.attackRange = GameManager.Instance.unitMechHvyTankStats.attackRange + unitAttackRangeBonusValue;
				unit.agentNav.speed = GameManager.Instance.unitMechHvyTankStats.speed + unitSpeedBonusValue;
			}
			if (unit.entityName == "VTOL Gunship")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.unitVtolGunshipStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.unitVtolGunshipStats.armour * unitArmourPercentageBonusValue);
				unit.weaponSystem.mainWeaponDamage = 1 + (int)(GameManager.Instance.unitVtolGunshipStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.weaponSystem.secondaryWeaponDamage = 1 + (int)(GameManager.Instance.unitVtolGunshipStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.attackRange = GameManager.Instance.unitVtolGunshipStats.attackRange + unitAttackRangeBonusValue;
				unit.agentNav.speed = GameManager.Instance.unitVtolGunshipStats.speed + unitSpeedBonusValue;
			}
			if (unit.entityName == "Turret")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.unitTurretStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.unitTurretStats.armour * unitArmourPercentageBonusValue);
				unit.weaponSystem.mainWeaponDamage = 1 + (int)(GameManager.Instance.unitTurretStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.weaponSystem.secondaryWeaponDamage = 1 + (int)(GameManager.Instance.unitTurretStats.mainWeaponDamage * unitDamagePercentageBonusValue);
				unit.attackRange = GameManager.Instance.unitTurretStats.attackRange + unitAttackRangeBonusValue;
				unit.agentNav.speed = GameManager.Instance.unitTurretStats.speed + unitSpeedBonusValue;
			}
			unit.UpdateHealthBar();
		}
		*/
	}

	[System.Serializable]
	public class Technology
	{
		public string TechName;
		public string TechInfo;
		public float TimeToResearchSec;
		public bool canBeReseached;
		public bool isBuildingTech;
		public bool hasResearched;
		public int hasSpaceBetweenNextTech;
		public int techCostMoney;
		public int techCostAlloys;
		public int techCostCrystals;
	}
}
