using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class TechTreeManager : MonoBehaviour
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
		GameManager.Instance.UpdateResourcesServerRPC(gameUIManager.playerController.isPlayerOne, true, false, false, false, 
			0, techList[index].techCostMoney, techList[index].techCostAlloys, techList[index].techCostCrystals);
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
	}
	public void CompleteResearch(List<Technology> techList, int index) //update bonus values provided by research
	{
		GameManager.Instance.playerNotifsManager.DisplayNotifisMessage("Research Complete", 2f);
		AnnouncerSystem.Instance.PlayAlertResearchCompleteSFX();

		if (techList == buildingTechList)
			gameUIManager.gameManager.UpdateTechBonusesServerRPC(true, index);
		else if (techList == unitTechList)
			gameUIManager.gameManager.UpdateTechBonusesServerRPC(false, index);
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
