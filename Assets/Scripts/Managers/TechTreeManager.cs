using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.UI.CanvasScaler;

public class TechTreeManager : MonoBehaviour
{
	public GameUIManager gameUIManager;

	[Header("UI Refs")]
	public GameObject techTemplate;
	public GameObject techTemplateEmpty;

	public GameObject buildingTechTreeParentObj;
	public GameObject unitTechTreeParentObj;

	[Header("Building Tech Tree Info")]
	public List<Technology> buildingTechList;
	public List<bool> hasResearchedBuildingTechlist;

	public float buildingHealthPercentageBonusValue;
	public float buildingArmourPercentageBonusValue;

	Technology buildingTechHealthOne = new Technology
	{
		TechName = "BuildingTech HealthOne",
		TechInfo = "Increases Health by 5%",
		TimeToResearchSec = 10,
		canBeReseached = true,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology buildingTechArmourOne = new Technology
	{
		TechName = "BuildingTech ArmourOne",
		TechInfo = "Increases Armour by 5%",
		TimeToResearchSec = 10,
		canBeReseached = true,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
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
	};
	Technology buildingTechHeavyMechs = new Technology
	{
		TechName = "BuildingTech HeavyMechs",
		TechInfo = "Unlocks Heavy Mechs",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology buildingTechVTOLS = new Technology
	{
		TechName = "BuildingTech VTOLS",
		TechInfo = "Unlocks Flying VTOL Gunships",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology buildingTechHealthTwo = new Technology
	{
		TechName = "BuildingTech HealthTwo",
		TechInfo = "Increases Health by 5%",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 2
	};
	Technology buildingTechArmourTwo = new Technology
	{
		TechName = "BuildingTech ArmourTwo",
		TechInfo = "Increases Armour by 5%",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = true,
		hasResearched = false,
		hasSpaceBetweenNextTech = 0
	};

	[Header("building Base Stats Info")]

	[Header("Unit Tech Tree Info")]
	public List<Technology> unitTechList;
	public List<bool> hasResearchedUnitTechlist;

	public float unitHealthPercentageBonusValue;
	public float unitArmourPercentageBonusValue;
	public float unitDamagePercentageBonusValue;
	public int unitAttackRangeBonusValue;
	public int unitSpeedBonusValue;

	Technology unitTechHealthOne = new Technology
	{
		TechName = "UnitTech HealthOne",
		TechInfo = "Increases Health by 5%",
		TimeToResearchSec = 10,
		canBeReseached = true,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology unitTechArmourOne = new Technology
	{
		TechName = "UnitTech ArmourOne",
		TechInfo = "Increases Armour by 5%",
		TimeToResearchSec = 10,
		canBeReseached = true,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology unitTechSpeedOne = new Technology
	{
		TechName = "UnitTech SpeedOne",
		TechInfo = "Increases Unit Speeds by 10MPH",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 2
	};
	Technology unitTechHealthTwo = new Technology
	{
		TechName = "UnitTech HealthTwo",
		TechInfo = "Increases Health by 5%",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology unitTechAttackRangeOne = new Technology
	{
		TechName = "UnitTech RangeOne",
		TechInfo = "Increases Range by 100M",
		TimeToResearchSec = 20,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology unitTechAttackDamageOne = new Technology
	{
		TechName = "UnitTech DamageOne",
		TechInfo = "Increases Damage by 5%",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 0
	};
	Technology unitTechAttackRangeTwo = new Technology
	{
		TechName = "UnitTech RangeTwo",
		TechInfo = "Increases Range by 100M",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 1
	};
	Technology unitTechAttackDamageTwo = new Technology
	{
		TechName = "UnitTech DamageTwo",
		TechInfo = "Increases Damage by 10%",
		TimeToResearchSec = 15,
		canBeReseached = false,
		isBuildingTech = false,
		hasResearched = false,
		hasSpaceBetweenNextTech = 0
	};

	[Header("Unit Base Stats Info")]

	[Header("stats")]
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
		UnityEngine.UI.Text TitleText = obj.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
		TitleText.text = tech.TechName;
		TitleText.color = new Color(0.8f, 0, 0, 1);

		UnityEngine.UI.Text infoText = obj.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>();
		infoText.text = tech.TechInfo;
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
				isCurrentlyReseaching = true;
				StartCoroutine(ResearchCountdownTimer(techList, index, buildingTechList[index].TimeToResearchSec, UiElement));
			}
		}
		if (techList == unitTechList)
		{
			if (CheckIfCanReseachTech(techList, index))
			{
				isCurrentlyReseaching = true;
				StartCoroutine(ResearchCountdownTimer(techList, index, unitTechList[index].TimeToResearchSec, UiElement));
			}
		}
	}
	public IEnumerator ResearchCountdownTimer(List<Technology> techList, int index, float researchTime, GameObject UiElement)
	{
		yield return new WaitForSeconds(researchTime);

		techList[index].hasResearched = true;
		isCurrentlyReseaching = false;
		GameManager.Instance.playerNotifsManager.DisplayNotifisMessage(techList[index].TechName + " Researched", 3f);
		UiElement.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().color = new Color(0, 0.8f, 0, 1);
		UiElement.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().color = new Color(0, 0.8f, 0, 1);

		CompleteResearch(techList, index);
		UnlockNextResearch(techList, index);
		ApplyTechUpgradesToExistingEntities();
	}
	public void CompleteResearch(List<Technology> techList, int index) //update bonus values provided by research
	{
		if (techList == buildingTechList)
		{
			if (index == 0)
			{
				buildingHealthPercentageBonusValue += 0.05f;
			}
			if (index == 1)
			{
				buildingArmourPercentageBonusValue += 0.05f;
			}
			if (index == 5)
			{
				buildingHealthPercentageBonusValue += 0.1f;
			}
			if (index == 6)
			{
				buildingArmourPercentageBonusValue += 0.1f;
			}
		}
		else if (techList == unitTechList)
		{
			if (index == 0)
			{
				unitHealthPercentageBonusValue += 0.05f;
			}
			if (index == 1)
			{
				unitArmourPercentageBonusValue += 0.05f;
			}
			if (index == 2)
			{
				unitSpeedBonusValue = 1;
			}
			if (index == 4)
			{
				unitAttackRangeBonusValue += 1;
			}
			if (index == 6)
			{
				unitAttackRangeBonusValue += 1;
			}
			if (index == 5)
			{
				unitDamagePercentageBonusValue += 0.05f;
			}
			if (index == 7)
			{
				unitDamagePercentageBonusValue += 0.1f;
			}
		}
	}
	public void UnlockNextResearch(List<Technology> techList, int index) //unlock next techs in respective trees
	{
		if (techList == buildingTechList)
		{
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
				GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Researching Tech", 1.5f);
				return true;
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
	public void UpdateUiTextColour(int index, List<Technology> techList, int techListIndex)
	{
		Debug.Log(techList[techListIndex].hasResearched);

		if (techList[techListIndex].canBeReseached && !techList[techListIndex].hasResearched)
		{
			Debug.Log("condition met");
			GameObject uiObj = unitTechTreeParentObj.transform.GetChild(index).gameObject;
			uiObj.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().color = new Color(0.8f, 0.8f, 0, 1);
			uiObj.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().color = new Color(0.8f, 0.8f, 0, 1);
		}
		else
		{
			Debug.Log("condition not met");
		}
	}

	//APPLY TECH UPGRADES TO ENTITIES
	public void ApplyTechUpgradesToNewUnits(GameObject unitObj)
	{
		UnitStateController unit = unitObj.GetComponent<UnitStateController>();

		unit.currentHealth = (int)(unit.currentHealth * unitHealthPercentageBonusValue);
		unit.maxHealth = (int)(unit.maxHealth * unitHealthPercentageBonusValue);
		unit.armour = (int)(unit.armour * unitArmourPercentageBonusValue);
		unit.weaponSystem.mainWeaponDamage *= unitDamagePercentageBonusValue;
		unit.weaponSystem.secondaryWeaponDamage *= unitDamagePercentageBonusValue;
		unit.attackRange += unitAttackRangeBonusValue;
		unit.agentNav.speed += unitSpeedBonusValue;
	}
	public void ApplyTechUpgradesToNewBuildings(GameObject buildingObj)
	{
		BuildingManager building = buildingObj.GetComponent<BuildingManager>();

		building.currentHealth = (int)(building.currentHealth * buildingHealthPercentageBonusValue);
		building.maxHealth = (int)(building.maxHealth * buildingHealthPercentageBonusValue);
		building.armour = (int)(building.armour * buildingArmourPercentageBonusValue);
	}
	//using list of all player units, first reset values to base then recalculate values
	public void ApplyTechUpgradesToExistingEntities()
	{
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
				unit.maxHealth = 1 + (int)(GameManager.Instance.buildingLightVehProdStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.buildingLightVehProdStats.armour * unitArmourPercentageBonusValue);
			}
			if (unit.entityName == "Heavy Mech Knight")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.buildingHeavyVehProdStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.buildingHeavyVehProdStats.armour * unitArmourPercentageBonusValue);
			}
			if (unit.entityName == "Heavy Mech Support")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.buildingVtolVehProdStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.buildingVtolVehProdStats.armour * unitArmourPercentageBonusValue);
			}
			if (unit.entityName == "VTOL Gunship")
			{
				unit.maxHealth = 1 + (int)(GameManager.Instance.buildingHQStats.health * unitHealthPercentageBonusValue);
				unit.armour = 1 + (int)(GameManager.Instance.buildingHQStats.armour * unitArmourPercentageBonusValue);
			}
		}
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
	}
}
