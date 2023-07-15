using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class TechTreeManager : MonoBehaviour
{
	[Header("UI Refs")]
	public GameObject techTemplate;
	public GameObject techTemplateEmpty;

	public GameObject buildingTechTreeParentObj;
	public GameObject unitTechTreeParentObj;

	[Header("Building Tech Tree Info")]
	public List<Technology> buildingTechList;
	public List<bool> hasResearchedBuildingTechlist;

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
		Debug.Log(index);

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

		UnlockNextResearch(techList, index);
	}
	public void UnlockNextResearch(List<Technology> techList, int index)
	{
		if (techList == buildingTechList) //unlock next building techs
		{
			if (index == 0 || index == 1)
			{
				techList[2].canBeReseached = true;
			}
			else if (index == 2)
			{
				techList[3].canBeReseached = true;
				techList[4].canBeReseached = true;
			}
			else if (index == 3 || index == 4)
			{
				techList[5].canBeReseached = true;
			}
			else if (index == 5)
			{
				techList[6].canBeReseached = true;
			}
		}
		else if (techList == unitTechList) //unlock next unit techs
		{
			if (index == 0 || index == 1)
			{
				techList[2].canBeReseached = true;
			}
			else if (index == 2)
			{
				techList[3].canBeReseached = true;
			}
			else if (index == 3)
			{
				techList[4].canBeReseached = true;
				techList[5].canBeReseached = true;
			}
			else if (index == 4)
			{
				techList[6].canBeReseached = true;
			}
			else if (index == 5)
			{
				techList[7].canBeReseached = true;
			}
		}
	}
	public bool CheckIfCanReseachTech(List<Technology> techList, int index)
	{
		if (!isCurrentlyReseaching && techList[index].hasResearched) //check if already researched
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
